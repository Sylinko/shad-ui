using System.Collections;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Metadata;
using Avalonia.Utilities;

namespace ShadUI;

/// <summary>
/// Presents literal items, an items source, and nested <see cref="CompositeCollection"/> instances
/// as one stable, observable list.
/// </summary>
/// <remarks>
/// <para>
/// Avalonia snapshots a plain <see cref="IEnumerable"/> used as an items source. An observable
/// source must also implement <see cref="IList"/>, otherwise <see cref="ItemsSourceView"/> rejects
/// it. This type therefore exposes a read-only flattened <see cref="IList"/> while
/// <see cref="Items"/> and <see cref="ItemsSource"/> provide the mutable inputs.
/// </para>
/// <para>
/// Unlike WPF, Avalonia has no collection-view factory that can provide a separate flattened view
/// for a composite collection. The flattened cache maintained here is consequently part of the
/// collection itself. It also gives controls produced by <see cref="ItemTemplate"/> stable
/// identity across Count, indexer, and enumeration calls.
/// </para>
/// <para>
/// This type deliberately derives from <see cref="StyledElement"/>. Compiled AXAML calls the
/// duck-typed <see cref="ProvideValue"/> method, which connects the otherwise detached collection
/// to the same context anchor that Avalonia's compiled bindings use. That connection lets
/// bindings on the collection use logical ancestors while controls materialized by the collection
/// receive the surrounding templated parent.
/// </para>
/// </remarks>
public sealed class CompositeCollection :
    StyledElement,
    IList,
    IReadOnlyList<object?>,
    INotifyCollectionChanged,
    IWeakEventSubscriber<AvaloniaPropertyChangedEventArgs>,
    IWeakEventSubscriber<LogicalTreeAttachmentEventArgs>
{
    private static readonly WeakEvent<StyledElement, LogicalTreeAttachmentEventArgs> ContextAnchorAttachedEvent =
        WeakEvent.Register<StyledElement, LogicalTreeAttachmentEventArgs>(
            (anchor, handler) => anchor.AttachedToLogicalTree += handler,
            (anchor, handler) => anchor.AttachedToLogicalTree -= handler);

    private static readonly WeakEvent<StyledElement, LogicalTreeAttachmentEventArgs> ContextAnchorDetachedEvent =
        WeakEvent.Register<StyledElement, LogicalTreeAttachmentEventArgs>(
            (anchor, handler) => anchor.DetachedFromLogicalTree += handler,
            (anchor, handler) => anchor.DetachedFromLogicalTree -= handler);

    /// <summary>
    /// Defines the <see cref="ItemsSource"/> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<CompositeCollection, IEnumerable?>(nameof(ItemsSource));

    /// <summary>
    /// Defines the <see cref="ItemTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<CompositeCollection, IDataTemplate?>(nameof(ItemTemplate));

    /// <summary>
    /// Gets the literal items in the collection.
    /// </summary>
    /// <remarks>
    /// This is the AXAML content property and follows the same mode rules as
    /// <see cref="ItemsControl.Items"/>. Once <see cref="ItemsSource"/> is non-null, attempts to
    /// mutate this collection throw until the items source is cleared.
    /// </remarks>
    [Content]
    public ItemCollection Items { get; }

    /// <summary>
    /// Gets or sets the enumerable used instead of literal <see cref="Items"/>.
    /// </summary>
    /// <remarks>
    /// Bind dynamic collections to this property rather than to <see cref="Items"/>. The internal
    /// <see cref="ItemCollection"/> owns source subscription and enforces the Items/ItemsSource
    /// mode transition in exactly the same way as Avalonia's items controls.
    /// </remarks>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to materialize non-composite data items.
    /// </summary>
    /// <remarks>
    /// The collection materializes the template itself because the flattened view must contain
    /// stable controls rather than the original data items. As a consequence, it also performs
    /// the item-container responsibility of assigning each source item as the generated control's
    /// data context.
    /// </remarks>
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets the number of items in the flattened view.
    /// </summary>
    public int Count
    {
        get
        {
            EnsureCache();
            return _flattenedItems.Count;
        }
    }

    /// <summary>
    /// Gets an item from the flattened view.
    /// </summary>
    /// <param name="index">The zero-based flattened index.</param>
    public object? this[int index]
    {
        get
        {
            EnsureCache();
            return _flattenedItems[index];
        }
    }

    object? IList.this[int index]
    {
        get => this[index];
        set => throw CreateReadOnlyException();
    }

    bool IList.IsFixedSize => false;

    bool IList.IsReadOnly => true;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    /// <summary>
    /// Occurs after the flattened view has changed.
    /// </summary>
    /// <remarks>
    /// The cache is updated before this event is raised, so Count and indexer access from an event
    /// handler observe the new state, matching the contract expected by Avalonia's item presenters.
    /// </remarks>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    private readonly List<object?> _flattenedItems = [];
    private readonly List<Segment> _segments = [];
    private bool _cacheInitialized;
    private bool _changingItemsSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeCollection"/> class.
    /// </summary>
    public CompositeCollection()
    {
        Items = CreateItemCollection();
        Items.CollectionChanged += HandleItemsChanged;
    }

    /// <summary>
    /// Determines whether the flattened view contains the specified item.
    /// </summary>
    public bool Contains(object? item)
    {
        EnsureCache();
        return _flattenedItems.Contains(item);
    }

    /// <summary>
    /// Returns the flattened index of the specified item, or -1 when it is absent.
    /// </summary>
    public int IndexOf(object? item)
    {
        EnsureCache();
        return _flattenedItems.IndexOf(item);
    }

    /// <summary>
    /// Returns an enumerator over the stable flattened cache.
    /// </summary>
    public IEnumerator<object?> GetEnumerator()
    {
        EnsureCache();
        return _flattenedItems.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    void ICollection.CopyTo(Array array, int index)
    {
        EnsureCache();
        ((ICollection)_flattenedItems).CopyTo(array, index);
    }

    int IList.Add(object? value) => throw CreateReadOnlyException();

    void IList.Clear() => throw CreateReadOnlyException();

    void IList.Insert(int index, object? value) => throw CreateReadOnlyException();

    void IList.Remove(object? value) => throw CreateReadOnlyException();

    void IList.RemoveAt(int index) => throw CreateReadOnlyException();

    /// <summary>
    /// Supplies this collection to compiled AXAML and connects it to the markup target.
    /// </summary>
    /// <param name="serviceProvider">The service provider supplied by compiled AXAML.</param>
    /// <returns>This collection.</returns>
    /// <remarks>
    /// <para>
    /// This method intentionally does not come from a public Avalonia interface or a
    /// <c>MarkupExtension</c> base class. The compiled AXAML loader recognizes the
    /// <c>ProvideValue(IServiceProvider)</c> shape and supplies <see cref="IProvideValueTarget"/>.
    /// </para>
    /// <para>
    /// Avalonia records a default binding anchor from
    /// <see cref="IAvaloniaXamlIlParentStackProvider"/> when a markup target is not itself a
    /// control. This method follows the same rule. A normal <see cref="StyledElement"/> target is
    /// used directly; a non-logical target such as <see cref="MenuFlyout"/> falls back to the
    /// closest control in the compiled AXAML parent stack.
    /// </para>
    /// <para>
    /// Merely assigning a logical parent during template construction is insufficient. The target
    /// is usually not rooted yet, and it does not list this non-visual helper in its own logical
    /// children. The collection therefore waits for the anchor to attach before setting its
    /// logical parent, and disconnects again when the anchor detaches. A nested collection is
    /// different: its containing <see cref="CompositeCollection"/> really owns it and records it
    /// in <see cref="StyledElement.LogicalChildren"/>, allowing Avalonia's normal recursive
    /// attach/detach notifications to do the work.
    /// </para>
    /// </remarks>
    // ReSharper disable once UnusedMember.Global
    public object ProvideValue(IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget provideValueTarget)
        {
            if (provideValueTarget.TargetObject is CompositeCollection compositeParent)
            {
                DisconnectLogicalParent();
                SetEffectiveTemplatedParent(null);
                compositeParent.AttachCompositeChild(this);
            }
            else
            {
                var contextAnchor = provideValueTarget.TargetObject as StyledElement ?? FindDefaultAnchor(serviceProvider);
                if (contextAnchor is not null)
                    ConnectContextAnchor(contextAnchor);
            }
        }

        return this;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            // ItemCollection raises a Remove followed by an Add while swapping sources. Those are
            // implementation details of the mode transition; consumers of the flattened view
            // should observe one atomic reset after the new source is installed.
            _changingItemsSource = true;
            try
            {
                SetItemsSource(Items, change.GetNewValue<IEnumerable?>());
            }
            finally
            {
                _changingItemsSource = false;
            }

            if (_cacheInitialized)
                RebuildCache(true);
        }
        else if (change.Property == ItemTemplateProperty && _cacheInitialized)
        {
            RebuildCache(true);
        }
        else if (change.Property == TemplatedParentProperty)
        {
            PropagateTemplatedParent(change.GetNewValue<AvaloniaObject?>());
        }
    }

    private void HandleItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_cacheInitialized || _changingItemsSource)
            return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add when TryHandleItemsAdded(e):
            case NotifyCollectionChangedAction.Remove when TryHandleItemsRemoved(e):
            case NotifyCollectionChangedAction.Replace when TryHandleItemsReplaced(e):
            case NotifyCollectionChangedAction.Move when TryHandleItemsMoved(e):
                return;
            default:
                RebuildCache(true);
                return;
        }
    }

    private void HandleChildCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not CompositeCollection child)
            return;

        var segmentIndex = _segments.FindIndex(x => ReferenceEquals(x.Child, child));
        if (segmentIndex < 0)
            return;

        var segment = _segments[segmentIndex];
        var flatOffset = GetFlatIndex(segmentIndex);

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add when
                e.NewItems is { } addedItems &&
                IsValidInsert(e.NewStartingIndex, addedItems.Count, segment.Items.Count):
            {
                var materialized = CopyItems(addedItems);
                segment.Items.InsertRange(e.NewStartingIndex, materialized);
                _flattenedItems.InsertRange(flatOffset + e.NewStartingIndex, materialized);
                RaiseCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        materialized,
                        flatOffset + e.NewStartingIndex));
                return;
            }
            case NotifyCollectionChangedAction.Remove when
                e.OldItems is { } removedItems &&
                IsValidRange(e.OldStartingIndex, removedItems.Count, segment.Items.Count):
            {
                var materialized = segment.Items.GetRange(e.OldStartingIndex, removedItems.Count);
                segment.Items.RemoveRange(e.OldStartingIndex, removedItems.Count);
                _flattenedItems.RemoveRange(flatOffset + e.OldStartingIndex, removedItems.Count);
                RaiseCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        materialized,
                        flatOffset + e.OldStartingIndex));
                return;
            }
            case NotifyCollectionChangedAction.Replace when
                e is { OldItems: { } replacedItems, NewItems: { } replacementItems } &&
                replacedItems.Count == replacementItems.Count &&
                IsValidRange(e.OldStartingIndex, replacedItems.Count, segment.Items.Count):
            {
                var oldMaterialized = segment.Items.GetRange(e.OldStartingIndex, replacedItems.Count);
                var newMaterialized = CopyItems(replacementItems);
                segment.Items.RemoveRange(e.OldStartingIndex, replacedItems.Count);
                segment.Items.InsertRange(e.OldStartingIndex, newMaterialized);
                _flattenedItems.RemoveRange(flatOffset + e.OldStartingIndex, replacedItems.Count);
                _flattenedItems.InsertRange(flatOffset + e.OldStartingIndex, newMaterialized);
                RaiseCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace,
                        newMaterialized,
                        oldMaterialized,
                        flatOffset + e.OldStartingIndex));
                return;
            }
            case NotifyCollectionChangedAction.Move when
                e.OldItems is { } movedItems &&
                IsValidMove(e.OldStartingIndex, e.NewStartingIndex, movedItems.Count, segment.Items.Count):
            {
                var materialized = segment.Items.GetRange(e.OldStartingIndex, movedItems.Count);
                segment.Items.RemoveRange(e.OldStartingIndex, movedItems.Count);
                segment.Items.InsertRange(e.NewStartingIndex, materialized);
                _flattenedItems.RemoveRange(flatOffset + e.OldStartingIndex, movedItems.Count);
                _flattenedItems.InsertRange(flatOffset + e.NewStartingIndex, materialized);
                RaiseCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Move,
                        materialized,
                        flatOffset + e.NewStartingIndex,
                        flatOffset + e.OldStartingIndex));
                return;
            }
            default:
                ResetChildSegment(segment, flatOffset);
                return;
        }
    }

    private bool TryHandleItemsAdded(NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not { } addedItems || !IsValidInsert(e.NewStartingIndex, addedItems.Count, _segments.Count))
            return false;

        var flatIndex = GetFlatIndex(e.NewStartingIndex);
        var segments = CreateSegments(addedItems);
        var materialized = Flatten(segments);
        _segments.InsertRange(e.NewStartingIndex, segments);
        _flattenedItems.InsertRange(flatIndex, materialized);

        if (materialized.Count > 0)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, materialized, flatIndex));
        }

        return true;
    }

    private bool TryHandleItemsRemoved(NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not { } removedItems || !IsValidRange(e.OldStartingIndex, removedItems.Count, _segments.Count))
            return false;

        var flatIndex = GetFlatIndex(e.OldStartingIndex);
        var segments = _segments.GetRange(e.OldStartingIndex, removedItems.Count);
        var materialized = Flatten(segments);
        DetachChildren(segments);
        _segments.RemoveRange(e.OldStartingIndex, removedItems.Count);
        _flattenedItems.RemoveRange(flatIndex, materialized.Count);

        if (materialized.Count > 0)
        {
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, materialized, flatIndex));
        }

        return true;
    }

    private bool TryHandleItemsReplaced(NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not { } replacedItems ||
            e.NewItems is not { } replacementItems ||
            !IsValidRange(e.OldStartingIndex, replacedItems.Count, _segments.Count))
            return false;

        var flatIndex = GetFlatIndex(e.OldStartingIndex);
        var oldSegments = _segments.GetRange(e.OldStartingIndex, replacedItems.Count);
        var oldMaterialized = Flatten(oldSegments);
        var newSegments = CreateSegments(replacementItems);
        var newMaterialized = Flatten(newSegments);

        DetachChildren(oldSegments);
        _segments.RemoveRange(e.OldStartingIndex, replacedItems.Count);
        _segments.InsertRange(e.OldStartingIndex, newSegments);
        _flattenedItems.RemoveRange(flatIndex, oldMaterialized.Count);
        _flattenedItems.InsertRange(flatIndex, newMaterialized);

        if (oldMaterialized.Count == 0 && newMaterialized.Count == 0)
            return true;

        if (oldMaterialized.Count != newMaterialized.Count)
        {
            RaiseReset();
            return true;
        }

        RaiseCollectionChanged(
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace,
                newMaterialized,
                oldMaterialized,
                flatIndex));
        return true;
    }

    private bool TryHandleItemsMoved(NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not { } movedItems || !IsValidMove(e.OldStartingIndex, e.NewStartingIndex, movedItems.Count, _segments.Count))
            return false;

        var oldFlatIndex = GetFlatIndex(e.OldStartingIndex);
        var segments = _segments.GetRange(e.OldStartingIndex, movedItems.Count);
        var materialized = Flatten(segments);
        _segments.RemoveRange(e.OldStartingIndex, movedItems.Count);
        _flattenedItems.RemoveRange(oldFlatIndex, materialized.Count);
        _segments.InsertRange(e.NewStartingIndex, segments);
        var newFlatIndex = GetFlatIndex(e.NewStartingIndex);
        _flattenedItems.InsertRange(newFlatIndex, materialized);

        if (materialized.Count > 0 && oldFlatIndex != newFlatIndex)
        {
            RaiseCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Move,
                    materialized,
                    newFlatIndex,
                    oldFlatIndex));
        }

        return true;
    }

    private void EnsureCache()
    {
        if (!_cacheInitialized)
            RebuildCache(false);
    }

    private void RebuildCache(bool raiseReset)
    {
        DetachChildren(_segments);
        _segments.Clear();
        _flattenedItems.Clear();

        foreach (var item in Items)
        {
            var segment = CreateSegment(item);
            _segments.Add(segment);
            _flattenedItems.AddRange(segment.Items);
        }

        _cacheInitialized = true;

        if (raiseReset)
            RaiseReset();
    }

    private Segment CreateSegment(object? item)
    {
        if (item is CompositeCollection child)
        {
            AttachCompositeChild(child);
            child.EnsureCache();
            child.CollectionChanged += HandleChildCollectionChanged;
            return new Segment(child, [.. child._flattenedItems]);
        }

        var materialized = item;
        if (ItemTemplate is { } dataTemplate && dataTemplate.Match(item))
        {
            materialized = dataTemplate.Build(item);

            // Normally ItemsControl.PrepareItemContainer assigns a non-control data item to the
            // generated container's DataContext. CompositeCollection has already replaced that
            // data item with the control returned by IDataTemplate.Build, so ItemsControl sees a
            // Control and intentionally skips that step. Perform it here to preserve normal data
            // template binding semantics.
            if (materialized is StyledElement styledElement)
                styledElement.DataContext = item;
        }

        ApplyTemplatedParent(materialized);
        return new Segment(null, [materialized]);
    }

    private List<Segment> CreateSegments(IList items)
    {
        var result = new List<Segment>(items.Count);
        foreach (var item in items) result.Add(CreateSegment(item));
        return result;
    }

    private void ResetChildSegment(Segment segment, int flatOffset)
    {
        _flattenedItems.RemoveRange(flatOffset, segment.Items.Count);
        segment.Items.Clear();
        segment.Child!.EnsureCache();
        segment.Items.AddRange(segment.Child._flattenedItems);
        _flattenedItems.InsertRange(flatOffset, segment.Items);
        RaiseReset();
    }

    /// <summary>
    /// Resolves the same lexical anchor used by Avalonia bindings when their immediate markup
    /// target is not a control.
    /// </summary>
    /// <remarks>
    /// The parent stack is valid only while <see cref="ProvideValue"/> is running, so the selected
    /// anchor is captured rather than retaining the service or its lazy enumerable. Avalonia
    /// prefers the nearest <see cref="Control"/> across the whole stack, then falls back to the
    /// nearest <see cref="IDataContextProvider"/>. Only a <see cref="StyledElement"/> can provide
    /// the logical-tree and templated-parent lifecycle required by this type.
    /// </remarks>
    private static StyledElement? FindDefaultAnchor(IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService(typeof(IAvaloniaXamlIlParentStackProvider)) is not IAvaloniaXamlIlParentStackProvider parentStack)
            return null;

        StyledElement? dataContextAnchor = null;

        foreach (var parent in parentStack.Parents)
        {
            if (parent is Control control)
                return control;

            if (dataContextAnchor is null && parent is StyledElement styledElement)
                dataContextAnchor = styledElement;
        }

        return dataContextAnchor;
    }

    /// <summary>
    /// Weakly observes the markup context whose logical ancestry and templated parent are projected
    /// onto this otherwise detached helper object.
    /// </summary>
    /// <remarks>
    /// No context anchor or subscription token is stored on the collection. Avalonia's
    /// <see cref="WeakEvent{TSender,TEventArgs}"/> keeps only a weak reference to this subscriber,
    /// so an anchor with a longer lifetime cannot retain a collection that has otherwise been
    /// released. The logical <see cref="StyledElement.Parent"/> is connected only while the anchor
    /// is rooted and is cleared by the corresponding weak detach notification.
    /// </remarks>
    private void ConnectContextAnchor(StyledElement anchor)
    {
        DisconnectLogicalParent();
        WeakEvents.AvaloniaPropertyChanged.Subscribe(anchor, this);
        ContextAnchorAttachedEvent.Subscribe(anchor, this);
        ContextAnchorDetachedEvent.Subscribe(anchor, this);
        SetEffectiveTemplatedParent(anchor.TemplatedParent);

        if (((ILogical)anchor).IsAttachedToLogicalTree)
            ConnectLogicalParent(anchor);
        else
            DisconnectLogicalParent();
    }

    void IWeakEventSubscriber<AvaloniaPropertyChangedEventArgs>.OnEvent(object? sender, WeakEvent ev, AvaloniaPropertyChangedEventArgs e)
    {
        if (ev == WeakEvents.AvaloniaPropertyChanged &&
            sender is StyledElement anchor &&
            e.Property == TemplatedParentProperty)
            SetEffectiveTemplatedParent(anchor.TemplatedParent);
    }

    void IWeakEventSubscriber<LogicalTreeAttachmentEventArgs>.OnEvent(object? sender, WeakEvent ev, LogicalTreeAttachmentEventArgs e)
    {
        if (sender is not StyledElement anchor)
            return;

        if (ev == ContextAnchorAttachedEvent)
        {
            SetEffectiveTemplatedParent(anchor.TemplatedParent);
            ConnectLogicalParent(anchor);
        }
        else if (ev == ContextAnchorDetachedEvent && ReferenceEquals(Parent, anchor))
        {
            DisconnectLogicalParent();
        }
    }

    private void ConnectLogicalParent(StyledElement anchor)
    {
        if (ReferenceEquals(Parent, anchor))
            return;

        DisconnectLogicalParent();
        ((ISetLogicalParent)this).SetParent(anchor);
    }

    private void DisconnectLogicalParent()
    {
        if (Parent is not null)
            ((ISetLogicalParent)this).SetParent(null);
    }

    /// <summary>
    /// Registers a nested composite as a real logical child of its containing composite.
    /// </summary>
    /// <remarks>
    /// Calling <see cref="ISetLogicalParent.SetParent"/> alone creates only the child-to-parent
    /// pointer. Avalonia propagates rooted logical-tree attachment by walking the parent's
    /// <see cref="StyledElement.LogicalChildren"/>, so maintaining the owning side of the
    /// relationship is essential for <c>$parent</c> bindings on nested collections to re-evaluate.
    /// Actual item controls are intentionally not added here; the receiving
    /// <see cref="ItemsControl"/> owns their logical-parent lifecycle when it realizes containers.
    /// </remarks>
    private void AttachCompositeChild(CompositeCollection child)
    {
        if (ReferenceEquals(child, this))
            throw new InvalidOperationException("A CompositeCollection cannot contain itself.");

        if (child.Parent is not null && !ReferenceEquals(child.Parent, this))
        {
            throw new InvalidOperationException(
                "A CompositeCollection cannot be attached to more than one logical parent.");
        }

        if (!LogicalChildren.Contains(child))
            LogicalChildren.Add(child);

        child.SetEffectiveTemplatedParent(TemplatedParent);
    }

    private void DetachCompositeChild(CompositeCollection child)
    {
        if (LogicalChildren.Contains(child))
            LogicalChildren.Remove(child);
    }

    /// <summary>
    /// Sets the shared templated parent for this composite subtree.
    /// </summary>
    /// <remarks>
    /// This is deliberately not a chain: nested composites and materialized controls all receive
    /// the same Avalonia object. Recursion is used only to traverse the non-visual composite
    /// subtree, matching <c>TemplatedControl.ApplyTemplatedParent</c>.
    /// </remarks>
    private void SetEffectiveTemplatedParent(AvaloniaObject? templatedParent) =>
        SetTemplatedParent(this, templatedParent);

    private void PropagateTemplatedParent(AvaloniaObject? templatedParent)
    {
        foreach (var logicalChild in LogicalChildren)
        {
            if (logicalChild is CompositeCollection compositeChild)
                compositeChild.SetEffectiveTemplatedParent(templatedParent);
        }

        if (!_cacheInitialized)
            return;

        foreach (var item in _flattenedItems)
            ApplyTemplatedParent(item);
    }

    private void ApplyTemplatedParent(object? item)
    {
        if (item is not StyledElement styledElement)
            return;

        SetTemplatedParent(styledElement, TemplatedParent);
    }

    private int GetFlatIndex(int segmentIndex)
    {
        var result = 0;
        for (var i = 0; i < segmentIndex; i++) result += _segments[i].Items.Count;
        return result;
    }

    private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e) =>
        CollectionChanged?.Invoke(this, e);

    private void RaiseReset() =>
        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

    private void DetachChildren(IEnumerable<Segment> segments)
    {
        foreach (var segment in segments)
        {
            if (segment.Child is not { } child)
                continue;

            child.CollectionChanged -= HandleChildCollectionChanged;
            DetachCompositeChild(child);
        }
    }

    private static List<object?> Flatten(IEnumerable<Segment> segments)
    {
        var result = new List<object?>();
        foreach (var segment in segments) result.AddRange(segment.Items);
        return result;
    }

    private static List<object?> CopyItems(IList items)
    {
        var result = new List<object?>(items.Count);
        foreach (var item in items) result.Add(item);
        return result;
    }

    private static bool IsValidInsert(int index, int count, int currentCount) =>
        index >= 0 && count >= 0 && index <= currentCount;

    private static bool IsValidRange(int index, int count, int currentCount) =>
        index >= 0 && count >= 0 && index <= currentCount - count;

    private static bool IsValidMove(int oldIndex, int newIndex, int count, int currentCount) =>
        IsValidRange(oldIndex, count, currentCount) && newIndex >= 0 && newIndex <= currentCount - count;

    private static NotSupportedException CreateReadOnlyException() =>
        new("The flattened CompositeCollection view is read-only. Modify Items or ItemsSource instead.");

    /// ItemCollection is public because controls expose it, but Avalonia keeps its constructor
    /// internal. Reusing it is intentional: it centralizes Items/ItemsSource mode switching,
    /// write locking, IList adaptation, and weak CollectionChanged subscription. Activator is
    /// preferable to copying those framework-private semantics into a second implementation.
    private static ItemCollection CreateItemCollection() =>
        (ItemCollection?)Activator.CreateInstance(typeof(ItemCollection), nonPublic: true) ??
        throw new InvalidOperationException("Unable to create Avalonia ItemCollection.");

    /// ItemCollection.SetItemsSource is the other internal half of the public ItemCollection API.
    /// UnsafeAccessor keeps the dependency explicit and compile-time checked without per-change
    /// reflection. This is deliberately coupled to the Avalonia implementation, just like the
    /// templated-parent hook below.
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SetItemsSource")]
    private static extern void SetItemsSource(ItemCollection collection, IEnumerable? value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_TemplatedParent")]
    private static extern void SetTemplatedParent(StyledElement element, AvaloniaObject? templatedParent);

    private sealed class Segment(CompositeCollection? child, List<object?> items)
    {
        public CompositeCollection? Child { get; } = child;

        public List<object?> Items { get; } = items;
    }
}
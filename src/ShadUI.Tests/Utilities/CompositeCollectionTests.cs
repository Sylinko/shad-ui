using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Xunit;

namespace ShadUI.Tests.Utilities;

public sealed class CompositeCollectionTests
{
    public CompositeCollectionTests()
    {
        AvaloniaTestFixture.EnsureInitialized();
    }

    [Fact]
    public void Items_WhenChanged_UpdatesFlattenedViewBeforeRaisingEvent()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var target = new CompositeCollection();
            target.Items.Add("first");
            Assert.Single(target);

            NotifyCollectionChangedEventArgs? received = null;
            var countDuringEvent = -1;
            object? itemDuringEvent = null;
            target.CollectionChanged += (_, e) =>
            {
                received = e;
                countDuringEvent = target.Count;
                itemDuringEvent = target[1];
            };

            target.Items.Add("second");

            Assert.NotNull(received);
            Assert.Equal(NotifyCollectionChangedAction.Add, received.Action);
            Assert.Equal(1, received.NewStartingIndex);
            Assert.Equal(2, countDuringEvent);
            Assert.Equal("second", itemDuringEvent);
        });
    }

    [Fact]
    public void ItemsSource_WhenAssigned_LocksItemsAndTracksSource()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var source = new ObservableCollection<object?> { "first" };
            var target = new CompositeCollection { ItemsSource = source };
            Assert.True(target.Items.IsReadOnly);
            Assert.Equal(["first"], target.ToArray());
            Assert.Throws<InvalidOperationException>(() => target.Items.Add("literal"));

            NotifyCollectionChangedEventArgs? received = null;
            target.CollectionChanged += (_, e) => received = e;

            source.Add("second");

            Assert.NotNull(received);
            Assert.Equal(NotifyCollectionChangedAction.Add, received.Action);
            Assert.Equal(1, received.NewStartingIndex);
            Assert.Equal(["first", "second"], target.ToArray());

            received = null;
            target.ItemsSource = null;

            Assert.NotNull(received);
            Assert.Equal(NotifyCollectionChangedAction.Reset, received.Action);
            Assert.False(target.Items.IsReadOnly);
            Assert.Empty(target);

            target.Items.Add("literal");
            Assert.Equal(["literal"], target.ToArray());
        });
    }

    [Fact]
    public void ItemsSource_WhenLiteralItemsExist_Throws()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var target = new CompositeCollection();
            target.Items.Add("literal");

            Assert.Throws<InvalidOperationException>(
                () => target.ItemsSource = new ObservableCollection<object?>());
        });
    }

    [Fact]
    public void ItemsSource_WhenReplaced_RaisesSingleReset()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var target = new CompositeCollection
            {
                ItemsSource = new ObservableCollection<object?> { "first" }
            };
            Assert.Single(target);

            var events = new List<NotifyCollectionChangedEventArgs>();
            target.CollectionChanged += (_, e) => events.Add(e);

            target.ItemsSource = new ObservableCollection<object?> { "second", "third" };

            var received = Assert.Single(events);
            Assert.Equal(NotifyCollectionChangedAction.Reset, received.Action);
            Assert.Equal(["second", "third"], target.ToArray());
        });
    }

    [Fact]
    public void ItemsSource_WhenMovedReplacedAndRemoved_PreservesEventSemantics()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var source = new ObservableCollection<object?> { "first", "second", "third" };
            var target = new CompositeCollection { ItemsSource = source };
            Assert.Equal(source, target);

            NotifyCollectionChangedEventArgs? received = null;
            target.CollectionChanged += (_, e) => received = e;

            source.Move(0, 2);

            Assert.NotNull(received);
            Assert.Equal(NotifyCollectionChangedAction.Move, received.Action);
            Assert.Equal(0, received.OldStartingIndex);
            Assert.Equal(2, received.NewStartingIndex);
            Assert.Equal(["second", "third", "first"], target.ToArray());

            received = null;
            source[1] = "replacement";

            Assert.NotNull(received);
            Assert.Equal(NotifyCollectionChangedAction.Replace, received.Action);
            Assert.Equal(1, received.NewStartingIndex);
            Assert.Equal(["second", "replacement", "first"], target.ToArray());

            received = null;
            source.RemoveAt(0);

            Assert.NotNull(received);
            Assert.Equal(NotifyCollectionChangedAction.Remove, received.Action);
            Assert.Equal(0, received.OldStartingIndex);
            Assert.Equal(["replacement", "first"], target.ToArray());
        });
    }

    [Fact]
    public void ItemTemplate_WhenEnumerated_ReusesMaterializedControl()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var template = new CountingTemplate();
            var target = new CompositeCollection { ItemTemplate = template };
            target.Items.Add("first");

            var fromIndexer = target[0];
            var fromFirstEnumeration = target.Single();
            var fromSecondEnumeration = target.Single();

            Assert.Same(fromIndexer, fromFirstEnumeration);
            Assert.Same(fromIndexer, fromSecondEnumeration);
            Assert.Equal(1, template.BuildCount);

            target.Items.Add("second");

            Assert.Same(fromIndexer, target[0]);
            Assert.Equal(2, template.BuildCount);
        });
    }

    [Fact]
    public void ItemTemplate_WhenMaterialized_AssignsSourceItemAsDataContext()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var sourceItem = "item";
            var target = new CompositeCollection { ItemTemplate = new CountingTemplate() };
            target.Items.Add(sourceItem);

            var materialized = Assert.IsType<TextBlock>(target.Single());

            Assert.Same(sourceItem, materialized.DataContext);
        });
    }

    [Fact]
    public void ChildCollection_WhenChanged_TranslatesFlattenedIndex()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var source = new ObservableCollection<object?> { "child-1" };
            var child = new CompositeCollection { ItemsSource = source };
            var target = new CompositeCollection();
            target.Items.Add("before");
            target.Items.Add(child);
            target.Items.Add("after");
            Assert.Equal(["before", "child-1", "after"], target.ToArray());

            NotifyCollectionChangedEventArgs? received = null;
            target.CollectionChanged += (_, e) => received = e;

            source.Add("child-2");

            Assert.NotNull(received);
            Assert.Equal(NotifyCollectionChangedAction.Add, received.Action);
            Assert.Equal(2, received.NewStartingIndex);
            Assert.Equal(["before", "child-1", "child-2", "after"], target.ToArray());
        });
    }

    [Fact]
    public void ItemTemplate_WhenChanged_RebuildsOnceAndRaisesReset()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var firstTemplate = new CountingTemplate();
            var secondTemplate = new CountingTemplate();
            var target = new CompositeCollection { ItemTemplate = firstTemplate };
            target.Items.Add("item");
            var firstControl = target.Single();

            var events = new List<NotifyCollectionChangedEventArgs>();
            target.CollectionChanged += (_, e) => events.Add(e);

            target.ItemTemplate = secondTemplate;

            Assert.Equal(1, secondTemplate.BuildCount);
            Assert.NotSame(firstControl, target.Single());
            Assert.Equal(NotifyCollectionChangedAction.Reset, Assert.Single(events).Action);
        });
    }

    [Fact]
    public void MenuFlyout_WhenCompositeSourceChanges_ForwardsCollectionEvent()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var source = new ObservableCollection<object?> { new MenuItem() };
            var composite = new CompositeCollection { ItemsSource = source };
            var flyout = new MenuFlyout { ItemsSource = composite };
            NotifyCollectionChangedEventArgs? received = null;
            flyout.Items.CollectionChanged += (_, e) => received = e;

            var added = new MenuItem();
            source.Add(added);

            Assert.NotNull(received);
            Assert.Equal(NotifyCollectionChangedAction.Add, received.Action);
            Assert.Equal(1, received.NewStartingIndex);
            Assert.Equal(2, flyout.Items.Count);
            Assert.Same(added, flyout.Items[1]);
        });
    }

    [Fact]
    public void ProvideValue_WithStyledTarget_TracksLogicalAndTemplatedParentLifecycle()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var anchor = new Border();
            var templatedParent = new Button();
            var target = new CompositeCollection();
            target.ProvideValue(new TestServiceProvider(anchor));

            Assert.Null(target.Parent);

            SetTemplatedParent(anchor, templatedParent);

            Assert.Same(templatedParent, target.TemplatedParent);

            var root = new Window { Content = anchor };

            Assert.Same(anchor, target.Parent);
            Assert.True(((ILogical)target).IsAttachedToLogicalTree);

            root.Content = null;

            Assert.Null(target.Parent);
            Assert.False(((ILogical)target).IsAttachedToLogicalTree);
            Assert.Same(templatedParent, target.TemplatedParent);

            root.Content = anchor;

            Assert.Same(anchor, target.Parent);
            Assert.True(((ILogical)target).IsAttachedToLogicalTree);

            root.Content = null;
        });
    }

    [Fact]
    public void ProvideValue_WithNonLogicalTarget_UsesCompiledXamlParentStackAnchor()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var anchor = new Border();
            var flyout = new MenuFlyout();
            var target = new CompositeCollection();
            target.ProvideValue(new TestServiceProvider(flyout, flyout, anchor));

            Assert.Null(target.Parent);

            var root = new Window { Content = anchor };

            Assert.Same(anchor, target.Parent);
            Assert.True(((ILogical)target).IsAttachedToLogicalTree);

            root.Content = null;
        });
    }

    [Fact]
    public void ProvideValue_WeakContextSubscription_DoesNotRetainCollection()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var anchor = new Border();
            var weakTarget = CreateWeaklySubscribedCollection(anchor);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.False(weakTarget.TryGetTarget(out _));
            GC.KeepAlive(anchor);
        });
    }

    [Fact]
    public void NestedComposite_WhenAnchorAttaches_ReevaluatesLogicalAncestorBinding()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var source = new ObservableCollection<object?> { "dynamic" };
            var anchor = new Border { Tag = source };
            var outer = new CompositeCollection();
            var inner = new CompositeCollection();
            inner.Bind(
                CompositeCollection.ItemsSourceProperty,
                new Binding("$parent[Border].Tag")
                {
                    TypeResolver = (_, name) => name == nameof(Border)
                        ? typeof(Border)
                        : throw new InvalidOperationException($"Unexpected type name '{name}'.")
                });

            inner.ProvideValue(new TestServiceProvider(outer, outer));
            outer.Items.Add(inner);
            Assert.Empty(outer);
            outer.ProvideValue(new TestServiceProvider(anchor));

            Assert.Same(outer, inner.Parent);
            Assert.Null(inner.ItemsSource);

            var root = new Window { Content = anchor };

            Assert.Same(anchor, outer.Parent);
            Assert.Same(outer, inner.Parent);
            Assert.True(((ILogical)inner).IsAttachedToLogicalTree);
            Assert.Same(source, inner.ItemsSource);
            Assert.Equal(["dynamic"], outer.ToArray());

            outer.Items.Remove(inner);

            Assert.Null(inner.Parent);
            Assert.False(((ILogical)inner).IsAttachedToLogicalTree);

            root.Content = null;
        });
    }

    [Fact]
    public void TemplatedParent_WhenResolvedLate_PropagatesSharedValueToNestedAndMaterializedItems()
    {
        AvaloniaTestFixture.RunOnUIThread(() =>
        {
            var anchor = new Border();
            var templatedParent = new Button();
            var item = new MenuItem();
            var inner = new CompositeCollection();
            inner.Items.Add(item);
            var outer = new CompositeCollection();
            inner.ProvideValue(new TestServiceProvider(outer, outer));
            outer.Items.Add(inner);
            Assert.Same(item, Assert.Single(outer));
            outer.ProvideValue(new TestServiceProvider(anchor));

            Assert.Null(outer.TemplatedParent);
            Assert.Null(inner.TemplatedParent);
            Assert.Null(item.TemplatedParent);

            SetTemplatedParent(anchor, templatedParent);

            Assert.Same(templatedParent, outer.TemplatedParent);
            Assert.Same(templatedParent, inner.TemplatedParent);
            Assert.Same(templatedParent, item.TemplatedParent);
        });
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_TemplatedParent")]
    private static extern void SetTemplatedParent(StyledElement element, AvaloniaObject? templatedParent);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<CompositeCollection> CreateWeaklySubscribedCollection(StyledElement anchor)
    {
        var target = new CompositeCollection();
        target.ProvideValue(new TestServiceProvider(anchor));
        return new WeakReference<CompositeCollection>(target);
    }

    private sealed class CountingTemplate : IDataTemplate
    {
        public int BuildCount { get; private set; }

        public Control Build(object? param)
        {
            BuildCount++;
            return new TextBlock { Text = param?.ToString() };
        }

        public bool Match(object? data) => data is string;
    }

    private sealed class TestServiceProvider : IServiceProvider, IProvideValueTarget, IAvaloniaXamlIlParentStackProvider
    {
        private readonly object[] _parents;

        public TestServiceProvider(object targetObject, params object[] parents)
        {
            TargetObject = targetObject;
            _parents = parents;
        }

        public object TargetObject { get; }

        public object TargetProperty => CompositeCollection.ItemsSourceProperty;

        public IEnumerable<object> Parents => _parents;

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IProvideValueTarget) ||
                serviceType == typeof(IAvaloniaXamlIlParentStackProvider))
                return this;

            return null;
        }
    }
}

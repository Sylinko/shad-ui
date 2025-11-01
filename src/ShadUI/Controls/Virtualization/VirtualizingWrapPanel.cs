using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;

namespace ShadUI.Controls;

/// <summary>
/// Virtualizing wrap panel with uniform-cell estimation and container recycling.
/// Horizontal: left-to-right, wrap to next row (vertical scrolling).
/// Vertical: top-to-bottom, wrap to next column (horizontal scrolling).
/// </summary>
public class VirtualizingWrapPanel : VirtualizingPanel
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        WrapPanel.OrientationProperty.AddOwner<VirtualizingWrapPanel>();

    public static readonly StyledProperty<double> ItemWidthProperty =
        WrapPanel.ItemWidthProperty.AddOwner<VirtualizingWrapPanel>();

    public static readonly StyledProperty<double> ItemHeightProperty =
        WrapPanel.ItemHeightProperty.AddOwner<VirtualizingWrapPanel>();

    public static readonly StyledProperty<double> CacheLengthProperty =
        AvaloniaProperty.Register<VirtualizingWrapPanel, double>(nameof(CacheLength), 0.5,
            validate: v => v is >= 0.0 and <= 2.0);

    private static readonly AttachedProperty<object?> RecycleKeyProperty =
        AvaloniaProperty.RegisterAttached<VirtualizingWrapPanel, Control, object?>("RecycleKey");

    private static readonly object ItemIsItsOwnContainer = new();

    private readonly Dictionary<int, Control> _realized = new();             // index -> control
    private readonly Dictionary<Control, int> _realizedReverse = new();       // control -> index
    private Dictionary<object, Stack<Control>>? _recyclePool;

    private Rect _viewport;
    private Rect _extendedViewport;
    private double _bufferFactor;
    private bool _isInLayout;
    private bool _isWaitingForViewportUpdate;

    private int _scrollToIndex = -1;
    private Control? _scrollToElement;

    private Control? _focusedElement;
    private int _focusedIndex = -1;

    private Size _estimatedCell = new(80, 80); // default estimation

    // Cached layout meta computed per measure/arrange pass
    private int _lineCapacity = 1; // columns (Horizontal) or rows (Vertical)
    private double _cellW;
    private double _cellH;

    public VirtualizingWrapPanel()
    {
        _bufferFactor = Math.Max(0.0, CacheLength);
        EffectiveViewportChanged += OnEffectiveViewportChanged;
        CacheLengthProperty.Changed.AddClassHandler<VirtualizingWrapPanel>((_, args) =>
        {
            _bufferFactor = Math.Max(0.0, args.NewValue as double? ?? 0.5);
            InvalidateMeasure();
        });
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public double CacheLength
    {
        get => GetValue(CacheLengthProperty);
        set => SetValue(CacheLengthProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var items = Items;
        if (items.Count == 0)
            return default;

        _isInLayout = true;
        try
        {
            ComputeCellEstimation();
            ComputeLineCapacity(availableSize, out var totalExtent);

            var (startIndex, endIndex) = ComputeRealizationRange(items.Count);
            RealizeRange(items, startIndex, endIndex, availableSize);

            // Desired size: constrained on the non-scrolling axis, extended on the scrolling axis
            return Orientation == Orientation.Horizontal
                ? new Size(availableSize.Width, totalExtent)
                : new Size(totalExtent, availableSize.Height);
        }
        finally
        {
            _isInLayout = false;
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_realized.Count == 0)
            return finalSize;

        _isInLayout = true;
        try
        {
            // Ensure cell/layout meta with finalized size
            ComputeCellEstimation();
            ComputeLineCapacity(finalSize, out _);

            foreach (var (index, element) in _realized.OrderBy(x => x.Key))
            {
                var rect = GetCellRect(index, finalSize);
                element.Arrange(rect);
            }

            // If we have a focused element parked in recycle, arrange it offscreen based on its estimated rect to keep BringIntoView/focus stable.
            if (_focusedElement != null && _focusedIndex >= 0)
            {
                var rect = GetCellRect(_focusedIndex, finalSize);
                _focusedElement.Arrange(rect);
            }

            return finalSize;
        }
        finally
        {
            _isInLayout = false;
        }
    }

    protected override void OnItemsChanged(IReadOnlyList<object?> items, NotifyCollectionChangedEventArgs e)
    {
        // Simple and safe: recycle everything on any change.
        RecycleAll();
        InvalidateMeasure();
    }

    protected override IInputElement? GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
    {
        var count = Items.Count;
        if (count == 0)
            return null;

        var index = from is Control c ? IndexFromContainer(c) : -1;
        var step1D = Orientation == Orientation.Horizontal ? _lineCapacity : 1;

        int target;
        switch (direction)
        {
            case NavigationDirection.First: target = 0; break;
            case NavigationDirection.Last: target = count - 1; break;
            case NavigationDirection.Next: target = Math.Min(count - 1, index + 1); break;
            case NavigationDirection.Previous: target = Math.Max(0, index - 1); break;

            case NavigationDirection.Left:
                target = Orientation == Orientation.Horizontal ? index - 1 : index - step1D;
                break;
            case NavigationDirection.Right:
                target = Orientation == Orientation.Horizontal ? index + 1 : index + step1D;
                break;
            case NavigationDirection.Up:
                target = Orientation == Orientation.Horizontal ? index - step1D : index - 1;
                break;
            case NavigationDirection.Down:
                target = Orientation == Orientation.Horizontal ? index + step1D : index + 1;
                break;
            default:
                return null;
        }

        if (target == index)
            return from;

        if (wrap)
        {
            if (target < 0) target = count - 1;
            if (target >= count) target = 0;
        }
        else
        {
            if (target < 0 || target >= count)
                return from;
        }

        return ScrollIntoView(target);
    }

    protected override IEnumerable<Control> GetRealizedContainers()
        => _realized.OrderBy(kv => kv.Key).Select(kv => kv.Value);

    protected override Control? ContainerFromIndex(int index)
    {
        if (index < 0 || index >= Items.Count)
            return null;

        if (_scrollToIndex == index)
            return _scrollToElement;
        if (_focusedIndex == index)
            return _focusedElement;
        if (_realized.TryGetValue(index, out var c))
            return c;

        // If item is its own container and was realized before, return it even if currently outside viewport
        var item = Items[index];
        if (item is Control itemCtrl &&
            Equals(itemCtrl.GetValue(RecycleKeyProperty), ItemIsItsOwnContainer))
            return itemCtrl;

        return null;
    }

    protected override int IndexFromContainer(Control container)
    {
        if (container == _scrollToElement)
            return _scrollToIndex;
        if (container == _focusedElement)
            return _focusedIndex;

        return _realizedReverse.GetValueOrDefault(container, -1);
    }

    protected override Control? ScrollIntoView(int index)
    {
        var items = Items;
        if (_isInLayout || index < 0 || index >= items.Count || !IsEffectivelyVisible)
            return null;

        var realized = ContainerFromIndex(index);
        if (realized != null)
        {
            realized.BringIntoView();
            return realized;
        }

        if (TopLevel.GetTopLevel(this) is not { } root)
            return null;

        // Create a temporary element, arrange at its estimated rect, bring into view, and force a layout pass.
        var element = GetOrCreateElement(items, index);
        element.Measure(Size.Infinity);

        // Compute with current bounds (best effort).
        var finalSize = Bounds.Size;
        ComputeCellEstimation();
        ComputeLineCapacity(finalSize, out _);
        var rect = GetCellRect(index, finalSize);
        element.Arrange(rect);

        _scrollToElement = element;
        _scrollToIndex = index;

        if (!Bounds.Contains(rect) && !_viewport.Contains(rect))
        {
            _isWaitingForViewportUpdate = true;
            root.UpdateLayout();
            _isWaitingForViewportUpdate = false;
        }

        element.BringIntoView();
        _isWaitingForViewportUpdate = !_viewport.Contains(rect);
        root.UpdateLayout();

        if (_isWaitingForViewportUpdate)
        {
            _isWaitingForViewportUpdate = false;
            InvalidateMeasure();
            root.UpdateLayout();
        }

        element.BringIntoView();
        _scrollToElement = null;
        _scrollToIndex = -1;
        return element;
    }

    private void ComputeCellEstimation()
    {
        // Prefer explicit item size; else estimate from realized elements.
        var w = ItemWidth > 0 ? ItemWidth : _estimatedCell.Width;
        var h = ItemHeight > 0 ? ItemHeight : _estimatedCell.Height;

        // If not explicitly set, update estimation from realized elements' DesiredSize average.
        if (ItemWidth <= 0 || ItemHeight <= 0)
        {
            double sumW = 0, sumH = 0; var n = 0;
            foreach (var c in _realized.Values)
            {
                if (c.IsMeasureValid)
                {
                    sumW += c.DesiredSize.Width;
                    sumH += c.DesiredSize.Height;
                    n++;
                }
            }
            if (n > 0)
            {
                if (ItemWidth <= 0) w = Math.Max(1, sumW / n);
                if (ItemHeight <= 0) h = Math.Max(1, sumH / n);
                _estimatedCell = new Size(w, h);
            }
        }

        _cellW = Math.Max(1, w);
        _cellH = Math.Max(1, h);
    }

    private void ComputeLineCapacity(Size viewportSize, out double totalExtent)
    {
        // Capacity per line in the flowing direction; total extent along the scrolling axis.
        if (Orientation == Orientation.Horizontal)
        {
            var availableW = Math.Max(1.0, viewportSize.Width);
            _lineCapacity = Math.Max(1, (int)Math.Floor(availableW / _cellW));
            var rows = (int)Math.Ceiling(Items.Count / (double)_lineCapacity);
            totalExtent = rows * _cellH;
        }
        else
        {
            var availableH = Math.Max(1.0, viewportSize.Height);
            _lineCapacity = Math.Max(1, (int)Math.Floor(availableH / _cellH));
            var cols = (int)Math.Ceiling(Items.Count / (double)_lineCapacity);
            totalExtent = cols * _cellW;
        }
    }

    private (int start, int end) ComputeRealizationRange(int itemCount)
    {
        if (itemCount == 0)
            return (0, -1);

        if (_lineCapacity <= 0)
            _lineCapacity = 1;

        if (Orientation == Orientation.Horizontal)
        {
            var viewTop = _extendedViewport.Top;
            var viewBottom = _extendedViewport.Bottom;

            var firstRow = Math.Max(0, (int)Math.Floor(viewTop / _cellH));
            var lastRow = Math.Max(firstRow, (int)Math.Floor((viewBottom - 1) / _cellH));

            var start = Math.Clamp(firstRow * _lineCapacity, 0, itemCount - 1);
            var end = Math.Clamp(((lastRow + 1) * _lineCapacity) - 1, 0, itemCount - 1);
            return (start, end);
        }
        else
        {
            var viewLeft = _extendedViewport.Left;
            var viewRight = _extendedViewport.Right;

            int firstCol = Math.Max(0, (int)Math.Floor(viewLeft / _cellW));
            int lastCol = Math.Max(firstCol, (int)Math.Floor((viewRight - 1) / _cellW));

            int start = Math.Clamp(firstCol * _lineCapacity, 0, itemCount - 1);
            int end = Math.Clamp(((lastCol + 1) * _lineCapacity) - 1, 0, itemCount - 1);
            return (start, end);
        }
    }

    private Rect GetCellRect(int index, Size finalSize)
    {
        if (_lineCapacity <= 0) _lineCapacity = 1;

        if (Orientation == Orientation.Horizontal)
        {
            int row = index / _lineCapacity;
            int col = index % _lineCapacity;

            double x = col * _cellW;
            double y = row * _cellH;

            double w = Math.Min(_cellW, Math.Max(0, finalSize.Width - x));
            double h = _cellH;

            return new Rect(x, y, w, h);
        }
        else
        {
            int col = index / _lineCapacity;
            int row = index % _lineCapacity;

            double x = col * _cellW;
            double y = row * _cellH;

            double w = _cellW;
            double h = Math.Min(_cellH, Math.Max(0, finalSize.Height - y));

            return new Rect(x, y, w, h);
        }
    }

    private void RealizeRange(IReadOnlyList<object?> items, int startIndex, int endIndex, Size availableSize)
    {
        // Recycle those outside [startIndex, endIndex]
        if (_realized.Count > 0)
        {
            var toRecycle = _realized.Keys.Where(i => i < startIndex || i > endIndex).ToArray();
            foreach (var idx in toRecycle)
            {
                var c = _realized[idx];
                RecycleElement(c, idx);
                _realized.Remove(idx);
                _realizedReverse.Remove(c);
            }
        }

        if (endIndex < startIndex)
            return;

        // Realize missing in [startIndex, endIndex]
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (_realized.ContainsKey(i))
                continue;

            var element = GetOrCreateElement(items, i);
            element.Measure(availableSize);

            _realized[i] = element;
            _realizedReverse[element] = i;
        }
    }

    private Control GetOrCreateElement(IReadOnlyList<object?> items, int index)
    {
        if (_realized.TryGetValue(index, out var e))
            return e;

        // Reuse special placeholders if matched
        var s = GetSpecialRealized(index, ref _focusedIndex, ref _focusedElement)
             ?? GetSpecialRealized(index, ref _scrollToIndex, ref _scrollToElement);

        if (s != null)
        {
            _realized[index] = s;
            _realizedReverse[s] = index;
            return s;
        }

        var item = items[index];
        if (ItemContainerGenerator?.NeedsContainer(item, index, out var recycleKey) is true)
        {
            return GetRecycledElement(item, index, recycleKey) ?? CreateElement(item, index, recycleKey);
        }

        return GetItemAsOwnContainer(item, index);
    }

    private static Control? GetSpecialRealized(int index, ref int specialIndex, ref Control? specialElement)
    {
        if (specialIndex != index)
            return null;
        var el = specialElement;
        specialIndex = -1;
        specialElement = null;
        return el;
    }

    private Control GetItemAsOwnContainer(object? item, int index)
    {
        var ctrl = (Control)item!;
        var gen = ItemContainerGenerator!;
        if (!ctrl.IsSet(RecycleKeyProperty))
        {
            gen.PrepareItemContainer(ctrl, ctrl, index);
            AddInternalChild(ctrl);
            ctrl.SetValue(RecycleKeyProperty, ItemIsItsOwnContainer);
            gen.ItemContainerPrepared(ctrl, item, index);
        }
        ctrl.SetCurrentValue(IsVisibleProperty, true);
        return ctrl;
    }

    private Control? GetRecycledElement(object? item, int index, object? recycleKey)
    {
        if (recycleKey == null)
            return null;

        if (_recyclePool == null || !_recyclePool.TryGetValue(recycleKey, out var stack) || stack.Count == 0)
            return null;

        var container = stack.Pop();
        container.SetCurrentValue(IsVisibleProperty, true);

        var gen = ItemContainerGenerator!;
        gen.PrepareItemContainer(container, item, index);
        gen.ItemContainerPrepared(container, item, index);
        AddIfNotInChildren(container);

        return container;
    }

    private Control CreateElement(object? item, int index, object? recycleKey)
    {
        var gen = ItemContainerGenerator!;
        var container = gen.CreateContainer(item, index, recycleKey);
        container.SetValue(RecycleKeyProperty, recycleKey);
        gen.PrepareItemContainer(container, item, index);
        AddInternalChild(container);
        gen.ItemContainerPrepared(container, item, index);
        return container;
    }

    private void AddIfNotInChildren(Control c)
    {
        if (!Children.Contains(c))
            AddInternalChild(c);
    }

    private void RecycleElement(Control element, int index)
    {
        var recycleKey = element.GetValue(RecycleKeyProperty);
        if (recycleKey == null)
        {
            RemoveInternalChild(element);
            return;
        }

        if (ReferenceEquals(recycleKey, ItemIsItsOwnContainer))
        {
            element.SetCurrentValue(IsVisibleProperty, false);
            return;
        }

        if (Equals(KeyboardNavigation.GetTabOnceActiveElement(ItemsControl!), element))
        {
            _focusedElement = element;
            _focusedIndex = index;
            return;
        }

        ItemContainerGenerator!.ClearItemContainer(element);
        PushToRecyclePool(recycleKey, element);
        element.SetCurrentValue(IsVisibleProperty, false);
    }

    private void PushToRecyclePool(object recycleKey, Control element)
    {
        _recyclePool ??= new Dictionary<object, Stack<Control>>();
        if (!_recyclePool.TryGetValue(recycleKey, out var stack))
        {
            stack = new Stack<Control>();
            _recyclePool.Add(recycleKey, stack);
        }
        stack.Push(element);
    }

    private void RecycleAll()
    {
        foreach (var kv in _realized.ToArray())
        {
            RecycleElement(kv.Value, kv.Key);
        }
        _realized.Clear();
        _realizedReverse.Clear();
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        // Update viewport and extended viewport with buffer on the scrolling axis
        _viewport = e.EffectiveViewport.Intersect(new Rect(Bounds.Size));
        _isWaitingForViewportUpdate = false;

        var buffer = (Orientation == Orientation.Horizontal ? _viewport.Height : _viewport.Width) * _bufferFactor;

        double start, end;
        if (Orientation == Orientation.Horizontal)
        {
            start = Math.Max(0, _viewport.Top - buffer);
            end = Math.Min(Bounds.Height, _viewport.Bottom + buffer);
            _extendedViewport = new Rect(_viewport.X, start, _viewport.Width, Math.Max(0, end - start));
        }
        else
        {
            start = Math.Max(0, _viewport.Left - buffer);
            end = Math.Min(Bounds.Width, _viewport.Right + buffer);
            _extendedViewport = new Rect(start, _viewport.Y, Math.Max(0, end - start), _viewport.Height);
        }

        InvalidateMeasure();
    }
}

namespace SquareCustomControl;

public partial class CustomControl : Grid
{
    private readonly GraphicsView _graphics;
    private bool _isLoaded = false;

    /// <summary>
    /// Actions that are 'always' drawn on invalidate that define the look of the control.
    /// </summary>
    private List<Action<ICanvas, RectF>> InternalDrawDocument { get; } = new List<Action<ICanvas, RectF>>();

    /// <summary>
    /// Actions that can be injected by the user.
    /// </summary>
    public List<Action<ICanvas, RectF>> UserDrawDocument { get; } = new List<Action<ICanvas, RectF>>();

    public CustomControl()
    {
        _graphics = new GraphicsView
        {
            Drawable = new InnerDrawable(this),
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
        };
        Children.Add(_graphics); // Add the GraphicsView to the control's visual tree

        PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(Window) && !_isLoaded)
            {
                _isLoaded = true;
                // Subscribe to page size changes
                if (this.FindAncestorOfType<ContentPage>() is ContentPage page)
                {
                    page.SizeChanged += OnPageSizeChanged;
                }
            }
        };
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (sender is ContentPage page)
        {
            double scaling = 1.0;
            int squareDimension = (int)(scaling * Math.Max(100, Math.Min(page.Height, page.Width)));
            HeightRequest = squareDimension;
            WidthRequest = squareDimension;

            Dispatcher.Dispatch(() =>
            {
                ClearDraw();
                var decr = (int)(WidthRequest / 40f);
                for (int radius = (int)WidthRequest / 2; radius > 0; radius -= decr)
                {
                    // This will 'not' work unless you capture
                    // the radius in a variable that exists just for this loop.
                    var loopScopeRadius = radius;
                    var drawAction = ((ICanvas canvas, RectF dirtyRect) =>
                    {
                        canvas.StrokeColor = Colors.Blue;
                        canvas.DrawCircle((float)(HeightRequest / 2), (float)(HeightRequest / 2), loopScopeRadius);
                    });
                    InternalDrawDocument.Add(drawAction);
                }
                _graphics.Invalidate();
            });
        }
    }

    private void DrawCircle(float centerX, float centerY, float radius, Color color)
    {
        var drawAction = ((ICanvas canvas, RectF dirtyRect) =>
        {
            canvas.StrokeColor = color;
            canvas.DrawCircle(centerX, centerY, radius);
        });
        InternalDrawDocument.Add(drawAction);
    }
    public void Refresh() =>_graphics.Invalidate();
    public void ClearDraw()
    {
        InternalDrawDocument.Clear();
    }

    private class InnerDrawable : IDrawable
    {
        private readonly CustomControl _parent;

        public InnerDrawable(CustomControl parent)
        {
            _parent = parent;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            foreach (var action in _parent.InternalDrawDocument)
            {
                action.Invoke(canvas, dirtyRect);
            }
            foreach (var action in _parent.UserDrawDocument)
            {
                action.Invoke(canvas, dirtyRect);
            }
            _parent.Draw?.Invoke(this, new DrawEventArgs(canvas,  dirtyRect));
        }
    }
    public event EventHandler<DrawEventArgs>? Draw;
    protected virtual void OnDraw(DrawEventArgs e)
    {
        Draw?.Invoke(this, e);
    }
}
public class DrawEventArgs : EventArgs
{
    public DrawEventArgs(ICanvas canvas, RectF dirtyRect)
    {
        Canvas = canvas;
        DirtyRect = dirtyRect;
    }

    public ICanvas Canvas { get; }
    public RectF DirtyRect { get; }
}

public static partial class Extensions
{
    public static T? FindAncestorOfType<T>(this Element element) where T : Element
    {
        while (element != null)
        {
            if (element is T correctType)
            {
                return correctType;
            }
            element = element.Parent;
        }

        return default;
    }
}

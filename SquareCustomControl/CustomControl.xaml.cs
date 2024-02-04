
using Microsoft.Maui.Graphics;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SquareCustomControl;

public partial class CustomControl : Grid
{
    private readonly GraphicsView _graphics;

    public CustomControl()
    {
        InitializeComponent();
        _graphics = new GraphicsView
        {
            Drawable = new InnerDrawable(this),
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
        };
        PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(Window))
            {
                if (!_isLoaded)
                {
                    _isLoaded = true;
                    // Check for null then subscribe to page size changes
                    if (this.FindAncestorOfType<ContentPage>() is ContentPage page)
                    {
                        page.SizeChanged -= localHandler; // Good habit...
                        page.SizeChanged += localHandler;
                        void localHandler(object? sender, EventArgs e)
                        {
                            double scaling = 1.0;
#if VERIFY_SCALING_WORKS
                            scaling = 0.75;
#endif
                            int squareDimension =
                                (int)(scaling * Math.Max(100, (Math.Min(page.Height, page.Width))));
                            HeightRequest = squareDimension;
                            WidthRequest = squareDimension;
                            _graphics.Invalidate();
                        }
                    }
                }
            }
        };
    }
    bool _isLoaded = false;
    private class InnerDrawable : IDrawable
    {
        public InnerDrawable(CustomControl parent)
        {
            _parent = parent;
        }
        private readonly CustomControl _parent;
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            _parent.Draw?.Invoke(this, new DrawEventArgs(canvas, dirtyRect));
            Debug.WriteLine($"Invalidated size is {dirtyRect.Size}");
        }
    }
    public event EventHandler<DrawEventArgs>? Draw;
    protected virtual void OnDraw(DrawEventArgs e)
    {
        Draw?.Invoke(this, e);
    }
    public class DrawEventArgs : EventArgs
    {
        public DrawEventArgs(ICanvas canvas, RectF dirtyRect)
        {
        }
    }
    public void DrawCircle(float centerX, float centerY, float radius, Color color, bool append = false)
    {
        var drawAction = new Action<ICanvas, RectF>((canvas, dirtyRect) =>
        {
            canvas.FillColor = color;
            canvas.FillCircle(centerX, centerY, radius);
        });

        if (!append)
        {
            _drawActions.Clear();
        }

        _drawActions.Enqueue(drawAction);
        _graphics.Invalidate();
    }
    private Queue<Action<ICanvas, RectF>> _drawActions = new Queue<Action<ICanvas, RectF>>();

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
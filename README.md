## Square Custom ControlOne approach you could experiment with is inheriting `Grid` to make a custom control that maintains an _internal_ `IDrawable`. There's little use in having the user of the control pass a canvas into the `CustomControl` in order to draw it. Now what you can do is make a new internal canvas whenever the control size changes.

As far as squaring it up, I tested an approach of subscribing to `ContentPage.SizeChanged` events by traversing up the visual tree to the root view. 


___

Here's  a proto I tested across platforms:

```csharp
public partial class CustomControl : Grid
{
    private readonly GraphicsView _graphics;
    private bool _isLoaded = false;
    private List<Action<ICanvas, RectF>> _drawDocument = new List<Action<ICanvas, RectF>>();

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

            ClearDraw();
            var decr = (int)(WidthRequest / 40f);
            for (int radius = (int)WidthRequest / 2; radius > 0; radius -= decr)
            {
                DrawCircle(
                    (float)(HeightRequest / 2),
                    (float)(HeightRequest / 2),
                    radius,
                    Colors.Blue);
            }
            _graphics.Invalidate();
        }
    }
```
##### Expose Draw Events
If you want to provide drawing, think of the custom control as a wrapper and do not expose the canvas itself.

    public void DrawCircle(float centerX, float centerY, float radius, Color color)
    {
        var drawAction = new Action<ICanvas, RectF>((canvas, dirtyRect) =>
        {
            canvas.StrokeColor = color;
            canvas.DrawCircle(centerX, centerY, radius);
        });
        _drawDocument.Add(drawAction);
    }
    public void Refresh() =>_graphics.Invalidate();
    public void ClearDraw()
    {
        _drawDocument.Clear();
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
            foreach (var action in _parent._drawDocument)
            {
                action.Invoke(canvas, dirtyRect);
            }
        }
    }
}
```




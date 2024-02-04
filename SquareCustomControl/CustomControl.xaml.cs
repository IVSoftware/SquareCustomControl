using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;

namespace SquareCustomControl;

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
            _graphics.Invalidate();
        }
    }

    public void DrawCircle(float centerX, float centerY, float radius, Color color, bool append = false)
    {
        var drawAction = new Action<ICanvas, RectF>((canvas, dirtyRect) =>
        {
            canvas.StrokeColor = color;
            canvas.DrawCircle(centerX, centerY, radius);
        });

        if (!append)
        {
            _drawDocument.Clear();
        }

        _drawDocument.Add(drawAction);
        _graphics.Invalidate(); // Trigger a redraw
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

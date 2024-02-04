## Square Custom Control
One approach you could experiment with is inheriting `Grid` to make a custom control that maintains an _internal_ `IDrawable` since there's little use in having the user of the control be required to pass a canvas into the `CustomControl` in order to draw it.  This paradigm loosely emulates what a Winforms control would do. You call `Refresh()` and the custom control responds with a Draw event that provides a canvas on which the user is free to draw. 

As far as squaring it up, I went about this by subscribing to `ContentPage.SizeChanged` events by traversing up the visual tree to the root view. Screenshots from Windows and Android physical devices are shown.

[![Win UI build][1]][1]
___

Here's  a proto I tested across platforms:

```csharp
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
 ```
 ##### InternalDrawDocument

 This is a list of `Action<ICanvas, RectF>` that constitute a 'Document' that will be drawn upon refresh.
 Here's where this particular control draws the standard 'concentric circles" that define its native appearance.
 ```
    protected virtual void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (sender is ContentPage page)
        {
            double scaling = 1.0;
            int squareDimension = (int)(scaling * Math.Max(100, Math.Min(page.Height, page.Width)));
            HeightRequest = squareDimension;
            WidthRequest = squareDimension;

            Dispatcher.Dispatch(() =>
            {
                InternalDrawDocument.Clear();
                UserDrawDocument.Clear();      // Maybe you 
                _graphics.Invalidate();
            });
            Dispatcher.Dispatch(() =>
            {
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

    public void Refresh() =>_graphics.Invalidate();
```
#### Inner IDrawable

Responds to IDrawable.Draw to paint the control, but also invokes the custom control's Draw event so that the user can make use of the canvas.

```
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
```

___

#### User Draw

Here is an example of the user injecting actions into the `UserDrawDocument` to be
included during refreshes. In this case it will animate a horizontal centered red line.

[![andoid screenshots][2]][2]

```
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }
    private void OnDrawClicked(object sender, EventArgs e)
    {
        Dispatcher.Dispatch(() =>
        {
            drawableControl.UserDrawDocument.Clear();
            drawableControl.Refresh();
        });
        Dispatcher.Dispatch(async() =>
        {
            for(float f1 = 100f, f2 = 105f; f2 < drawableControl.Width - 100f; f1 += 5f, f2 += 5f)
            {
                float loopScopeF1 = f1;
                float loopScopeF2 = f2;
                drawableControl.UserDrawDocument.Add((canvas, r) =>
                {
                    canvas.StrokeColor = Colors.Red;
                    canvas.DrawLine(loopScopeF1, (float)(drawableControl.Height / 2), loopScopeF2, (float)(drawableControl.Height / 2));
                });
                drawableControl.Refresh();
                await Task.Delay(1);
            }
        });
    }

    private void OnDrawClear(object sender, EventArgs e)
    {
        drawableControl.UserDrawDocument.Clear();
        drawableControl.Refresh();
    }
}
```

**MainPage xaml**

This shows the placement of the custom control on the main page.

```xaml
<ContentPage 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:local="clr-namespace:SquareCustomControl"
    x:Class="SquareCustomControl.MainPage"
    Shell.NavBarIsVisible="False">
    <ScrollView>
        <VerticalStackLayout
            Padding="30,0"
            Spacing="25"
            VerticalOptions="Center">
            <local:CustomControl
                x:Name="drawableControl"
                BackgroundColor="WhiteSmoke">
                <HorizontalStackLayout
                    HorizontalOptions="Center"
                    VerticalOptions="End">
                    <Button 
                        HeightRequest="50"
                        WidthRequest="150"
                        BackgroundColor="RoyalBlue" 
                        Text="Draw"
                        Clicked="OnDrawClicked"/>    
                    <Button 
                        HeightRequest="50"
                        WidthRequest="150"
                        BackgroundColor="LightSalmon" 
                        Text="Clear"
                        Clicked="OnDrawClear"/>                    
                </HorizontalStackLayout>
            </local:CustomControl>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

This is definitely a first pass, but I found your question interesting enough that I wanted to give it some thought and I hope it's useful in this rough form.

  [1]: https://i.stack.imgur.com/KCTcV.png
  [2]: https://i.stack.imgur.com/fjfci.png
```


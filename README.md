## Square Custom Control

I feel you are on the right track with controlling the squareness of the containing grid so that its correct dimensions go into the custom draw method. In fact if your use case allows, have the base class of the custom control be a `grid` in the first place. The idea is to experiment pulling the dimension into the custom control by subscribing to the `SizeChanged` event of the ancestral `ContentPage`. I tested this on Android, iPhone, iPad, and WinUI and it seems to work.

[![android screenshot][1]][1]

#### Custon Control
```csharp
public partial class CustomControl : Grid
{
	public CustomControl()
	{
		InitializeComponent();
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
                        }
                    }
                }
            }
        };
	}
    bool _isLoaded = false;
    public virtual void Draw(ICanvas canvas, RectF dirtyRect) { }
}
```
##### Uses Extension

For example, you can retieve an ancestor `ContentPage`.

```
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

**MainPage**

```xaml
<?xml version="1.0" encoding="utf-8" ?>
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
                BackgroundColor="Fuchsia">
                <Image
                    Source="dotnet_bot.png"
                    Aspect="AspectFit"
                    Margin="2"
                    SemanticProperties.Description="dot net bot in a race car number eight" 
                    BackgroundColor="White"/>
            </local:CustomControl>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

```csharp
public partial class MainPage : ContentPage
{
    public MainPage() => InitializeComponent();
}
```



  [1]: https://i.stack.imgur.com/8NWxp.png
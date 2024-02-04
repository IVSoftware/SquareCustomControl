## Square Custom Control

I feel you are on the right track with controlling the squareness of the containing grid so that its correct dimensions go into the custom draw method. 

___
*I'm trying to decide whether this is "that much different" from what you can do with normal layout grids. This may be a tad bit simpler, you decide.*
___

You could experiment with binding to the `SizeChanged` event of your `ContentView`. I tested this on Android, iPhone, iPad, and WinUI and it seems to work.

[![android screenshot][1]][1]


```csharp
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        SizeChanged += (sender, e) =>
        {
            BindingContext.SquareDimension =  
                (int)Math.Max(100, (Math.Min(Height, Width)));
        };
    }
    new MainPageBindingContext BindingContext => 
        (MainPageBindingContext)base.BindingContext;
}
class MainPageBindingContext : INotifyPropertyChanged
{
    public int SquareDimension
    {
        get => _squareDimension;
        set
        {
            if (!Equals(_squareDimension, value))
            {
                _squareDimension = value;
                OnPropertyChanged();
            }
        }
    }
    int _squareDimension = 100;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName]string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

___

What I tried is binding both the `HeightRequest` and the `WidthRequest` to the `SquareDimension` property.

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:local="clr-namespace:SquareCustomControl"
    x:Class="SquareCustomControl.MainPage"
    Shell.NavBarIsVisible="False">
    <ContentPage.BindingContext>
        <local:MainPageBindingContext/>
    </ContentPage.BindingContext>
    <ScrollView>
        <VerticalStackLayout
            Padding="30,0"
            Spacing="25"
            VerticalOptions="Center">
            <Grid
                HeightRequest="{Binding SquareDimension}"
                WidthRequest="{Binding SquareDimension}"
                BackgroundColor="Fuchsia">
                <Image
                    Source="dotnet_bot.png"
                    Aspect="AspectFit"
                    Margin="2"
                    SemanticProperties.Description="dot net bot in a race car number eight" 
                    BackgroundColor="White"/>
            </Grid>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```


  [1]: https://i.stack.imgur.com/8NWxp.png
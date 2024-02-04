using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SquareCustomControl
{
    public partial class MainPage : ContentPage
    {
        public MainPage() => InitializeComponent();

        private void OnDrawClicked(object sender, EventArgs e)
        {
            drawableControl.DrawCircle(100, 100, 50, Colors.Blue);
        }
    }
}

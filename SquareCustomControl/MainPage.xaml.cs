using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SquareCustomControl
{
    public partial class MainPage : ContentPage
    {
        public MainPage() => InitializeComponent();

        private void OnDrawClicked(object sender, EventArgs e)
        {
            Dispatcher.Dispatch(() =>
            {
                for (int radius = (int)drawableControl.Width/2; radius > 0; radius -= 15)
                {
                    drawableControl.DrawCircle(
                        (float)(drawableControl.Width / 2),
                        (float)(drawableControl.Height / 2),
                        radius,
                        Colors.Blue);
                }
            });
        }
    }
}

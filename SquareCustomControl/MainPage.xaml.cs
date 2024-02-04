using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SquareCustomControl
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        private void OnDrawClicked(object sender, EventArgs e)
        {
            drawableControl.UserDrawDocument.Add((canvas, r) =>
            {
                canvas.StrokeColor = Colors.Red;
                canvas.DrawLine(100f, (float)(drawableControl.Height / 2), 200f, (float)(drawableControl.Height / 2));
            });
            drawableControl.Refresh();
        }

        private void OnDrawClear(object sender, EventArgs e)
        {
            drawableControl.UserDrawDocument.Clear();
            drawableControl.Refresh();
        }
    }
}

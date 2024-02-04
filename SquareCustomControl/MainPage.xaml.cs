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
}

﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SquareCustomControl
{
    public partial class MainPage : ContentPage
    {
        public MainPage() => InitializeComponent();

        private void OnDrawClicked(object sender, EventArgs e)
        {
            drawableControl.DrawCircle(
                (float)(drawableControl.Width / 2),
                (float)(drawableControl.Height / 2), 
                _radius -= 5, 
                Colors.Blue);
        }
        int _radius = 100;
    }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SquareCustomControl
{
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
        new MainPageBindingContext BindingContext => (MainPageBindingContext)base.BindingContext;
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
}

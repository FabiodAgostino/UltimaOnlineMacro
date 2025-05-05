// SplashScreen.xaml.cs
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace UltimaOnlineMacro
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        public void UpdateStatus(string message)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = message;
            });
        }

        public async Task CloseSplashScreen()
        {
            await Dispatcher.Invoke(async () =>
            {
                var storyboard = (Storyboard)FindResource("FadeOut");
                storyboard.Completed += (s, e) => this.Close();
                storyboard.Begin(this);
            });
        }
    }
}
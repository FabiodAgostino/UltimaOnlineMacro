using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LogManager;

namespace UltimaOnlineMacro
{
    public partial class MacrocheckNotificationWindow : Window
    {
        private DispatcherTimer _timer;
        private int _remainingSeconds;

        public MacrocheckNotificationWindow(int countdownSeconds)
        {
            InitializeComponent();

            // Debug: verifica che gli elementi siano accessibili
            this.SourceInitialized += (s, e) =>
            {
                Logger.Loggin($"ProgressBar: {ProgressBar != null}");
                Logger.Loggin($"Grid Elements: {((Grid)MainBorder.Child).Children.Count}");
            };

            // Posiziona la finestra dopo che è stata caricata
            this.Loaded += Window_Loaded;

            // Configura il timer
            _remainingSeconds = countdownSeconds;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            // Aggiorna il display iniziale
            UpdateDisplay();

            // Avvia il timer
            _timer.Start();

            // Avvia l'animazione della barra di progresso
            StartProgressBarAnimation();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PositionOnSameScreenAsMainWindow();
        }

        private void PositionOnSameScreenAsMainWindow()
        {
            try
            {
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWindow != null && mainWindow.IsVisible)
                {
                    // Ottieni lo schermo su cui si trova la MainWindow
                    var handle = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;
                    var screen = System.Windows.Forms.Screen.FromHandle(handle);

                    var workArea = screen.WorkingArea;

                    // Calcola la posizione DPI-aware
                    var dpi = VisualTreeHelper.GetDpi(this);
                    var scale = dpi.DpiScaleX;

                    // Posiziona in basso a destra con padding
                    this.Left = (workArea.Right - this.Width * scale - 20) / scale;
                    this.Top = (workArea.Bottom - this.Height * scale - 20) / scale;
                }
                else
                {
                    // Fallback: usa il monitor primario
                    var screen = System.Windows.Forms.Screen.PrimaryScreen;
                    var workArea = screen.WorkingArea;
                    var dpi = VisualTreeHelper.GetDpi(this);
                    var scale = dpi.DpiScaleX;

                    this.Left = (workArea.Right - this.Width * scale - 20) / scale;
                    this.Top = (workArea.Bottom - this.Height * scale - 20) / scale;
                }
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore nel posizionamento: {ex.Message}");
                // Fallback semplice
                var workArea = SystemParameters.WorkArea;
                this.Left = workArea.Right - this.Width - 20;
                this.Top = workArea.Bottom - this.Height - 20;
            }
        }

        private void StartProgressBarAnimation()
        {
            // Assicurati che il ProgressBar sia visibile
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.Height = 8;

            // Crea un'animazione per la barra di progresso che dura quanto il countdown
            var storyboard = new System.Windows.Media.Animation.Storyboard();
            var animation = new System.Windows.Media.Animation.DoubleAnimation();
            animation.From = 100;
            animation.To = 0;
            animation.Duration = new Duration(TimeSpan.FromSeconds(_remainingSeconds));

            System.Windows.Media.Animation.Storyboard.SetTarget(animation, ProgressBar);
            System.Windows.Media.Animation.Storyboard.SetTargetProperty(animation,
                new PropertyPath(System.Windows.Controls.ProgressBar.ValueProperty));

            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _remainingSeconds--;

            if (_remainingSeconds <= 0)
            {
                _timer.Stop();
                AnimateAndClose();
            }
            else
            {
                UpdateDisplay();

                // Aggiungi effetti su tempi specifici
                if (_remainingSeconds <= 10)
                {
                    // Gli ultimi 10 secondi: countdown verde
                    CountdownText.Foreground = (System.Windows.Media.Brush)FindResource("AccentBrush");

                    // Aumenta la velocità della pulsazione
                    var pulse = (System.Windows.Media.Animation.Storyboard)FindResource("CountdownPulse");
                    pulse.SpeedRatio = 2;
                }
            }
        }

        private void UpdateDisplay()
        {
            var timeSpan = TimeSpan.FromSeconds(_remainingSeconds);
            CountdownText.Text = string.Format("{0:mm\\:ss}", timeSpan);

            // Aggiorna il tooltip con informazioni dettagliate
            this.ToolTip = $"Tempo rimanente: {_remainingSeconds} secondi\nNon chiudere questa finestra";
        }

        private void AnimateAndClose()
        {
            // Animazione di chiusura
            var fadeOut = new System.Windows.Media.Animation.Storyboard();

            var opacityAnimation = new System.Windows.Media.Animation.DoubleAnimation();
            opacityAnimation.From = 1;
            opacityAnimation.To = 0;
            opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));

            var scaleTransform = new System.Windows.Media.ScaleTransform(1, 1);
            MainBorder.RenderTransform = scaleTransform;
            MainBorder.RenderTransformOrigin = new Point(0.5, 0.5);

            var scaleAnimation = new System.Windows.Media.Animation.DoubleAnimation();
            scaleAnimation.From = 1;
            scaleAnimation.To = 0.8;
            scaleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));

            System.Windows.Media.Animation.Storyboard.SetTarget(opacityAnimation, MainBorder);
            System.Windows.Media.Animation.Storyboard.SetTargetProperty(opacityAnimation,
                new PropertyPath(UIElement.OpacityProperty));

            System.Windows.Media.Animation.Storyboard.SetTarget(scaleAnimation, scaleTransform);
            System.Windows.Media.Animation.Storyboard.SetTargetProperty(scaleAnimation,
                new PropertyPath(System.Windows.Media.ScaleTransform.ScaleXProperty));

            fadeOut.Children.Add(opacityAnimation);
            fadeOut.Children.Add(scaleAnimation);

            fadeOut.Completed += (s, e) => this.Close();
            fadeOut.Begin();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _timer?.Stop();
        }
    }
}
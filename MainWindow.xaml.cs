using AutoClicker.Models.System;
using AutoClicker.Models.TM;
using AutoClicker.Service;
using LogManager;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using UltimaOnlineMacro.Service;
using Rectangle = System.Drawing.Rectangle;

namespace UltimaOnlineMacro
{
    public partial class MainWindow : Window
    {
        private Regions Regions = new Regions();
        public Pg Pg;
        private MainWindowService _mainWindowService;

        public MainWindow()
        {
            InitializeComponent();
            Logger._logTextBox = txtLog;
            Pg = new Pg();
            SavedImageTemplate.Initialize();
            cmbKey.ItemsSource = AutoClicker.Service.ExtensionMethod.Key.PopolaComboKey();
            cmbKey.SelectedIndex = 0;
            _mainWindowService = new MainWindowService(this);
        }

        #region Callback

        public void SetBackpackRegion(Rectangle region)
        {
            if (Regions.BackpackRegion != region)
            {
                Regions.BackpackRegion = region;
                Logger.Loggin($"Regione zaino selezionata: {Regions.BackpackRegion}");
                try
                {
                    var pickaxe = new Pickaxe(region, SavedImageTemplate.ImageTemplatePickaxe);
                    Pg.PickaxeInBackpack = pickaxe;

                    if (pickaxe.IsFound)
                    {
                        Regions.Pickaxe = pickaxe;
                        btnSelectBackpack.Background = System.Windows.Media.Brushes.Gray;
                    }
                    else
                        Regions.Pickaxe = null;
                }
                catch (Exception e)
                {
                    Logger.Loggin($"Errore: {e.Message}");
                }
            }
        }

        public void SetStatus(Rectangle region)
        {
            var t = new TestService();
            t.MeasureExecutionTime(region);
            Regions.StatusRegion = region;
        }

        public void PaperdollHavePickaxe(Rectangle region)
        {
            // Calcola metà larghezza e metà altezza
            int newWidth = region.Width / 2;
            int newHeight = region.Height / 2;

            // Calcola le nuove coordinate per centrare il rettangolo
            int newX = region.X + (region.Width - newWidth);
            int newY = region.Y + (region.Height - newHeight);

            // Crea la nuova regione centrata
            Rectangle centeredRegion = new Rectangle(newX, newY, newWidth, newHeight);

            if (Regions.PaperdollRegion != region)
            {
                Regions.PaperdollRegion = region;
                Regions.PaperdollPickaxeRegion = centeredRegion;
                Logger.Loggin($"Regione paperdoll selezionata (centrata): {Regions.PaperdollRegion}");
                btnSelectPaperdoll.Background = System.Windows.Media.Brushes.Gray;
            }
        }

        #endregion Callback

        #region WindowsHelper

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private async void TestKeyboard_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            await Task.Delay(1000);
            var service = new SendInputService();
            await service.TestKeyboard();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private async void TestMouse_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            await Task.Delay(1000);
            var service = new SendInputService();
            await service.TestMouse();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        #endregion WindowsHelper

        #region ClickEvent

        private void SelectStatus_Click(object sender, RoutedEventArgs e)
        {
            OverlayWindow overlay = new OverlayWindow("Status");
            overlay.Show();
        }

        private void SelectBackpackRegion_Click(object sender, RoutedEventArgs e)
        {
            OverlayWindow overlay = new OverlayWindow("Piccone");
            overlay.Show();
        }

        private void SelectPaperdoll_Click(object sender, RoutedEventArgs e)
        {
            OverlayWindow overlay = new OverlayWindow("Paperdoll");
            overlay.Show();
        }

        private void StopBeep_Click(object sender, RoutedEventArgs e)
        {
            Pg.StopBeep();
        }

        private async void Run_Click(object sender, RoutedEventArgs e)
        {
            if (!Debugger.IsAttached)
            {
                string haveValue = Regions.HaveValue();
                if (!String.IsNullOrEmpty(haveValue))
                {
                    Logger.Loggin(haveValue);
                    return;
                }
                string isReady = Pg.IsReady();
                if (!String.IsNullOrEmpty(isReady))
                {
                    Logger.Loggin(isReady);
                    return;
                }
                if (!chkMuloDaSoma.IsChecked.Value && !chkLamaPortatore.IsChecked.Value)
                {
                    Logger.Loggin("Seleziona almeno un animale da soma");
                    return;
                }
            }

            _mainWindowService.TimerUltima.Start();
            _mainWindowService.CheckMacroButtons();
            _mainWindowService.SetMuli();

            btnRun.Background = System.Windows.Media.Brushes.Gray;
            btnStop.Background = System.Windows.Media.Brushes.Red;
            Logger.Loggin("Run!");
            await Pg.Work(Regions);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _mainWindowService.TimerUltima.Stop();
            Pg.Stop();
            Pg.RunWork = false;
            btnRun.Background = System.Windows.Media.Brushes.Green;
            btnStop.Background = System.Windows.Media.Brushes.Red;
            Logger.Loggin("Stop!");
        }

        private async void SelectWater_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            await Pg.SetWater();
            this.WindowState = WindowState.Normal;
            this.Activate();
            btnSelectFood.Background = System.Windows.Media.Brushes.Gray;
        }

        private async void SelectFood_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            await Pg.SetFood();
            this.WindowState = WindowState.Normal;
            this.Activate();
            btnSelectWater.Background = System.Windows.Media.Brushes.Gray;
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new();
            openFileDialog.Title = "Seleziona un file di testo";
            string expectedFileName = DateTime.Now.ToString("yyyy_MM_dd") + "_journal.txt";

            openFileDialog.Filter = $"{expectedFileName}|{expectedFileName}";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(selectedFile);
                if (fileName.Equals(expectedFileName, StringComparison.OrdinalIgnoreCase))
                {
                    SelectedFilePathText.Text = $"File selezionato: {selectedFile}";
                    Pg.PathJuornalLog = selectedFile;
                }
                else
                {
                    System.Windows.MessageBox.Show($"Il file selezionato non corrisponde al formato richiesto ({expectedFileName}).", "File non valido", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        #endregion ClickEvent
    }
}
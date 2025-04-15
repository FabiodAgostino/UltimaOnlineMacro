// Questo programma è un'applicazione WPF che utilizza un hook di basso livello per intercettare i clic del mouse
// anche al di fuori della finestra principale, consentendo la selezione di regioni specifiche nella finestra di gioco.
// 
// Flusso dell'applicazione:
// 1. L'utente avvia l'app e può selezionare due punti importanti nel gioco:
//    - La regione dello zaino (dove si cerca il piccone)
//    - La posizione target (dove trascinare il piccone)
// 2. Quando l'utente clicca "Avvia Macro", viene eseguita la ricerca del piccone nella regione selezionata.
// 3. Se il piccone viene trovato, viene simulato un drag-and-drop per posizionarlo sulla posizione target.
// 4. L'utente può interrompere la macro cliccando "Ferma Macro".
// 5. L'app utilizza un hook di sistema per rilevare i clic del mouse ovunque sullo schermo.

using AutoClicker.Models.System;
using AutoClicker.Models.TM;
using AutoClicker.Service;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Shapes;
using UltimaOnlineMacro.Service;
using Rectangle = System.Drawing.Rectangle;

namespace UltimaOnlineMacro
{
    public partial class MainWindow : Window
    {
        private Regions Regions = new Regions();
        public Pg Pg = new Pg();
        public Logger LogManager;
        private MainWindowService _mainWindowService;
        public MainWindow()
        {
            InitializeComponent();

            //LogManager = new(txtLog);
            //SavedImageTemplate.Initialize();
            //cmbKey.ItemsSource = AutoClicker.Service.ExtensionMethod.Key.PopolaComboKey();
            //cmbKey.SelectedIndex = 0;
            //_mainWindowService = new MainWindowService(this);
            //_mainWindowService.ReadFilesConfiguration();
        }

        private async Task test()
        {
            string muloModel = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "MuloModel.zip");
            string muloModelx = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "MuloRegressorX.zip");
            string muloModely = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "MuloRegressorY.zip");

            var d = new MuloDetectorService(muloModel, muloModelx, muloModely);
            while (true)
            {
                d.DetectAndDrawMulo();
            }
        }
      

        #region Callback
        public void SetBackpackRegion(Rectangle region)
        {
            if (Regions.BackpackRegion != region)
            {
                Regions.BackpackRegion = region;
                LogManager.Loggin($"Regione zaino selezionata: {Regions.BackpackRegion}");
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
                    LogManager.Loggin($"Errore: {e.Message}");
                }
            }
        }

        public void SetStatus(Rectangle region)
        {
          
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
                LogManager.Loggin($"Regione paperdoll selezionata (centrata): {Regions.PaperdollRegion}");
                btnSelectPaperdoll.Background = System.Windows.Media.Brushes.Gray;
            }
        }

        #endregion


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


        #endregion


        #region ClickEvent
        private void SelectStatus_Click(object sender, RoutedEventArgs e)
        {
            OverlayWindow overlay = new OverlayWindow("Status", LogManager);
            overlay.Show();
        }

        private void SelectBackpackRegion_Click(object sender, RoutedEventArgs e)
        {
            OverlayWindow overlay = new OverlayWindow("Piccone", LogManager);
            overlay.Show();
        }


        private void SelectPaperdoll_Click(object sender, RoutedEventArgs e)
        {
            OverlayWindow overlay = new OverlayWindow("Paperdoll", LogManager);
            overlay.Show();
        }

        private void StopBeep_Click(object sender, RoutedEventArgs e)
        {
            Pg.StopBeep();
        }

        private async void Run_Click(object sender, RoutedEventArgs e)
        {
            await test();

            if (!Debugger.IsAttached)
            {
                string haveValue = Regions.HaveValue();
                if (!String.IsNullOrEmpty(haveValue))
                {
                    LogManager.Loggin(haveValue);
                    return;
                }
                string isReady = Pg.IsReady();
                if (!String.IsNullOrEmpty(isReady))
                {
                    LogManager.Loggin(isReady);
                    return;
                }
                if(!chkMuloDaSoma.IsChecked.Value && !chkLamaPortatore.IsChecked.Value)
                {
                    LogManager.Loggin("Seleziona almeno un animale da soma");
                    return;
                }
            }

            //_mainWindowService.TimerUltima.Start();
            //_mainWindowService.CheckMacroButtons();
            //_mainWindowService.SetMuli();

            //btnRun.Background = System.Windows.Media.Brushes.Gray;
            //btnStop.Background = System.Windows.Media.Brushes.Red;
            //LogManager.Loggin("Run!");
            //await Pg.Work(Regions);
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _mainWindowService.TimerUltima.Stop();
            Pg.Stop();
            Pg.RunWork = false;
            btnRun.Background = System.Windows.Media.Brushes.Green;
            btnStop.Background = System.Windows.Media.Brushes.Red;
            LogManager.Loggin("Stop!");
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


        #endregion

      
    }

}

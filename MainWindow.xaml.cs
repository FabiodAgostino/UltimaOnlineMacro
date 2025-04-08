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

using System.Drawing;
using System.Windows;
using System.Windows.Input;
using Point = System.Drawing.Point;
using UltimaOnlineMacro.Service;
using Serilog;
using AutoClicker.Models.TM;
using AutoClicker.Models.System;
using AutoClicker.Service;
using AutoClicker.Library;
using static Emgu.Util.Platform;
using Microsoft.Win32;
using AutoClicker.Service.ExtensionMethod;
using Gma.System.MouseKeyHook;
using System.Windows.Forms;

namespace UltimaOnlineMacro
{
    public partial class MainWindow : Window
    {
        private IKeyboardMouseEvents _globalHook;

        // Coordinate della regione dello zaino e della posizione target nel gioco
        private Regions Regions = new Regions();
        private Pg Pg = new Pg();
        public Logger LogManager;

        public MainWindow()
        {
            SavedImageTemplate.Initialize();
            InitializeComponent();
            LogManager = new(txtLog);
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHook_KeyDown;
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
        public void SetBarStatus(Rectangle region)
        {
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

        public void SetBackpackMuloRegion(Rectangle region)
        {
            if (Regions.MuloRegion != region)
            {
                // Calcola metà larghezza e metà altezza
                int newWidth = region.Width / 2;
                int newHeight = region.Height / 2;

                // Calcola le nuove coordinate per centrare il rettangolo
                int newX = region.X + (region.Width - newWidth);
                int newY = region.Y + (region.Height - newHeight);

                // Crea la nuova regione centrata
                Rectangle centeredRegion = new Rectangle(newX, newY, newWidth, newHeight);
                Regions.MuloRegion = centeredRegion;
                LogManager.Loggin($"Regione zaino mulo selezionata: {Regions.MuloRegion}");
            }
        }
        #endregion


        #region WindowsHelper
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void TestSendInput_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            Thread.Sleep(1000);
            var service = new MouseInputSimulator();
            service.SimulateDoubleClick100Times(300,500);
        }

        private void TestPostMessage_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            Thread.Sleep(1000);
            var service = new PostMessageService();
            service.SimulateRightArrowFor3Seconds();
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

        private void Run_Click(object sender, RoutedEventArgs e)
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

            btnRun.Background = System.Windows.Media.Brushes.Gray;
            btnStop.Background = System.Windows.Media.Brushes.Red;
            Pg.RunWork = true;
            Pg.Work(Regions);

        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Pg.RunWork = false;
            btnRun.Background = System.Windows.Media.Brushes.Green;
            btnStop.Background = System.Windows.Media.Brushes.Red;
            LogManager.Loggin("Stop!");
        }

        private void SelectBackpackMulo_Click(object sender, RoutedEventArgs e)
        {
            OverlayWindow overlay = new OverlayWindow("Mulo", LogManager);
            overlay.Show();
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

        private void SelectBarStatus_Click(object sender, RoutedEventArgs e)
        {
            OverlayWindow overlay = new OverlayWindow("status", LogManager);
            overlay.Show();

          
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


        #region EscMethod
        private void GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Pg.RunWork = false;
                this.WindowState = WindowState.Normal;
                this.Activate();
                btnRun.Background = System.Windows.Media.Brushes.Green;
                btnStop.Background = System.Windows.Media.Brushes.Red;
                LogManager.Loggin("Stop!");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _globalHook.KeyDown -= GlobalHook_KeyDown;
            _globalHook.Dispose();
            base.OnClosed(e);
        }
        #endregion

    }

}

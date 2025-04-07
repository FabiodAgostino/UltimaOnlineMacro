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

namespace UltimaOnlineMacro
{
    public partial class MainWindow : Window
    {
        // ID dell'hook per intercettare gli eventi del mouse
        private IntPtr _hookID = IntPtr.Zero;

        // Coordinate della regione dello zaino e della posizione target nel gioco
        private Regions Regions = new Regions();
        private Pg Pg = new Pg();
        public Logger LogManager;

        public MainWindow()
        {
            SavedImageTemplate.Initialize();
            InitializeComponent();
            LogManager = new(txtLog);
        }

        #region Callback
        public void SetBackpackRegion(Rectangle region)
        {
            if (Region.BackpackRegion != region)
            {
                Region.BackpackRegion = region;
                LogManager.Loggin($"Regione zaino selezionata: {Region.BackpackRegion}");
                try
                {
                    var pickaxe = new Pickaxe(region, SavedImageTemplate.ImageTemplatePickaxe);
                    Pg.PickaxeInBackpack = pickaxe;

                    if (pickaxe.IsFound)
                        Region.Pickaxe = pickaxe;
                    else
                        Region.Pickaxe = null;
                }
                catch (Exception e)
                {
                    LogManager.Loggin($"Errore: {e.Message}");
                }
            }
        }

        public void PaperdollHavePickaxe(Rectangle region)
        {
            if (Region.PaperdollRegion != region)
            {
                Region.PaperdollRegion = region;
                LogManager.Loggin($"Regione paperdoll selezionata: {Region.PaperdollRegion}");
            }
        }

        public void SetBackpackMuloRegion(Rectangle region)
        {
            if (Region.MuloRegion != region)
            {
                Region.MuloRegion = region;
                LogManager.Loggin($"Regione zaino mulo selezionata: {Region.MuloRegion}");
            }
        }
        #endregion


        #region WindowsHelper
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
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

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            string haveValue = Regions.HaveValue();
            if (!String.IsNullOrEmpty(haveValue))
            {
                LogManager.Loggin(haveValue);
                return;
            }
            Pg.Work(Regions);

        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Pg.RunWork = false;
        }

        private void SelectBackpackMulo_Click(object sender, RoutedEventArgs e)
        {
            OverlayWindow overlay = new OverlayWindow("Mulo", LogManager);
            overlay.Show();
        }
        #endregion

    }

}

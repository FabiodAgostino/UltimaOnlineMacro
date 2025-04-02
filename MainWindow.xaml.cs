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

using Emgu.CV.Structure;
using Emgu.CV;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using UltimaOnlineMacro.ExtensionMethod;
using Point = System.Drawing.Point;
using System.IO;
using Emgu.CV.Reg;
using Emgu.CV.Util;
using System.Drawing.Imaging;
using static UltimaOnlineMacro.UtilityClass;
using UltimaOnlineMacro.Models;
using System.Windows.Input;
namespace UltimaOnlineMacro
{
    public partial class MainWindow : Window
    {
        // ID dell'hook per intercettare gli eventi del mouse
        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelMouseProc _proc; // Delegato che gestisce gli eventi mouse

        // Coordinate della regione dello zaino e della posizione target nel gioco
        private Regions Region = new Regions();
        private Pg Pg = new Pg();
        private Point targetPosition; // Posizione dove il piccone deve essere trascinato
        private CancellationTokenSource _cts; // Token per annullare l'esecuzione della macro
        private Point selectionStart;

        ImageTemplate ImageTemplatePickaxe = new ImageTemplate("Pickaxe.png");
        public MainWindow()
        {
            InitializeComponent();
            LogManager.Initialize(txtLog);
        }

        private void WearPickaxe()
        {
            var pickaxe = new Pickaxe(Region.BackpackRegion, ImageTemplatePickaxe);
            SetCursorPos(pickaxe.X, pickaxe.Y);
            DoubleClick(pickaxe.X, pickaxe.Y);
            Pg.PickaxeInHand = pickaxe;
        }

        #region Callback
        public void SetBackpackRegion(Rectangle region)
        {
            if (Region.BackpackRegion != region)
            {
                Region.BackpackRegion = region;
                LogManager.Log($"Regione zaino selezionata: {Region.BackpackRegion}");
                try
                {
                    var pickaxe = new Pickaxe(region, ImageTemplatePickaxe);
                    Pg.PickaxeInBackpack = pickaxe;

                    if (pickaxe.IsFound)
                        Region.Pickaxe = pickaxe;
                    else
                        Region.Pickaxe = null;
                }
                catch (Exception e)
                {
                    LogManager.Log($"Errore: {e.Message}");
                }
            }
        }

        public void PaperdollHavePickaxe(Rectangle region)
        {
            if (Region.PaperdollRegion != region)
            {
                Region.PaperdollRegion = region;
                LogManager.Log($"Regione paperdoll selezionata: {Region.PaperdollRegion}");
                try
                {
                    var pickaxePaperdoll = new Pickaxe(region, ImageTemplatePickaxe);
                    if (pickaxePaperdoll.X == 0 && pickaxePaperdoll.Y == 0) //Se il paperdoll non ha il piccone
                    {
                        LogManager.Log($"Il paperdoll non utilizza il piccone.");
                        if (!Region.PaperdollRegion.IsEmpty && !Region.PaperdollRegion.IsEmpty)
                        {
                            WearPickaxe();
                        }
                        else
                        {
                            LogManager.Log($"Seleziona una regione per lo zaino.");
                        }
                    }

                }
                catch (Exception e)
                {
                    LogManager.Log($"Errore: {e.Message}");
                }
            }
        }

        public void SetBackpackMuloRegion(Rectangle region)
        {
            if (Region.MuloRegion != region)
            {
                Region.MuloRegion = region;
                LogManager.Log($"Regione zaino mulo selezionata: {Region.MuloRegion}");
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
            OverlayWindow overlay = new OverlayWindow("Piccone");
            overlay.Show();
        }


        private void SelectPaperdoll_Click(object sender, RoutedEventArgs e)
        {
            OverlayWindow overlay = new OverlayWindow("Paperdoll");
            overlay.Show();
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            string haveValue = Region.HaveValue();
            if (!String.IsNullOrEmpty(haveValue))
            {
                LogManager.Log(haveValue);
                return;
            }

            LogManager.Log("RUN");



        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
        }

        private void SelectBackpackMulo_Click(object sender, RoutedEventArgs e)
        {
            OverlayWindow overlay = new OverlayWindow("Mulo");
            overlay.Show();
        }
        #endregion
    }
}

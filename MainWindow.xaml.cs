using AutoClicker.Models.System;
using AutoClicker.Models.TM;
using AutoClicker.Service;
using AutoClicker.Utils;
using LogManager;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using UltimaOnlineMacro.Service;
using Rectangle = System.Drawing.Rectangle;

namespace UltimaOnlineMacro
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public Regions Regions = new Regions();
        public Pg Pg;
        private MainWindowService _mainWindowService;
        public ProcessService ProcessService = new();
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseChanged([CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        // Espongo Risorse come ObservableCollection
        private ObservableCollection<Mulo> _muli;
        public ObservableCollection<Mulo> Muli
        {
            get => _muli;
            set
            {
                _muli = value;
                RaiseChanged();
            }
        }
        private ICommand _svuotaMuloCommand;
        public ICommand SvuotaMuloCommand => _svuotaMuloCommand ??= new RelayCommand<Mulo>(_mainWindowService.SvuotaMulo);

        public MainWindow()
        {
            this.Hide();
            DataContext = this;
            _mainWindowService = new MainWindowService(this);
            InitializeComponent();
            Logger.Loggin("Applicazione avviata", false, false);
        }

        #region Callback

        public void SetBackpackRegion(Rectangle region)
        {
            Regions.BackpackRegion = region;
            Logger.Loggin($"Regione zaino selezionata");
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

        public void SetStatus(Rectangle region)
        {
            Regions.StatusRegion = region;
            btnSelectStatus.Background = System.Windows.Media.Brushes.Gray;
            var t = new TesserActService();
            var result = t.GetStatusBar(Regions.StatusRegion);
            if (result.Stamina.value > 0 || result.Stamina.max > 0)
                Logger.Loggin("Status intercettato correttamente!");
            else
                Logger.Loggin("Status non trovato");
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
                Logger.Loggin($"Regione paperdoll selezionata");
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
            var service = new SendInputService(new ProcessService());
            await service.TestKeyboard();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private async void TestMouse_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            await Task.Delay(1000);
            var service = new SendInputService(new ProcessService());
            await service.TestMouse();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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
            SoundsPlayerService.Stop();
            SoundsPlayerService.Dispose();
        }

        private async void Run_Click(object sender, RoutedEventArgs e) => await Run();

        public async Task Run()
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
            _mainWindowService.SaveSettings();
            btnRun.Background = System.Windows.Media.Brushes.Gray;
            btnStop.Background = System.Windows.Media.Brushes.Red;
            Pg.FuriaChecked = chkFuria.IsChecked.Value;
            Logger.Loggin("Run!");
            await Pg.Work(Regions,true);
        }


        private async void RefreshScreen_Click(object sender, RoutedEventArgs e)
        {
            SetBackpackRegion(Regions.BackpackRegion);
            SetStatus(Regions.StatusRegion);
        }

        private async void Stop_Click(object sender, RoutedEventArgs e) => await Stop();
        public async Task Stop()
        {
            if(Pg.HaveMacrocheck)
            {
                new MacrocheckNotificationWindow(120).Show();
            }
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

        private void SelectFileMacro_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new();
            openFileDialog.Title = "Seleziona il file macros";
            string expectedFileName = "macros.xml";
            openFileDialog.Filter = $"{expectedFileName}|{expectedFileName}";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(selectedFile);

                if (fileName.Equals(expectedFileName, StringComparison.OrdinalIgnoreCase))
                {
                    // Ottieni il nome della cartella principale (nome PG)
                    string folderPath = System.IO.Path.GetDirectoryName(selectedFile);
                    string characterName = System.IO.Path.GetFileName(folderPath);

                    SelectedFilePathText.Text = $"File selezionato: {selectedFile}";
                    Pg.Name = characterName;
                    Pg.PathMacro = PathHelper.NormalizePath(selectedFile);

                    // Usa il nuovo metodo per trovare il journal di oggi
                    try
                    {
                        
                        string journalFilePath = _mainWindowService.FindTodayJournalFile(selectedFile);

                        if (!string.IsNullOrEmpty(journalFilePath))
                        {
                            Pg.PathJuornalLog = journalFilePath;
                            Logger.Loggin($"Impostato PathJuornalLog: {journalFilePath}");
                        }
                        else
                        {
                            Pg.PathJuornalLog = PathHelper.NormalizePath(selectedFile); // Fallback al file macro
                            Logger.Loggin($"Utilizzato fallback per PathJuornalLog: {selectedFile}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Loggin($"Errore nell'impostazione del PathJuornalLog: {ex.Message}");
                        Pg.PathJuornalLog = PathHelper.NormalizePath(selectedFile); // Fallback al file macro
                    }

                    // Elabora il file macro con MacroManipulator
                    try
                    {
                        // Verifica e manipola le macro nel file
                        var result = AutoClicker.Service.MacroManipulator.ManipulateMacros(
                            selectedFile, // File di input
                            selectedFile, // Sovrascrive lo stesso file
                            true         // Crea infuriarsi se non presente
                        );

                        // Mostra informazioni sulle macro create/aggiornate
                        string message = "";

                        Pg.HaveBaseFuria = result.HaveBaseFuria;
                        if (Pg.HaveBaseFuria)
                            chkFuria.IsEnabled = true;

                        if (result.HasInfuriarsi)
                        {
                            message += $"Macro 'Infuriarsi' impostata su: {result.InfuriarsiKeyInfo}\n";
                        }

                        if (result.HasSaccaraccolta)
                        {
                            message += $"Macro 'saccaraccolta' impostata su: {result.SaccaraccoltaKeyInfo}\n";
                        }

                        // Se sono state aggiunte nuove macro, riavvia il client
                        if (result.NeedsClientRestart)
                        {
                            if (System.Windows.MessageBox.Show(
                                $"Personaggio: {characterName}\n\n{message}\n\nSono state aggiunte nuove macro. È necessario riavviare il client per applicare le modifiche. Vuoi riavviare il client ora?",
                                "Riavvio necessario",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                bool restartSuccessful = ProcessService.HandleClientRestartUpdater(selectedFile);

                                if (!restartSuccessful)
                                {
                                    System.Windows.MessageBox.Show(
                                        "Non è stato possibile riavviare automaticamente il client. Chiudi manualmente il client e riavvialo per applicare le modifiche.",
                                        "Errore",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning
                                    );
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(message))
                        {
                            System.Windows.MessageBox.Show(
                                $"Personaggio: {characterName}\n\n{message}\n\nLe macro sono già presenti e configurate correttamente.",
                                "Informazioni macro",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(
                            $"Errore durante l'elaborazione del file macro: {ex.Message}",
                            "Errore",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        $"Il file selezionato non corrisponde al formato richiesto ({expectedFileName}).",
                        "File non valido",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
        }

        #endregion ClickEvent

        #region Wizard
        public async Task LoadServices() => await _mainWindowService.Initialize();
        public void LoadSettings() => _mainWindowService.LoadSettings();
        public void LoadTools() => _mainWindowService.LoadingTools();
        public void LoadMisc() => _mainWindowService.LoadingTools();

        #endregion
    }
}
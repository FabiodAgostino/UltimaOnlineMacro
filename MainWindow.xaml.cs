using AutoClicker.Models.System;
using AutoClicker.Models.TM;
using AutoClicker.Service;
using AutoClicker.Utils;
using LogManager;
using Microsoft.Win32;
using MQTT;
using MQTT.Models;
using QRCoder;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using UltimaOnlineMacro.Service;
using static MQTT.Models.MqttNotificationModel;
using static System.Net.WebRequestMethods;
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
            this.Loaded += PositionWindowBottomRight;

        }


        // Assicurati di smaltire correttamente il servizio MQTT
        protected override void OnClosed(EventArgs e)
        {
            _mainWindowService.MqttNotificationService.Dispose();
            base.OnClosed(e);
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
        private void PositionWindowBottomRight(object sender, RoutedEventArgs e)
        {
            try
            {
                // Usa il monitor primario
                var screen = System.Windows.Forms.Screen.PrimaryScreen;
                var workArea = screen.WorkingArea;

                // Calcola la posizione DPI-aware
                var dpi = VisualTreeHelper.GetDpi(this);
                var scale = dpi.DpiScaleX;

                // Posiziona in basso a destra con padding di 20 pixel
                this.Left = (workArea.Right - this.ActualWidth * scale - 20) / scale;
                this.Top = (workArea.Bottom - this.ActualHeight * scale - 20) / scale;
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore nel posizionamento: {ex.Message}", true, false);

                // Fallback semplice
                var workArea = SystemParameters.WorkArea;
                this.Left = workArea.Right - this.ActualWidth - 20;
                this.Top = workArea.Bottom - this.ActualHeight - 20;
            }
        }
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

        private async void NotificaInfo_Click(object sender, RoutedEventArgs e)
        {
            await _mainWindowService.MqttNotificationService.SendNotificationAsync(new MqttNotificationModel() { DeviceId = Pg.Name, Title = "Notifica di Test", Message = "Questo è un test di notifica informativa inviata dall'app desktop.", Type = MQTT.Models.MqttNotificationModel.NotificationSeverity.Info });
        }

        private async void NotificaWarning_Click(object sender, RoutedEventArgs e)
        {
            await _mainWindowService.MqttNotificationService.SendNotificationAsync(new MqttNotificationModel() { DeviceId = Pg.Name, Title = "Avviso di Test", Message = "Questo è un test di notifica di avviso inviata dall'app desktop", Type = MQTT.Models.MqttNotificationModel.NotificationSeverity.Warning });
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
            await Pg.Work(Regions, true);
        }


        private async void RefreshScreen_Click(object sender, RoutedEventArgs e)
        {
            SetBackpackRegion(Regions.BackpackRegion);
            SetStatus(Regions.StatusRegion);
        }

        private async void Stop_Click(object sender, RoutedEventArgs e) => await Stop();
        public async Task Stop()
        {
            // Se non siamo sul thread UI, dobbiamo invocare il metodo sul thread UI
            if (!Dispatcher.CheckAccess())
            {
                await Dispatcher.InvokeAsync(async () => await Stop());
                return;
            }

            if (Pg.HaveMacrocheck)
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
        public async Task LoadTools() => await _mainWindowService.LoadingTools();
        #endregion


        #region QR Code Event Handlers

        /// <summary>
        /// Event handler per rigenerare il QR del personaggio
        /// </summary>
        private void RegenerateCharacterQR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Chiama il metodo esistente del MainWindowService
                _mainWindowService.GenerateQrCode();
                Logger.Loggin("QR code personaggio rigenerato");
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore nella rigenerazione QR personaggio: {ex.Message}", true);
            }
        }

        /// <summary>
        /// Event handler per aprire il link di download diretto
        /// </summary>
        private void OpenDownloadLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                const string downloadUrl = "https://github.com/FabiodAgostino/UOMacroMobile/releases/download/1.2/UoMacroMobileV1.2.apk";

                Process.Start(new ProcessStartInfo
                {
                    FileName = downloadUrl,
                    UseShellExecute = true
                });

                Logger.Loggin("Link di download aperto nel browser");
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore nell'apertura del link: {ex.Message}", true);
            }
        }


        #endregion

        #region QR Code Generation Methods

        /// <summary>
        /// Genera QR code per il download dell'app mobile
        /// </summary>
        private void GenerateDownloadQrCode()
        {
            try
            {
                var qrImage = QRCodeService.GenerateAppDownloadQRCode();

                // Aggiorna l'elemento UI per il download
                DownloadQRCodeImage.Source = qrImage;
                DownloadQRCodeInfo.Text = "Scansiona per scaricare ROTMobile";

                Logger.Loggin("QR code per download app generato con successo");
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore nella generazione del QR code download: {ex.Message}", true);

                // Gestisci l'errore nell'UI
                DownloadQRCodeImage.Source = null;
                DownloadQRCodeInfo.Text = "Errore nella generazione del QR Code";
            }
        }

        /// <summary>
        /// Metodo helper per copiare il link negli appunti
        /// </summary>
        private void CopyDownloadLinkToClipboard()
        {
            try
            {
                const string downloadUrl = "https://drive.google.com/file/d/1sT4oOYhntRGbd2QebdU--Cppi9TjHLGR/view";
                Clipboard.SetText(downloadUrl);
                Logger.Loggin("Link di download copiato negli appunti");

                // Feedback visivo temporaneo
                ShowTemporaryMessage("Link copiato negli appunti!");
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore nella copia del link: {ex.Message}", true);
            }
        }

        /// <summary>
        /// Mostra un messaggio temporaneo nell'UI
        /// </summary>
        private void ShowTemporaryMessage(string message)
        {
            try
            {
                var originalText = DownloadQRCodeInfo.Text;
                var originalBrush = DownloadQRCodeInfo.Foreground;

                DownloadQRCodeInfo.Text = message;
                DownloadQRCodeInfo.Foreground = System.Windows.Media.Brushes.LimeGreen;

                // Ripristina dopo 3 secondi
                Task.Delay(3000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        DownloadQRCodeInfo.Text = originalText;
                        DownloadQRCodeInfo.Foreground = originalBrush;
                    });
                });
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore nel mostrare messaggio temporaneo: {ex.Message}", true);
            }
        }

        #endregion

    }
}
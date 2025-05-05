using AutoClicker.Const;
using AutoClicker.Models.System;
using AutoClicker.Models.System.UltimaOnlineMacro.Models.System;
using AutoClicker.Models.TM;
using AutoClicker.Service;
using AutoClicker.Utils;
using LogManager;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace UltimaOnlineMacro.Service
{
    public class MainWindowService
    {
        public TimerUltima TimerUltima;
        private Pg _pg { get; set; }
        private bool _initializeMacro { get; set; } = true;
        private MainWindow _mainWindow { get; set; }
        private SettingsService _settingsService;

        public MainWindowService(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _mainWindow.Muli = new ObservableCollection<Mulo>();
            _mainWindow.Pg = new Pg(_mainWindow.ProcessService, async (bool run) => await StartStop(run));
            _pg = mainWindow.Pg;
            Initialize();
        }

        public async Task StartStop(bool run)
        {
            if (run)
                await _mainWindow.Run();
            else
                await _mainWindow.Stop();
        }
        public async void Initialize()
        {
            await _mainWindow.ProcessService.FocusWindowReliably();
            Logger._logTextBox = _mainWindow.txtLog;
            SavedImageTemplate.Initialize();
            _mainWindow.cmbKey.ItemsSource = AutoClicker.Service.ExtensionMethod.Key.PopolaComboKey();
            _mainWindow.cmbKey.SelectedIndex = 0;
            _settingsService = new SettingsService();
            LoadSettings();
            SetTimerUltima();
            ReadFilesConfiguration();
            _pg.RefreshRisorse += RefreshRisorse;
            _mainWindow.Activate();
        }

        public void RefreshRisorse()
        {
            var mulo=
            _mainWindow.Muli = new ObservableCollection<Mulo>(_pg.Muli);
            _mainWindow.RaiseChanged();
            var muloSelezionato = _pg.Muli.FirstOrDefault(m => m.Selected);
            if (muloSelezionato != null)
            {
                double percentualeCarico = (double)muloSelezionato.ActualOre / Mulo.MaxOre * 100;

                if (percentualeCarico > 90)
                {
                    SoundsPlayerService.OnePlay(SoundsFile.Notify);
                    Logger.Loggin($"Attenzione: Mulo carico al {percentualeCarico:F1}%");
                }
            }
        }
        private void LoadSettings()
        {
            var settings = _settingsService.LoadSettings();

            // Verifica se le impostazioni sono nulle prima di applicarle
            if (settings == null)
                return;

            try
            {
                // Applica le impostazioni caricate
                if (settings.BackpackRegion != null)
                    _mainWindow.SetBackpackRegion(settings.BackpackRegion.ToRectangle());

                if (settings.PaperdollRegion != null)
                    _mainWindow.PaperdollHavePickaxe(settings.PaperdollRegion.ToRectangle());

                if (settings.StatusRegion != null)
                    _mainWindow.SetStatus(settings.StatusRegion.ToRectangle());

                // Imposta i valori della macro
                if (!string.IsNullOrEmpty(settings.SelectedKey))
                    _mainWindow.cmbKey.SelectedItem = settings.SelectedKey;
                if(settings.HaveBaseFuria)
                {
                    _mainWindow.Pg.HaveBaseFuria = settings.HaveBaseFuria;
                    _mainWindow.chkFuria.IsEnabled = true;
                    _mainWindow.chkFuria.IsChecked = settings.FuriaChecked;
                }
                _mainWindow.chkCtrl.IsChecked = settings.CtrlModifier;
                _mainWindow.chkAlt.IsChecked = settings.AltModifier;
                _mainWindow.chkShift.IsChecked = settings.ShiftModifier;
                _mainWindow.sldDelay.Value = settings.DelayValue == 0 ? 5000 : settings.DelayValue;

                _mainWindow.Pg.Name = settings.PgName;
                // Imposta il percorso del journal
                if (!string.IsNullOrEmpty(settings.JournalPath))
                {
                    _mainWindow.Pg.PathJuornalLog = settings.JournalPath;
                    _mainWindow.Pg.PathMacro = settings.MacroPath;

                    _mainWindow.SelectedFilePathText.Text = $"File selezionato: {settings.MacroPath}";
                }

                // Imposta gli animali selezionati
                _mainWindow.chkMuloDaSoma.IsChecked = settings.MuloDaSomaSelected == false && settings.LamaPortatoreSelected == false ? true : settings.MuloDaSomaSelected;
                _mainWindow.chkLamaPortatore.IsChecked = settings.LamaPortatoreSelected;

                // Imposta posizioni cibo e acqua
                if (settings.FoodPosition != null)
                    _mainWindow.Regions.FoodXY = settings.FoodPosition.ToPoint();

                if (settings.WaterPosition != null)
                    _mainWindow.Regions.WaterXY = settings.WaterPosition.ToPoint();

                Logger.Loggin("Impostazioni di default caricate con successo!");
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore durante l'applicazione delle impostazioni: {ex.Message}", true);
            }
        }

        public void SaveSettings()
        {
            var settings = new AppSettings
            {
                PgName = _mainWindow.Pg.Name,
                HaveBaseFuria = _mainWindow.Pg.HaveBaseFuria,
                BackpackRegion = _mainWindow.Regions.BackpackRegion != Rectangle.Empty ? new RectangleSettings(_mainWindow.Regions.BackpackRegion) : null,
                PaperdollRegion = _mainWindow.Regions.PaperdollRegion != Rectangle.Empty ? new RectangleSettings(_mainWindow.Regions.PaperdollRegion) : null,
                StatusRegion = _mainWindow.Regions.StatusRegion != Rectangle.Empty ? new RectangleSettings(_mainWindow.Regions.StatusRegion) : null,
                PaperdollPickaxeRegion = _mainWindow.Regions.PaperdollPickaxeRegion != Rectangle.Empty ? new RectangleSettings(_mainWindow.Regions.PaperdollPickaxeRegion) : null,

                SelectedKey = _mainWindow.cmbKey.SelectedItem?.ToString(),
                CtrlModifier = _mainWindow.chkCtrl.IsChecked ?? false,
                AltModifier = _mainWindow.chkAlt.IsChecked ?? false,
                ShiftModifier = _mainWindow.chkShift.IsChecked ?? false,
                DelayValue = _mainWindow.sldDelay.Value,

                JournalPath = _mainWindow.Pg.PathJuornalLog,
                MacroPath = _mainWindow.Pg.PathMacro,
                FuriaChecked = _mainWindow.chkFuria.IsChecked ?? false,

                MuloDaSomaSelected = _mainWindow.chkMuloDaSoma.IsChecked ?? false,
                LamaPortatoreSelected = _mainWindow.chkLamaPortatore.IsChecked ?? false,

                FoodPosition = _mainWindow.Regions.FoodXY.X != 0 && _mainWindow.Regions.FoodXY.Y != 0 ? new PointSettings(_mainWindow.Regions.FoodXY) : null,
                WaterPosition = _mainWindow.Regions.WaterXY.X != 0 && _mainWindow.Regions.WaterXY.Y != 0 ? new PointSettings(_mainWindow.Regions.WaterXY) : null
            };

            _settingsService.SaveSettings(settings);
        }

        public void SetTimerUltima()
        {
            TimerUltima = new TimerUltima(text =>
            {
                if (_mainWindow.Dispatcher.CheckAccess())
                {
                    _mainWindow.lblElapsed.Text = text;
                }
                else
                {
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.lblElapsed.Text = text;
                    });
                }
            });
        }

        public void ReadFilesConfiguration()
        {
            ReadMuloDetector();
            ReadTessdata();
        }

        private void ReadMuloDetector()
        {
            //    string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MuloDetector.zip");
            //    if (File.Exists(modelPath))
            //        _pg.DetectorService = new MuloDetectorService(modelPath);
            //    else
            //        LogManager.Loggin("MuloDetector non inizializzato, non trovo il il file.");
        }

        private void ReadTessdata()
        {
            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata", "eng.traineddata");
            if (!File.Exists(modelPath))
                Logger.Loggin("Non trovo il file tessdata.");
        }

        public void CheckMacroButtons()
        {
            Keys modifiers = Keys.None;
            if (_mainWindow.chkCtrl.IsChecked == true)
                modifiers |= Keys.ControlKey;
            if (_mainWindow.chkShift.IsChecked == true)
                modifiers |= Keys.Shift;
            if (_mainWindow.chkAlt.IsChecked == true)
                modifiers |= Keys.Alt;

            string? selectedKey = _mainWindow.cmbKey.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedKey))
            {
                return;
            }

            Keys key;
            var delay = _mainWindow.sldDelay.Value;
            if (Enum.TryParse(selectedKey, out key))
            {
                var macroKeys = new List<Keys>();
                macroKeys.Add(key);
                if (modifiers != Keys.None)
                    macroKeys.Add(modifiers);
                if (_pg.Macro == null || _pg.Macro.Delay != delay || _pg.Macro.MacroKeys.All(x => !macroKeys.Contains(x)))
                {
                    _pg.Macro = new Macro(macroKeys, delay, (repetitions) => { _mainWindow.txtRuns.Text = repetitions.ToString(); });
                    if(_mainWindow.chkFuria.IsChecked == true)
                    {
                        _pg.Macro.MacroFuria = new();
                        _pg.Macro.MacroFuria.Add(Keys.ShiftKey);
                        _pg.Macro.MacroFuria.Add(Keys.O);
                    }
                    _pg.Macro.MacroSaccaRaccolta = new();
                    _pg.Macro.MacroSaccaRaccolta.Add(Keys.ShiftKey);
                    _pg.Macro.MacroSaccaRaccolta.Add(Keys.I);
                }
            }
            else
            {
            }
            _initializeMacro = false;
        }

        public void SetMuli()
        {
            if (_pg.Muli.Count == 0)
            {
                if (_mainWindow.chkMuloDaSoma.IsChecked.HasValue && _mainWindow.chkMuloDaSoma.IsChecked.Value)
                    _pg.Muli.Add(new Mulo(MuloType.MULO_DA_SOMA, true, () => _pg.ChangeMuloOrStop()));

                if (_mainWindow.chkLamaPortatore.IsChecked.HasValue && _mainWindow.chkLamaPortatore.IsChecked.Value)
                    _pg.Muli.Add(new Mulo(MuloType.LAMA_PORTATORE, false, () => _pg.ChangeMuloOrStop()));
            }
        }
    }
}
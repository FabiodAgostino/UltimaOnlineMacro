using AutoClicker.Models.System;
using AutoClicker.Models.System.UltimaOnlineMacro.Models.System;
using AutoClicker.Models.TM;
using AutoClicker.Utils;
using LogManager;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
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
            _mainWindow.Risorse = new ObservableCollection<KeyValuePair<string, int>>();
            _mainWindow.Pg = new Pg();
            _pg = mainWindow.Pg;
            Initialize();
        }

        public void Initialize()
        {
            Logger._logTextBox = _mainWindow.txtLog;
            SavedImageTemplate.Initialize();
            _mainWindow.cmbKey.ItemsSource = AutoClicker.Service.ExtensionMethod.Key.PopolaComboKey();
            _mainWindow.cmbKey.SelectedIndex = 0;
            _settingsService = new SettingsService();
            LoadSettings();
            SetTimerUltima();
            ReadFilesConfiguration();
            _pg.RefreshRisorse += RefreshRisorse;
        }

        public void RefreshRisorse(Dictionary<string, int> risorse)
        {
            // Svuota e ricostruisci la collection
            _mainWindow.Risorse.Clear();
            foreach (var kv in risorse)
            {
                _mainWindow.Risorse.Add(kv);
            }
            _mainWindow.RaiseChanged();
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

                _mainWindow.chkCtrl.IsChecked = settings.CtrlModifier;
                _mainWindow.chkAlt.IsChecked = settings.AltModifier;
                _mainWindow.chkShift.IsChecked = settings.ShiftModifier;
                _mainWindow.sldDelay.Value = settings.DelayValue == 0 ? 5000 : settings.DelayValue;

                // Imposta il percorso del journal
                if (!string.IsNullOrEmpty(settings.JournalPath))
                {
                    _mainWindow.Pg.PathJuornalLog = settings.JournalPath;
                    _mainWindow.SelectedFilePathText.Text = $"File selezionato: {settings.JournalPath}";
                }

                // Imposta gli animali selezionati
                _mainWindow.chkMuloDaSoma.IsChecked = settings.MuloDaSomaSelected == false && settings.LamaPortatoreSelected == false ? true : settings.MuloDaSomaSelected;
                _mainWindow.chkLamaPortatore.IsChecked = settings.LamaPortatoreSelected;

                // Imposta posizioni cibo e acqua
                if (settings.FoodPosition != null)
                    _mainWindow.Regions.FoodXY = settings.FoodPosition.ToPoint();

                if (settings.WaterPosition != null)
                    _mainWindow.Regions.WaterXY = settings.WaterPosition.ToPoint();

                Logger.Loggin("Impostazioni caricate con successo!");
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
            //    string modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MuloDetector.zip");
            //    if (File.Exists(modelPath))
            //        _pg.DetectorService = new MuloDetectorService(modelPath);
            //    else
            //        LogManager.Loggin("MuloDetector non inizializzato, non trovo il il file.");
        }

        private void ReadTessdata()
        {
            string modelPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata", "eng.traineddata");
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
                    _pg.Macro = new Macro(macroKeys, delay, (repetitions) => { _mainWindow.txtRuns.Text = repetitions.ToString(); });
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
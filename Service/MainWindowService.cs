using AutoClicker.Models.TM;
using AutoClicker.Utils;
using LogManager;
using System.IO;
using System.Windows.Forms;

namespace UltimaOnlineMacro.Service
{
    public class MainWindowService
    {
        public TimerUltima TimerUltima;
        private Pg _pg { get; set; }
        private bool _initializeMacro { get; set; } = true;
        private MainWindow _mainWindow { get; set; }

        public MainWindowService(MainWindow mainWindow)
        {
            _pg = mainWindow.Pg;
            _mainWindow = mainWindow;
            SetTimerUltima();
            ReadFilesConfiguration();
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
            //string modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MuloDetector.zip");
            //if (File.Exists(modelPath))
            //    _pg.DetectorService = new MuloDetectorService(modelPath);
            //else
            //    LogManager.Loggin("MuloDetector non inizializzato, non trovo il il file.");
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
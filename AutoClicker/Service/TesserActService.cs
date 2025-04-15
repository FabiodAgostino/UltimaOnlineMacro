using AutoClicker.Models.TM;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Threading;
using Tesseract;
using static AutoClicker.Utils.User32DLL;
using static System.Windows.Forms.AxHost;
namespace AutoClicker.Service
{
    public class TesserActService
    {
        // Token per la cancellazione del task periodico
        private CancellationTokenSource _cancellationTokenSource;
        // L'ultimo stato rilevato
        public StatusBar _lastStatusBar;
        // Lock object per l'accesso thread-safe
        private readonly object _lockObject = new object();
        // Evento che notifica quando lo stato viene aggiornato
        public event EventHandler<StatusBar> StatusUpdated;
        private AutoClickerLogger _logger = new AutoClickerLogger();


        public StatusBar GetStatusBar(Rectangle region)
        {
            StatusBar statusBar = new StatusBar();

            Bitmap bitmap = new Bitmap(region.Width, region.Height, PixelFormat.Format24bppRgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(region.X, region.Y, 0, 0, region.Size);
            }
            using (Bitmap processedBitmap = PreprocessImage(bitmap))
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
                processedBitmap.Save("processed.png");
                using (var engine = new TesseractEngine(path, "eng", EngineMode.Default))
                {
                    engine.DefaultPageSegMode = PageSegMode.SingleBlock;
                    engine.SetVariable("tessedit_char_whitelist", "0123456789/StamWeight");

                    engine.SetVariable("segment_mode", "1");

                    using (var page = engine.Process(processedBitmap))
                    {
                        string text = page.GetText();

                        ExtractStatusValues(text, out (int, int) stamina, out (int, int) weight);
                        if (stamina.Item2 != 0 && stamina.Item1 != 0)
                        {
                            statusBar.Stamina = stamina;
                            statusBar.Stone = weight;
                        }
                        _logger.Loggin($"Stamina:{statusBar.Stamina.value}/{statusBar.Stamina.max}");
                        _logger.Loggin($"Weight:{statusBar.Stone.value}/{statusBar.Stone.max}");

                    }
                }
            }

            return statusBar;
        }

        private Bitmap PreprocessImage(Bitmap original)
        {
            Bitmap result = new Bitmap(original.Width, original.Height);

            using (Graphics g = Graphics.FromImage(result))
            {
                ColorMatrix colorMatrix = new ColorMatrix(
                    new float[][]
                    {
                new float[] {1.5f, 0, 0, 0, 0},
                new float[] {0, 1.5f, 0, 0, 0},
                new float[] {0, 0, 1.5f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {-0.2f, -0.2f, -0.2f, 0, 1}
                    });

                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);
                    g.DrawImage(original, new Rectangle(0, 0, result.Width, result.Height),
                        0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            return result;
        }

        private void ExtractStatusValues(string text, out (int, int) stamina, out (int, int) weight)
        {
            stamina = (0, 0);
            weight = (0, 0);

            Regex stamRegex = new Regex(@"Stam(\d+)/(\d+)");
            Regex weightRegex = new Regex(@"Weight(\d+)/(\d+)");

            // Cerca Stamina
            Match stamMatch = stamRegex.Match(text);
            if (stamMatch.Success)
            {
                int stamMin = int.Parse(stamMatch.Groups[1].Value);
                int stamMax = int.Parse(stamMatch.Groups[2].Value);
                stamina = (stamMin, stamMax);
            }

            // Cerca Weight
            Match weightMatch = weightRegex.Match(text);
            if (weightMatch.Success)
            {
                int weightMin = int.Parse(weightMatch.Groups[1].Value);
                int weightMax = int.Parse(weightMatch.Groups[2].Value);
                weight = (weightMin, weightMax);
            }
        }

        public void StartMonitoring(Rectangle region, int updateIntervalMs = 2000)
        {
            StopMonitoring(); // Ferma eventuali monitoraggi esistenti

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            // Avvia un task in background che aggiorna lo stato periodicamente
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        Stopwatch sw = Stopwatch.StartNew();

                        // Esegui il rilevamento dello stato in modo asincrono
                        var newStatus = await GetStatusBarAsync(region);

                        sw.Stop();
                        Debug.WriteLine($"Aggiornamento stato completato in {sw.ElapsedMilliseconds} ms");

                        // Aggiorna lo stato e notifica i listener
                        lock (_lockObject)
                        {
                            _lastStatusBar = newStatus;
                        }

                        // Notifica i listener dell'aggiornamento
                        StatusUpdated?.Invoke(this, newStatus);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Errore nell'aggiornamento dello stato: {ex.Message}");
                    }

                    // Attendi l'intervallo specificato prima del prossimo aggiornamento
                    try
                    {
                        await Task.Delay(updateIntervalMs, token);
                    }
                    catch (TaskCanceledException)
                    {
                        // Gestisci la cancellazione
                        break;
                    }
                }
            }, token);
        }

        public void StopMonitoring()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        // Ottieni l'ultimo stato rilevato (senza bloccare)
        public StatusBar GetLastStatus()
        {
            lock (_lockObject)
            {
                return _lastStatusBar;
            }
        }

        // Versione asincrona del metodo GetStatusBar
        public async Task<StatusBar> GetStatusBarAsync(Rectangle region)
        {
            return await Task.Run(() => GetStatusBar(region));
        }

    }
}

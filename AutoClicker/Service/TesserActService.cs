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

            Bitmap bitmap = ExtensionMethod.Image.CaptureRegionBitmap(region);

            using var processedStream = ImagePreprocessorService.ProcessImage(bitmap);
            {
                using var pix = Pix.LoadFromMemory(processedStream.ToArray());
                // Salva l'immagine preprocessata per debug
                pix.Save("pix_debug.png");

                string tessdataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
                using var engine = new TesseractEngine(tessdataPath, "eng", EngineMode.LstmOnly);
                {
                    using var page = engine.Process(bitmap, PageSegMode.Auto);
                    var text = page.GetText();
                    var confidence = page.GetMeanConfidence();
                    if (confidence > 0.7)
                    {
                        bool success = ExtractStatusValues(text, out (int, int) stamina, out (int, int) weight);
                        if (success && (stamina.Item2 != 0 && stamina.Item1 != 0))
                        {
                            statusBar.Stamina = stamina;
                            statusBar.Stone = weight;
                        }
                    }
                }
            }

            return statusBar;
        }


        private bool ExtractStatusValues(string text, out (int, int) stamina, out (int, int) weight)
        {
            stamina = (0, 0);
            weight = (0, 0);

            // Regex con eventuali spazi opzionali e case insensitive
            Regex stamRegex = new Regex(@"Stam\s*(\d+)\s*/\s*(\d+)", RegexOptions.IgnoreCase);
            Regex weightRegex = new Regex(@"Weight\s*(\d+)\s*/\s*(\d+)", RegexOptions.IgnoreCase);

            // Cerca e controlla la match per Stamina
            Match stamMatch = stamRegex.Match(text);
            if (!stamMatch.Success)
            {
                return false;
            }
            int stamMin = int.Parse(stamMatch.Groups[1].Value);
            int stamMax = int.Parse(stamMatch.Groups[2].Value);
            if (stamMin > stamMax)
            {
                // Condizione impossibile: stamina min > stamina max
                return false;
            }
            stamina = (stamMin, stamMax);

            // Cerca e controlla la match per Weight
            Match weightMatch = weightRegex.Match(text);
            if (!weightMatch.Success)
            {
                return false;
            }
            int weightMin = int.Parse(weightMatch.Groups[1].Value);
            int weightMax = int.Parse(weightMatch.Groups[2].Value);
            if (weightMin == 0 || weightMin + 150 > weightMax)
            {
                // Se weightMin è zero oppure weightMin+150 > weightMax, scarta il risultato
                return false;
            }
            weight = (weightMin, weightMax);

            return true;
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

using AutoClicker.Models.TM;
using System.Media;
using System.Text.RegularExpressions;

namespace AutoClicker.Service
{
    public class ReadLogTMService
    {
        private DateTime lastMacrocheckDateTime = new DateTime();
        private Pg _pg;
        private string _beepSound = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Sounds", "Beep.wav");
        public SoundPlayer _playerBeep;
        public Dictionary<string, int> RisorseRaccolte { get; }
                = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static readonly Regex _raccoltaRegex = new Regex(
        @"Hai raccolto\s*:\s*(\d+)\s+(.+?grezzo)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public Action<Dictionary<string, int>> RefreshRisorse { get; set; }
        public ReadLogTMService(Pg pg)
        {
            _pg = pg;
            _playerBeep = new SoundPlayer(_beepSound);
        }

        public Status ReadRecentLogs(string logFilePath)
        {
            var status = new Status();
            if (!File.Exists(logFilePath))
            {
                status.Error = "Il file di log non esiste.";
                return status;
            }

            // File che memorizza la posizione dell'ultimo byte letto
            string positionFilePath = logFilePath + ".pos";
            long lastPosition = 0;

            // Recupera l'ultima posizione salvata
            if (File.Exists(positionFilePath))
            {
                try
                {
                    string posStr = File.ReadAllText(positionFilePath);
                    long.TryParse(posStr, out lastPosition);
                }
                catch (Exception ex)
                {
                    // Gestisce eventuali errori nella lettura del file di posizione
                    Console.WriteLine($"Errore nel leggere il file di posizione: {ex.Message}");
                    lastPosition = 0; // Reset della posizione in caso di errore
                }
            }

            try
            {
                var lastMinute = DateTime.Now.AddMinutes(-1);
                var last2Minute = DateTime.Now.AddMinutes(-2);

                using (var fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // Controllo se il file è più piccolo dell'ultima posizione
                    // (potrebbe essere stato troncato o ruotato)
                    if (fs.Length < lastPosition)
                    {
                        Console.WriteLine("File più piccolo dell'ultima posizione letta, ripartendo dall'inizio.");
                        lastPosition = 0;
                    }

                    // Posiziona lo stream all'ultima posizione letta
                    fs.Position = lastPosition;

                    using (var sr = new StreamReader(fs))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            // Processa solo le nuove righe
                            var timestampMatch = Regex.Match(line, @"\[(\d{2}/\d{2}/\d{4} \d{2}:\d{2})\]");
                            if (timestampMatch.Success)
                            {
                                var timestamp = DateTime.ParseExact(timestampMatch.Groups[1].Value, "MM/dd/yyyy HH:mm", null);

                                if (timestamp >= last2Minute && (line.Contains("macrocheck") || line.Contains("antimacro")))
                                {
                                    if (lastMacrocheckDateTime != timestamp)
                                    {
                                        status.Macrocheck = true;
                                        _playerBeep.PlayLooping();
                                    }
                                    lastMacrocheckDateTime = timestamp;
                                }

                                if (timestamp >= lastMinute)
                                {
                                    if (line.Contains("troppo stanco"))
                                        status.Stamina = true;
                                    if (line.Contains("da scavare qui"))
                                        status.Move = true;
                                    if (line.Contains("troppo peso"))
                                    {
                                        status.MoveIrons = true;
                                        status.Stone = true;
                                    }
                                    if(line.Contains("attrezzo"))
                                        status.PickaxeBroke = true;
                                }

                                var raccoltaMatch = _raccoltaRegex.Match(line);
                                if (raccoltaMatch.Success)
                                {
                                    int qty = int.Parse(raccoltaMatch.Groups[1].Value);
                                    string risorsa = raccoltaMatch.Groups[2].Value.Trim();

                                    // Verifica se la risorsa è un minerale grezzo
                                    if (risorsa.EndsWith("grezzo", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Estrae il tipo di metallo rimuovendo "grezzo"
                                        string metallo = risorsa.Substring(0, risorsa.Length - "grezzo".Length).Trim();

                                        // Costruisce il nome del lingotto
                                        risorsa = $"Lingotti di {metallo}";
                                    }

                                    // Aggiunge o aggiorna il dizionario
                                    if (RisorseRaccolte.ContainsKey(risorsa))
                                        RisorseRaccolte[risorsa] += qty * 2;
                                    else
                                        RisorseRaccolte[risorsa] = qty * 2;

                                    RefreshRisorse.Invoke(RisorseRaccolte);
                                }

                            }
                        }

                        // Salva la nuova posizione nel file
                        lastPosition = fs.Position;
                        File.WriteAllText(positionFilePath, lastPosition.ToString());
                    }
                }

                return status;
            }
            catch (Exception ex)
            {
                status.Error = ex.Message;
                return status;
            }
        }

        public void StopSound()
        {
            if (_playerBeep != null)
                _playerBeep.Stop();
        }
    }
}
using AutoClicker.Models.TM;
using AutoClicker.Service;
using LogManager;
using System.Media;
using System.Text.RegularExpressions;

public class ReadLogTMService
{
    private DateTime lastMacrocheckDateTime = new DateTime();
    private Pg _pg;
    private static readonly Regex _raccoltaRegex = new Regex(
        @"Hai raccolto\s*:\s*(\d+)\s+(.+?grezzo)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public Action RefreshRisorse { get; set; }

    // Nuovo campo per tenere traccia dei controlli senza cambiamenti
    private int _consecutiveEmptyChecks = 0;
    private const int MAX_EMPTY_CHECKS = 3;

    public ReadLogTMService(Pg pg)
    {
        _pg = pg;
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
                Console.WriteLine($"Errore nel leggere il file di posizione: {ex.Message}");
                lastPosition = 0;
            }
        }

        try
        {
            var lastMinute = DateTime.Now.AddMinutes(-1);
            var last2Minute = DateTime.Now.AddMinutes(-2);
            bool foundNewLines = false; // Flag per controllare se abbiamo trovato nuove linee

            using (var fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                // Controllo se il file è più piccolo dell'ultima posizione
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
                        foundNewLines = true; // Abbiamo trovato almeno una nuova linea

                        // Processa solo le nuove righe
                        var timestampMatch = Regex.Match(line, @"\[(\d{2}/\d{2}/\d{4} \d{2}:\d{2})\]");
                        if (timestampMatch.Success)
                        {
                            var timestamp = DateTime.ParseExact(timestampMatch.Groups[1].Value, "MM/dd/yyyy HH:mm", null);

                            if (timestamp >= last2Minute && (line.Contains("macrocheck") || line.Contains("antimacro") || line.Contains("anti") || line.Contains("maco")))
                            {
                                if (lastMacrocheckDateTime != timestamp)
                                {
                                    status.Macrocheck = true;
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
                                    Logger.Loggin("Troppo peso.");
                                    status.Stone = true;
                                }
                                if (line.Contains("attrezzo"))
                                    status.PickaxeBroke = true;
                            }

                            var raccoltaMatch = _raccoltaRegex.Match(line);
                            if (raccoltaMatch.Success)
                            {
                                int qty = int.Parse(raccoltaMatch.Groups[1].Value);
                                string risorsa = raccoltaMatch.Groups[2].Value.Trim();

                                if (risorsa.EndsWith("grezzo", StringComparison.OrdinalIgnoreCase))
                                {
                                    string metallo = risorsa.Substring(0, risorsa.Length - "grezzo".Length).Trim();
                                    risorsa = $"Lingotti di {metallo}";
                                }

                                var mulo = _pg.Muli.FirstOrDefault(x => x.Selected);
                                mulo.AddRisorseQuantita(risorsa, qty);

                                RefreshRisorse.Invoke();
                            }
                        }
                    }

                    // Salva la nuova posizione nel file
                    lastPosition = fs.Position;
                    File.WriteAllText(positionFilePath, lastPosition.ToString());
                }
            }

            // Controllo per i controlli consecutivi senza nuove linee
            if (foundNewLines)
            {
                _consecutiveEmptyChecks = 0; // Reset del contatore se troviamo nuove linee
            }
            else
            {
                _consecutiveEmptyChecks++; // Incrementa il contatore
            }

            // Controllo se abbiamo superato il limite di controlli vuoti
            if (_consecutiveEmptyChecks >= MAX_EMPTY_CHECKS)
            {
                status.Error = "Il file di log non viene aggiornato.";
                // Reset del contatore per evitare messaggi multipli
                _consecutiveEmptyChecks = 0;
            }

            return status;
        }
        catch (Exception ex)
        {
            status.Error = ex.Message;
            return status;
        }
    }

}
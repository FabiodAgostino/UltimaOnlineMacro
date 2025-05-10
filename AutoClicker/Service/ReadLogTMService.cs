using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AutoClicker.Models.TM;
using AutoClicker.Service;
using LogManager;

public class ReadLogTMService
{
    private DateTime lastMacrocheckDateTime = DateTime.MinValue;
    private Pg _pg;
    private static readonly Regex _raccoltaRegex = new Regex(
        @"Hai raccolto\s*:\s*(\d+)\s+(.+?grezzo)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Pattern per riconoscere messaggi di controllo anti-macro
    private static readonly Regex _macroCheckRegex = new Regex(
        @"(?:ti\s+[eèé]\s+ar[rl]ivato|[eèé]\s+ar[rl]ivato|ar[rl]ivato)\s+un\s+contro[lI1][lI1]o\s+anti\s*[-_]?\s*macro",
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

        string positionFilePath = logFilePath + ".pos";
        long lastPosition = 0;

        if (File.Exists(positionFilePath))
        {
            try
            {
                long.TryParse(File.ReadAllText(positionFilePath), out lastPosition);
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
            bool foundNewLines = false;

            using (var fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (fs.Length < lastPosition)
                {
                    Console.WriteLine("File più piccolo dell'ultima posizione letta, ripartendo dall'inizio.");
                    lastPosition = 0;
                }

                fs.Position = lastPosition;
                using (var sr = new StreamReader(fs))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        foundNewLines = true;
                        var timestampMatch = Regex.Match(line, @"\[(\d{2}/\d{2}/\d{4} \d{2}:\d{2})\]");
                        if (timestampMatch.Success)
                        {
                            var timestamp = DateTime.ParseExact(
                                timestampMatch.Groups[1].Value,
                                "MM/dd/yyyy HH:mm",
                                null);

                            // Migliore rilevamento del controllo anti-macro
                            if (timestamp >= last2Minute && IsMacroCheckMessage(line))
                            {
                                if (lastMacrocheckDateTime != timestamp)
                                {
                                    status.Macrocheck = true;
                                    Logger.Loggin($"Rilevato controllo anti-macro: {line.Trim()}");
                                    TakeFullSizeScreenshot();
                                }
                                lastMacrocheckDateTime = timestamp;
                            }

                            // Altri controlli...
                            if (timestamp >= lastMinute)
                            {
                                if (line.Contains("troppo stanco"))
                                    status.Stamina = true;
                                if (line.Contains("da scavare qui"))
                                    status.Move = true;
                                if (line.Contains("troppo peso") || line.ToLower().Contains("il tuo zaino è pieno"))
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
                                    string metallo = risorsa.Substring(
                                        0,
                                        risorsa.Length - "grezzo".Length)
                                        .Trim();
                                    risorsa = $"Lingotti di {metallo}";
                                }

                                var mulo = _pg.Muli.FirstOrDefault(x => x.Selected);
                                if (mulo != null)
                                {
                                    mulo.AddRisorseQuantita(risorsa, qty);
                                    RefreshRisorse?.Invoke();
                                }
                            }
                        }
                    }

                    lastPosition = fs.Position;
                    File.WriteAllText(positionFilePath, lastPosition.ToString());
                }
            }

            if (foundNewLines)
                _consecutiveEmptyChecks = 0;
            else
                _consecutiveEmptyChecks++;

            if (_consecutiveEmptyChecks >= MAX_EMPTY_CHECKS)
            {
                status.Error = "Il file di log non viene aggiornato.";
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

    /// <summary>
    /// Determina se una riga di log contiene un messaggio di controllo anti-macro
    /// </summary>
    private bool IsMacroCheckMessage(string line)
    {
        if (line.ToLower().Contains("slogga") || line.ToLower().Contains("rilogga") || line.ToLower().Contains("apparso") || line.ToLower().Contains("non l'hai visto") || line.ToLower().Contains("tra un minuto") || line.ToLower().Contains("antimacro") || line.ToLower().Contains("anti"))
            return true;
            // Rimuovi i timestamp e normalizza
            var cleanLine = Regex.Replace(line, @"\[\d{2}/\d{2}/\d{4} \d{2}:\d{2}\]", "").Trim();
        // Verifica se la riga contiene "Sistema:" seguito da un messaggio
        if (cleanLine.StartsWith("Sistema:", StringComparison.OrdinalIgnoreCase))
        {
            cleanLine = cleanLine.Substring("Sistema:".Length).Trim();

            // Rimuovi le virgolette se presenti
            cleanLine = cleanLine.Trim('\'', '"');
        }

        // Normalizza caratteri ambigui
        var normalizedLine = NormalizeAmbiguousCharacters(cleanLine.ToLowerInvariant());

        // Prima prova con regex
        if (_macroCheckRegex.IsMatch(normalizedLine))
            return true;

        // Poi verifica parole chiave con fuzzy matching
        var keywords = new[] {
            "antimacro", "anti macro", "controllo", "controllo antimacro",
            "slogga", "rilogga", "macrocheck"
        };

        var wordCount = 0;
        foreach (var keyword in keywords)
        {
            if (FuzzyContains(normalizedLine, keyword))
                wordCount++;

            // Se almeno due parole chiave sono presenti, è molto probabile che sia un messaggio anti-macro
            if (wordCount >= 2)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Normalizza i caratteri ambigui, come la 'I' maiuscola con 'l' minuscola
    /// </summary>
    private string NormalizeAmbiguousCharacters(string input)
    {
        var result = input
            .Replace('I', 'l') // I maiuscola -> l minuscola
            .Replace('1', 'l') // 1 -> l minuscola
            .Replace('0', 'o') // 0 -> o minuscola
            .Replace('è', 'e')
            .Replace('é', 'e')
            .Replace('à', 'a')
            .Replace('ò', 'o')
            .Replace('ù', 'u');

        return result;
    }

    /// <summary>
    /// Verifica se una stringa contiene una sottostringa con tolleranza di errori
    /// </summary>
    private bool FuzzyContains(string source, string pattern, int maxDistance = 2)
    {
        if (string.IsNullOrEmpty(pattern)) return false;
        if (source.Contains(pattern)) return true;

        // Semplificazione: controlla anche parole separate
        var words = source.Split(new[] { ' ', ',', '.', ':', ';', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            if (LevenshteinDistance(word, pattern) <= maxDistance)
                return true;
        }

        // Controllo per frasi multi-parola
        if (pattern.Contains(" "))
        {
            return LevenshteinDistance(source, pattern) <= maxDistance * 2; // tolleranza maggiore per frasi
        }

        return false;
    }

    /// <summary>
    /// Calcola la distanza di Levenshtein tra due stringhe
    /// </summary>
    private int LevenshteinDistance(string s, string t)
    {
        int n = s.Length, m = t.Length;
        var d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; i++) d[i, 0] = i;
        for (int j = 0; j <= m; j++) d[0, j] = j;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }

    private void TakeFullSizeScreenshot()
    {
        try
        {
            // Ottieni il percorso dell'eseguibile
            string exePath = AppDomain.CurrentDomain.BaseDirectory;

            // Crea una directory per gli screenshot nella stessa cartella dell'eseguibile
            string screenshotDir = Path.Combine(exePath, "MacroCheckScreenshots");
            if (!Directory.Exists(screenshotDir))
                Directory.CreateDirectory(screenshotDir);

            // Dimensione dell'intero schermo
            var bounds = Screen.PrimaryScreen.Bounds;
            using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var filename = Path.Combine(screenshotDir, $"macrocheck_{timestamp}.png");
                bitmap.Save(filename, ImageFormat.Png);
                Logger.Loggin($"Screenshot salvato: {filename}");
            }
        }
        catch (Exception ex)
        {
            Logger.Loggin($"Errore durante lo screenshot: {ex.Message}");
        }
    }
}
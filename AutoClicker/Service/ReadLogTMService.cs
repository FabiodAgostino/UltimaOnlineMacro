using AutoClicker.Models.TM;
using System.IO;
using System.Media;
using System.Text.RegularExpressions;

namespace AutoClicker.Service
{
    public class ReadLogTMService
    {
        private DateTime lastMacrocheckDateTime = new DateTime();
        private Pg _pg;
        private string _beepSound = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Sounds", "Beep.wav");
        private SoundPlayer _playerBeep;
        public ReadLogTMService(Pg pg)
        {
            _pg = pg;
            _playerBeep = new SoundPlayer(_beepSound);
        }

        public Status ReadRecentLogs(string logFilePath)
        {
            var status = new Status();

            if (!File.Exists(logFilePath))
                status.Error = "Il file di log non esiste.";

            var lines = new List<string>();
            using (var fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs))
            {
                while (!sr.EndOfStream)
                {
                    lines.Add(sr.ReadLine());
                }
            }
            try
            {
                var lastMinute = DateTime.Now.AddMinutes(-1);
                var last2Minute = DateTime.Now.AddMinutes(-2);
                foreach (var line in lines.AsEnumerable().Reverse())
                {
                    var timestampMatch = Regex.Match(line, @"\[(\d{2}/\d{2}/\d{4} \d{2}:\d{2})\]");
                    if (timestampMatch.Success)
                    {
                        var timestamp = DateTime.ParseExact(timestampMatch.Groups[1].Value, "MM/dd/yyyy HH:mm", null);
                        if (timestamp >= last2Minute && (line.Contains("macrocheck") || line.Contains("antimacro")))
                        {
                            if(lastMacrocheckDateTime!=timestamp)
                            {
                                status.Macrocheck = true;
                                _playerBeep.PlayLooping();
                            }
                            lastMacrocheckDateTime = timestamp;
                        }

                        if (timestamp >= lastMinute)
                        {
                            // Verifica se ci sono minerali o overstone
                            if (line.Contains("stanco"))
                            {
                                status.Stamina = true;
                            }

                            if (line.Contains("da scavare qui"))
                            {
                                status.Move = true;
                            }

                            if(line.Contains("troppo peso"))
                            {
                                status.MoveIrons = true;
                                status.Stone = true;
                            }

                          
                        }
                        else
                        {
                            break;
                        }
                     
                    }
                }
                return status;
            }
            catch (Exception ex)
            {
                status.Error = ex.Message;
            }
            return status;
        }

        public void StopSound()
        {
            if(_playerBeep != null )
                _playerBeep.Stop();
        }
    }
}

using AutoClicker.Models.System;
using AutoClicker.Models.System.UltimaOnlineMacro.Models.System;
using AutoClicker.Utils;
using LogManager;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace UltimaOnlineMacro.Service
{
    public class SettingsService
    {
        private const string SettingsFileName = "settings.json";
        private readonly string _settingsFilePath;

        public SettingsService()
        {
            _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string jsonString = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(jsonString);

                    // Decodifica i percorsi con caratteri Unicode
                    if (settings != null)
                    {
                        if (!string.IsNullOrEmpty(settings.MacroPath))
                            settings.MacroPath = PathHelper.NormalizePath(settings.MacroPath);

                        if (!string.IsNullOrEmpty(settings.JournalPath))
                            settings.JournalPath = PathHelper.NormalizePath(settings.JournalPath);
                    }

                    Logger.Loggin($"Impostazioni caricate correttamente da {_settingsFilePath}",false,false);
                    return settings;
                }

                Logger.Loggin($"File impostazioni non trovato. Creazione nuove impostazioni di default.", false);
                return new AppSettings();
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore durante il caricamento delle impostazioni: {ex.Message}", true,true);
                return new AppSettings();
            }
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                // Configura le opzioni JSON per mantenere i caratteri speciali
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    // Permette di usare caratteri speciali senza convertirli in \uXXXX
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                };

                string jsonString = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_settingsFilePath, jsonString);
                Logger.Loggin($"Impostazioni salvate correttamente in {_settingsFilePath}",false,false);
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore durante il salvataggio delle impostazioni: {ex.Message}", true,true);
            }
        }
    }
}
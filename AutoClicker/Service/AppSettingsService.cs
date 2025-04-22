using AutoClicker.Models.System;
using AutoClicker.Models.System.UltimaOnlineMacro.Models.System;
using LogManager;
using System.Text.Json;

namespace UltimaOnlineMacro.Service
{
    public class SettingsService
    {
        private const string SettingsFileName = "settings.json";
        private readonly string _settingsFilePath;

        public SettingsService()
        {
            _settingsFilePath = Path.Combine(Directory.GetCurrentDirectory(), SettingsFileName);
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonString = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_settingsFilePath, jsonString);

                Logger.Loggin($"Impostazioni salvate correttamente in {_settingsFilePath}");
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore durante il salvataggio delle impostazioni: {ex.Message}", true);
            }
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string jsonString = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(jsonString);

                    Logger.Loggin($"Impostazioni caricate correttamente da {_settingsFilePath}");
                    return settings;
                }

                Logger.Loggin("File di impostazioni non trovato. Verranno utilizzate le impostazioni predefinite.");
                return new AppSettings();
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore durante il caricamento delle impostazioni: {ex.Message}", true);
                return new AppSettings();
            }
        }
    }
}
using Serilog;
using System.Diagnostics;
using System.IO;

namespace LogManager
{
    public static class LoggerConfig
    {
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return;

            // Prova a usare il processo corrente per ottenere la directory dell'eseguibile
            string appDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            // Se questo fallisce, usa AppContext.BaseDirectory
            if (string.IsNullOrEmpty(appDirectory))
            {
                appDirectory = AppContext.BaseDirectory;
            }

            string logPath = Path.Combine(appDirectory, "Logs", $"Log{DateTime.Today:dd-MM-yyyy}.txt");

            // Debug: aggiungi un log per vedere dove viene creato il file
            Console.WriteLine($"Tentando di creare log in: {logPath}");

            // Assicurati che la directory Logs esista
            Directory.CreateDirectory(Path.Combine(appDirectory, "Logs"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(logPath)
                .CreateLogger();

            _initialized = true;
        }
    }
}
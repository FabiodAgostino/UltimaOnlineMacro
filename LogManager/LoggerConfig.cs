using Serilog;
using System.IO;

namespace LogManager
{
    public static class LoggerConfig
    {
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return;

            string appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string logPath = Path.Combine(appDirectory, "Logs", $"Log{DateTime.Today:dd-MM-yyyy}.txt");

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
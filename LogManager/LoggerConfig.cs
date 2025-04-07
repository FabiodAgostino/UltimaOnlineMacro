using Serilog;

namespace LogManager
{
    public static class LoggerConfig
    {
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File($"../../../Logs/Log{DateTime.Today:dd-MM-yyyy}.txt")
                .CreateLogger();

            _initialized = true;
        }
    }
}

using Serilog;
using System.Windows.Controls;

namespace LogManager
{
    public static class Logger
    {
        public static TextBox _logTextBox;

        public static void Loggin(string message, bool err = false)
        {
            LoggerConfig.Initialize();
            if (_logTextBox != null)
            {
                _logTextBox.Dispatcher.Invoke(() =>
                {
                    _logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
                    _logTextBox.ScrollToEnd();
                });
            }

            if (err)
            {
                Log.Error(message);
                Console.Error.WriteLine(message);
            }
            else
            {
                Console.WriteLine(message);
                Log.Debug(message);
            }
        }
    }
}
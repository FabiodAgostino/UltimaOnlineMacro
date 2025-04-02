using System.Windows.Controls;

namespace UltimaOnlineMacro.ExtensionMethod
{
    public static class LogManager
    {
        private static TextBox _logTextBox;

        public static void Initialize(TextBox textBox)
        {
            _logTextBox = textBox;
        }

        public static void Log(string message)
        {
            if (_logTextBox != null)
            {
                _logTextBox.Dispatcher.Invoke(() =>
                {
                    _logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
                    _logTextBox.ScrollToEnd();
                });
            }
        }
    }
}

using LogManager;
using Serilog;
using System.Windows.Controls;

namespace UltimaOnlineMacro.Service
{
    public class Logger : LogManager.Logger
    {
        private TextBox _logTextBox;

        public Logger(TextBox logTextBox)
        {
            _logTextBox = logTextBox;
        }

        public override void Initialize(object textBox)
        {
            _logTextBox = (TextBox)textBox;
        }

        public override void Loggin(string message, bool err=false)
        {
            if (_logTextBox != null)
            {
                _logTextBox.Dispatcher.Invoke(() =>
                {
                    _logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
                    _logTextBox.ScrollToEnd();
                });
            }

            Serilog(message, err);
        }
    }
}

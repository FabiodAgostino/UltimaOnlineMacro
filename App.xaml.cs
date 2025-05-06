using System.Windows;

namespace UltimaOnlineMacro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            // Crea e mostra lo splash screen
            SplashScreen splash = new SplashScreen();
            splash.Show();
            splash.UpdateStatus("Caricamento componenti...");
            MainWindow mainWindow = new();
            splash.UpdateStatus("Caricamento servizi...");
            await mainWindow.LoadServices();

            splash.UpdateStatus("Caricamento settaggi...");
            mainWindow.LoadSettings();

            splash.UpdateStatus("Caricamento strumenti...");
            mainWindow.LoadTools();
            mainWindow.LoadMisc();

            // Crea la main window

            // Chiudi lo splash screen
            await splash.CloseSplashScreen();
            // Mostra la main window
            mainWindow.Show();
            mainWindow.Activate();
            this.MainWindow = mainWindow;

            base.OnStartup(e);
        }
    }
}
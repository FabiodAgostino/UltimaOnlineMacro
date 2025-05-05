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
            MainWindow mainWindow = new MainWindow();

            // Simula caricamento o fai operazioni di inizializzazione
            await Task.Run(async () =>
            {
                splash.UpdateStatus("Caricamento risorse...");
                await Task.Delay(500); // Simula lavoro

                splash.UpdateStatus("Inizializzazione componenti...");
                await Task.Delay(500); // Simula lavoro

                splash.UpdateStatus("Preparazione interfaccia...");
                await Task.Delay(500); // Simula lavoro
            });

            // Crea la main window
            
            // Chiudi lo splash screen
            await splash.CloseSplashScreen();
            // Mostra la main window
            mainWindow.Show();
            mainWindow.Activate();
            
            // Importante: la MainWindow ora è la finestra principale
            this.MainWindow = mainWindow;

            base.OnStartup(e);
        }
    }
}
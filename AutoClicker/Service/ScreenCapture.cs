using System.Drawing.Imaging;
using Timer = System.Threading.Timer;
namespace AutoClicker.Service
{


    public class ScreenCapture
    {
        private Timer captureTimer;
        private string outputDirectory;
        private int captureCount = 0;
        private int halfWidth;
        private int halfHeight;

        public ScreenCapture(int halfWidth = 100, int halfHeight = 100)
        {
            this.halfWidth = halfWidth;
            this.halfHeight = halfHeight;

            // Creo la directory "newDataset" nel percorso corrente
            outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "newDataset");

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Inizializza il timer con intervallo di 5 secondi (5000 ms)
            captureTimer = new Timer(CaptureTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start()
        {
            // Avvia il timer con intervallo di 5 secondi
            captureTimer.Change(0, 1000);
            Console.WriteLine("Cattura schermo avviata. Le immagini verranno salvate in: " + outputDirectory);
        }

        public void Stop()
        {
            // Ferma il timer
            captureTimer.Change(Timeout.Infinite, Timeout.Infinite);
            Console.WriteLine("Cattura schermo fermata. Sono state catturate " + captureCount + " immagini.");
        }

        private void CaptureTimerCallback(object state)
        {
            try
            {
                // Utilizzo il metodo CaptureCenterScreenshot fornito
                var (bitmap, rectangle) = CaptureCenterScreenshot();

                // Genera un nome file unico basato sulla data/ora
                string fileName = $"Mulo{Guid.NewGuid()}.png";
                string filePath = Path.Combine(outputDirectory, fileName);

                // Salva l'immagine
                bitmap.Save(filePath, ImageFormat.Png);

                // Incrementa il contatore e libera le risorse
                captureCount++;
                bitmap.Dispose();

                Console.WriteLine($"Screenshot salvato: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la cattura: {ex.Message}");
            }
        }

        // Utilizzo esattamente il metodo che mi hai fornito
        public static (Bitmap bitmap, Rectangle rectangle) CaptureCenterScreenshot(int halfWidth = 100, int halfHeight = 100)
        {
            Rectangle? screen = null;
            if (Screen.PrimaryScreen != null)
                screen = Screen.PrimaryScreen.Bounds;
            else
                throw new Exception("Non trovo le dimensioni dello schermo primario");
            Rectangle screenBounds = screen.Value;
            // Calcola il centro dello schermo
            int centerX = screenBounds.X + screenBounds.Width / 2;
            int centerY = screenBounds.Y + screenBounds.Height / 2;
            // Calcola l'angolo superiore sinistro del rettangolo centrato
            int startX = centerX - halfWidth - 280;
            int startY = centerY - halfHeight - 200;
            // Calcola larghezza e altezza totali
            int width = halfWidth * 2;
            int height = halfHeight * 2;
            // Crea la bitmap della dimensione desiderata
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            // Copia la porzione di schermo nella bitmap
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(startX, startY, 0, 0, new Size(width, height));
            }
            return (bitmap, new Rectangle(startX, startY, width, height));
        }
    }
}

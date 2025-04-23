using Emgu.CV;
using Emgu.CV.Structure;
using LogManager;
using System.Drawing.Imaging;

namespace AutoClicker.Service.ExtensionMethod
{
    public static class Image
    {
        public static Image<Bgr, byte> BitmapToImage(this Bitmap bitmap)
        {
            // Blocca il bitmap per ottenere i dati dei pixel
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb // Assicurati che sia a 24 bit
            );

            // Crea un'istanza di Image<Bgr, byte> usando i dati del bitmap
            Image<Bgr, byte> img = new Image<Bgr, byte>(bitmap.Width, bitmap.Height, bitmapData.Stride, bitmapData.Scan0);

            // Sblocca il bitmap dopo la conversione
            bitmap.UnlockBits(bitmapData);

            return img;
        }

        public static Image<Bgr, byte> CaptureRegion(this Rectangle region)
        {
            try
            {
                Bitmap screenshot = new Bitmap(region.Width, region.Height);
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(region.Location, Point.Empty, region.Size);
                }

                // Creazione di Image<Gray, float> con lo stesso formato del Bitmap
                Image<Bgr, byte> image = new Image<Bgr, byte>(region.Width, region.Height);

                // Copia dei dati del Bitmap nei pixel dell'immagine
                for (int y = 0; y < region.Height; y++)
                {
                    for (int x = 0; x < region.Width; x++)
                    {
                        // Ottieni il colore del pixel
                        Color pixelColor = screenshot.GetPixel(x, y);
                        // Calcola la luminanza come media dei valori R, G e B (per scala di grigi)
                        float grayValue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3f;
                        // Imposta il valore nel Image<Gray, float>
                        image[y, x] = new Bgr(pixelColor);
                    }
                }

                return image;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
        }

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

        public static Bitmap CaptureFullScreenshot()
        {
            Rectangle screenBounds;
            if (Screen.PrimaryScreen != null)
                screenBounds = Screen.PrimaryScreen.Bounds;
            else
                throw new Exception("Non trovo le dimensioni dello schermo primario");

            // Crea una bitmap delle dimensioni dell'intero schermo
            Bitmap bitmap = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format24bppRgb);

            // Copia l'intero schermo nella bitmap
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0, screenBounds.Size);
            }

            return bitmap;
        }

        public static Bitmap CaptureRegionBitmap(Rectangle region)
        {
            Bitmap bmp = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
            try
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(region.Location, new Point(0, 0), region.Size, CopyPixelOperation.SourceCopy);
                }
            }catch
            {
                Logger.Loggin("Qualcosa è andato storto in CaptureRegionBitmap");
            }
           

            return bmp;
        }
    }
}
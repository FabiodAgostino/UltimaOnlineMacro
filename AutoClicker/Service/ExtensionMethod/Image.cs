using Emgu.CV;
using Emgu.CV.Structure;
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

        public static Image<Bgra, byte> BitmapToImageBgra(this Bitmap bitmap)
        {
            // Converti il bitmap in Format32bppArgb se necessario
            Bitmap bmp32 = bitmap;
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                bmp32 = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp32))
                {
                    g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                }
            }

            // Blocca il bitmap per ottenere i dati dei pixel (in formato 32bppArgb che include l'alpha)
            BitmapData bitmapData = bmp32.LockBits(
                new Rectangle(0, 0, bmp32.Width, bmp32.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb // Usa 32 bit per includere il canale alpha
            );

            // Crea un'istanza di Image<Bgra, byte> usando i dati del bitmap
            Image<Bgra, byte> img = new Image<Bgra, byte>(bmp32.Width, bmp32.Height, bitmapData.Stride, bitmapData.Scan0);

            // Sblocca il bitmap dopo la conversione
            bmp32.UnlockBits(bitmapData);

            // Se abbiamo creato un nuovo bitmap, liberiamolo
            if (bmp32 != bitmap)
            {
                bmp32.Dispose();
            }

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


        public static Image<Bgr, byte> Prova(this (Bitmap bitmap, Rectangle rectangle) screenImage)
        {
            // Crea una nuova Bitmap forzando il formato a 24bppRgb
            Bitmap safeBitmap = new Bitmap(screenImage.bitmap.Width, screenImage.bitmap.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(safeBitmap))
            {
                g.DrawImage(screenImage.bitmap, new Rectangle(0, 0, safeBitmap.Width, safeBitmap.Height));
            }

            // Usa il costruttore di Emgu CV che copia i dati
            Image<Bgr, byte> imSafe = safeBitmap.BitmapToImage();
            return imSafe;
        }

        public static Image<Bgr, byte> ConvertSafe(this Bitmap bitmap)
        {
            // Crea una nuova Bitmap forzando il formato a 24bppRgb
            Bitmap safeBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(safeBitmap))
            {
                g.DrawImage(bitmap, new Rectangle(0, 0, safeBitmap.Width, safeBitmap.Height));
            }

            Image<Bgr, byte> imSafe = safeBitmap.BitmapToImage();
            return imSafe;
        }

    }
}

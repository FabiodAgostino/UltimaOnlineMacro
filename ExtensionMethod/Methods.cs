using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Point = System.Drawing.Point;

namespace UltimaOnlineMacro.ExtensionMethod
{
    public static class Methods
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

        public static Image<Gray, float> CaptureRegion(this Rectangle region)
        {
            try
            {
                Bitmap screenshot = new Bitmap(region.Width, region.Height);
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(region.Location, Point.Empty, region.Size);
                }

                // Creazione di Image<Gray, float> con lo stesso formato del Bitmap
                Image<Gray, float> image = new Image<Gray, float>(region.Width, region.Height);

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
                        image[y, x] = new Gray(grayValue);
                    }
                }

                return image;
            }
            catch (ArgumentException ex)
            {
                LogManager.Log($"Errore: Dimensione non supportata. {ex.Message}");
                return null;
            }
        }
    }

}

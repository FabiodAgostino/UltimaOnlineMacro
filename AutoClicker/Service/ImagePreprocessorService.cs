using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;
using System.Drawing.Imaging;
using ColorMatrix = System.Drawing.Imaging.ColorMatrix;
using Rectangle = System.Drawing.Rectangle;

namespace AutoClicker.Service
{
    class ImagePreprocessorService
    {
        public static MemoryStream ProcessImage(Bitmap bmp)
        {
            // Converte la Bitmap in MemoryStream PNG
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);

            // Carica in ImageSharp
            using var image = Image.Load<Rgba32>(ms);

            // Preprocessing light: Scala di grigi, resize, contrasto e sharpening.
            image.Mutate(ctx => ctx
                 .Grayscale()
                 .Resize(image.Width * 3, image.Height * 3)
                 .GaussianSharpen(0.5f)
            );

            // Salva senza binarizzazione forzata
            var processedStream = new MemoryStream();
            image.SaveAsPng(processedStream);
            processedStream.Seek(0, SeekOrigin.Begin);
            return processedStream;
        }

       

    }
}

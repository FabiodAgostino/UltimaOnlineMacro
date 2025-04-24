using AutoClicker.Service.ExtensionMethod;
using Emgu.CV;
using Emgu.CV.Structure;

namespace AutoClicker.Models.System
{
    public class ImageTemplate
    {
        public Bitmap Image { get; set; }
        public Image<Bgr, byte> Template { get; set; }
        public string Name { get; set; }

        public ImageTemplate(string path)
        {
            CreateImage(path);
        }

        public void CreateImage(string path)
        {
            try
            {
                // Ottieni il percorso base dell'applicazione (più affidabile di AppDomain.CurrentDomain.BaseDirectory)
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Prova vari percorsi possibili
                string[] possiblePaths = new[]
                {
                Path.Combine(baseDirectory, "Assets", "Images", path),
                Path.Combine(baseDirectory, path),
                Path.Combine(baseDirectory, "Assets", path),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", path)
            };

                string imagePath = null;

                // Cerca il file in tutti i percorsi possibili
                foreach (string possiblePath in possiblePaths)
                {
                    if (File.Exists(possiblePath))
                    {
                        imagePath = possiblePath;
                        break;
                    }
                }

                if (imagePath != null)
                {
                    Image = new Bitmap(imagePath);
                    Template = Image.BitmapToImage().Convert<Bgr, byte>();
                    Name = path;
                }
                else
                {
                    // Se non riesci a trovare l'immagine, crea un'immagine placeholder
                    Console.WriteLine($"ERRORE: Impossibile trovare l'immagine: {path}");
                    Image = new Bitmap(10, 10);
                    Template = Image.BitmapToImage().Convert<Bgr, byte>();
                    Name = path;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRORE durante il caricamento dell'immagine {path}: {ex.Message}");
                // Crea un'immagine di fallback
                Image = new Bitmap(10, 10);
                Template = Image.BitmapToImage().Convert<Bgr, byte>();
                Name = path;
            }
        }
    }
}
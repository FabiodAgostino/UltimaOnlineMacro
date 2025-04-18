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
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Images", path);
            Image = new Bitmap(imagePath);
            Template = Image.BitmapToImage().Convert<Bgr, byte>();
            Name = path;
        }
    }
}
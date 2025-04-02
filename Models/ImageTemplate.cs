using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
using UltimaOnlineMacro.ExtensionMethod;
using System.IO;

namespace UltimaOnlineMacro.Models
{
    public class ImageTemplate
    {
        public Bitmap Image { get; set; }
        public Image<Gray, float> Template { get; set; }

        public ImageTemplate(string path)
        {
            CreateImage(path);
        }

        public void CreateImage(string path)
        {
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Images", path);
            Image = new Bitmap(imagePath);
            Template = Image.BitmapToImage().Convert<Gray, float>();
        }

    }
}

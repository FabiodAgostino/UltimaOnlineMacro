using AutoClicker.Service.ExtensionMethod;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace AutoClicker.Models
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

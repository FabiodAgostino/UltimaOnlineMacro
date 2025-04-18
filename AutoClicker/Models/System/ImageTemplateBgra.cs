using Emgu.CV;
using Emgu.CV.Structure;

namespace AutoClicker.Models.System
{
    public class ImageTemplateBgra
    {
        public Bitmap Image { get; set; }
        public Image<Bgra, byte> Template { get; set; }
        public string Name { get; set; }

        public ImageTemplateBgra(string path)
        {
            CreateImage(path);
        }

        public void CreateImage(string path)
        {
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Images", path);
            Image = new Bitmap(imagePath);
            Template = Image.ToImage<Bgra, byte>();
            Name = path;
        }
    }
}
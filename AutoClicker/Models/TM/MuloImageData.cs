using Microsoft.ML.Data;

namespace AutoClicker.Models.TM
{
    public class MuloImageData
    {
        [LoadColumn(0)]
        public string ImagePath { get; set; }

        [LoadColumn(1)]
        public string Label { get; set; }

        public static MuloImageData FromFile(string imagePath, string label)
        {
            return new MuloImageData
            {
                ImagePath = imagePath,
                Label = label
            };
        }
    }
}

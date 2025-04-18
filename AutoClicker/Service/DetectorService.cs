using DetectorModel.Services;
using static AutoClicker.Utils.User32DLL;

namespace AutoClicker.Service
{
    public class DetectorService
    {
        public ObjectDetector ObjectDetector { get; set; } = new();

        public async Task<POINT> GetPointToClickPackHorse()
        {
            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "mulo-detector.zip");
            string tempImagePath = Path.Combine(Path.GetTempPath(), $"CenterScreen_{Guid.NewGuid()}.png");
            var center = ExtensionMethod.Image.CaptureCenterScreenshot();
            center.bitmap.Save(tempImagePath);
            var result = await ObjectDetector.DetectObjectAsync(Path.Combine(Directory.GetCurrentDirectory(), "CenterScreen.png"), modelPath);
            File.Delete(tempImagePath);
            return new POINT() { X = (int)result.X, Y = (int)result.Y };
        }
    }
}
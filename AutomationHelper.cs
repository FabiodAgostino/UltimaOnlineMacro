using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using WindowsInput;
using AutoClicker.Service.ExtensionMethod;
namespace UltimaOnlineMacro
{
    public static class AutomationHelper
    {
        // Cattura uno screenshot di una regione specifica
        public static Bitmap CaptureRegion(Rectangle region)
        {
            Bitmap bmp = new Bitmap(region.Width, region.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(region.Location, Point.Empty, region.Size);
            }
            return bmp;
        }

        // Cerca l'immagine del piccone nella regione dello zaino usando il template matching.
        // Restituisce le coordinate dello schermo (se trovate) oppure null.
        public static Point? FindPickaxeInRegion(Rectangle backpackRegion, string templateImagePath, double threshold = 0.8)
        {
            using (Bitmap regionBmp = CaptureRegion(backpackRegion))
            using (Image<Bgr, byte> source = regionBmp.BitmapToImage())
            using (Image<Bgr, byte> template = new Image<Bgr, byte>(templateImagePath))
            {
                using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                {
                    double[] minVal = new double[1];
                    double[] maxVal = new double[1];
                    Point[] minLoc = new Point[1];
                    Point[] maxLoc = new Point[1];

                    result.MinMax(out minVal, out maxVal, out minLoc, out maxLoc);

                    if (maxVal[0] >= threshold)
                    {
                        return new Point(maxLoc[0].X + backpackRegion.X, maxLoc[0].Y + backpackRegion.Y);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }


        // Simula il trascinamento da una posizione di partenza a una destinazione
        public static void DragItem(Point start, Point end, int durationMs = 500)
        {
            var sim = new InputSimulator();

            // Converte le coordinate in valori normalizzati [0, 65535]
            double normalizedXStart = (double)start.X * 65535 / System.Windows.SystemParameters.PrimaryScreenWidth;
            double normalizedYStart = (double)start.Y * 65535 / System.Windows.SystemParameters.PrimaryScreenHeight;
            double normalizedXEnd = (double)end.X * 65535 / System.Windows.SystemParameters.PrimaryScreenWidth;
            double normalizedYEnd = (double)end.Y * 65535 / System.Windows.SystemParameters.PrimaryScreenHeight;

            // Sposta il mouse alla posizione di partenza
            sim.Mouse.MoveMouseTo(normalizedXStart, normalizedYStart);
            Thread.Sleep(100);
            sim.Mouse.LeftButtonDown();
            Thread.Sleep(100);

            // Calcola il numero di step per un movimento fluido
            int steps = 20;
            for (int i = 1; i <= steps; i++)
            {
                double newX = normalizedXStart + (normalizedXEnd - normalizedXStart) * i / steps;
                double newY = normalizedYStart + (normalizedYEnd - normalizedYStart) * i / steps;
                sim.Mouse.MoveMouseTo(newX, newY);
                Thread.Sleep(durationMs / steps);
            }
            sim.Mouse.LeftButtonUp();
        }
    }
}

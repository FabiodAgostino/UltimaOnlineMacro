using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
using System.IO;
using AutoClicker.Service.ExtensionMethod;
using AutoClicker.Models.System;
using LogManager;
using AutoClicker.Service;

namespace AutoClicker.Models.TM
{
    public class Pickaxe
    {
        public int Y { get; set; }
        public int X { get; set; }
        public bool IsFound { get; set; }
        public Pickaxe(Rectangle region, ImageTemplate imageTemplate)
        {
            GetPosition(region, imageTemplate);
        }

        public void GetPosition(Rectangle region, ImageTemplate imageTemplate)
        {
            var logger = new AutoClickerLogger();
            var screenImage = region.CaptureRegion();

            // Esegui il template matching
            using (Mat result = new Mat())
            {
                CvInvoke.MatchTemplate(screenImage, imageTemplate.Template, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);

                // Trova la posizione migliore del piccone
                double minVal = 0;
                double maxVal = 1;
                Point minLoc = new Point();
                Point maxLoc = new Point();

                CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                if (maxVal > 0.8) // Imposta una soglia di somiglianza appropriata
                {
                    Point picconePosition = maxLoc;
                    logger.Loggin($"Piccone trovato a: {picconePosition}");

                    X = region.X + picconePosition.X + imageTemplate.Image.Width / 2;
                    Y = region.Y + picconePosition.Y + imageTemplate.Image.Height / 2;
                    IsFound = true;
                }
                else
                {
                    IsFound = false;
                    logger.Loggin("Piccone non trovato.");
                }
            }
        }
    }
}

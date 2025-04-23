using AutoClicker.Models.System;
using AutoClicker.Service;
using AutoClicker.Service.ExtensionMethod;
using Emgu.CV;
using Emgu.CV.Structure;
using LogManager;

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
            // Acquisisci screenshot (ritorna Image<Bgr, byte>)
            var screenImage = region.CaptureRegion();

            // Converto sia immagine che template in grayscale (8-bit)
            var grayScreen = screenImage.Convert<Gray, byte>();
            var grayTemplate = imageTemplate.Template.Convert<Gray, byte>();

            using (Mat result = new Mat())
            {
                CvInvoke.MatchTemplate(grayScreen, grayTemplate, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);

                // Trova il valore massimo e la sua posizione
                double minVal = 0, maxVal = 0;
                Point minLoc = new Point(), maxLoc = new Point();
                CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                if (maxVal > 0.8) // Soglia standard
                {
                    Point pos = maxLoc;
                    Logger.Loggin($"Piccone trovato a: {pos}");

                    X = region.X + pos.X + imageTemplate.Template.Width / 2;
                    Y = region.Y + pos.Y + imageTemplate.Template.Height / 2;
                    IsFound = true;
                }
                else
                {
                    IsFound = false;
                    Logger.Loggin("Piccone non trovato.");
                }
            }
        }
    }
}
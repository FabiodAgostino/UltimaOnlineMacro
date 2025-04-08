using AutoClicker.Models.System;
using AutoClicker.Service;
using AutoClicker.Service.ExtensionMethod;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace AutoClicker.Models.TM
{
    public class Iron
    {
        public int Y { get; set; }
        public int X { get; set; }
        public bool IsFound { get; set; }
        public Iron(Rectangle region, ImageTemplate imageTemplate)
        {
            GetPosition(region, imageTemplate);
        }

        public void GetPosition(Rectangle region, ImageTemplate imageTemplate)
        {
            var logger = new AutoClickerLogger();

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

                logger.Loggin($"MaxVal ottenuto: {maxVal}");
                grayScreen.Save("debug_screen_iron.png");
                grayTemplate.Save("debug_template_iron.png");
                if (maxVal > 0.5) // Soglia standard
                {
                    Point pos = maxLoc;
                    logger.Loggin($"Oggetto trovato a: {pos}");

                    X = region.X + pos.X + imageTemplate.Template.Width / 2;
                    Y = region.Y + pos.Y + imageTemplate.Template.Height / 2;
                    IsFound = true;
                }
                else
                {
                    IsFound = false;
                    logger.Loggin("Oggetto non trovato.");
                }
            }
        }
    }
}

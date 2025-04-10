using AutoClicker.Models.TM;
using AutoClicker.Service.ExtensionMethod;
using AutoClicker.Service;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
using AutoClicker.Library;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using Emgu.CV.XObjdetect;
using Emgu.CV.CvEnum;

namespace AutoClicker.Models.System
{
    public class Regions
    {
        public Pickaxe Pickaxe { get; set; } // Definisce la regione del piccone selezionata
        public Rectangle BackpackRegion { get; set; } // Definisce la regione dello zaino selezionata
        public Rectangle PaperdollRegion { get; set; } // Definisce la regione del paperdoll selezionata
        public Rectangle PaperdollPickaxeRegion { get; set; } // Definisce la regione del piccone sul paperdoll selezionata
        public Rectangle GameRegion { get; set; } // Definisce la regione del game selezionata
        public string HaveValue()
        {
            if (BackpackRegion == Rectangle.Empty)
                return "Seleziona la regione dello zaino";
            //if (PaperdollRegion == Rectangle.Empty)
            //    return "Seleziona la regione del paperdoll";
            if (Pickaxe is null || !Pickaxe.IsFound)
                return "Nessun piccone presente nello zaino";

            return string.Empty;
        }

        public Rectangle GetMuloRegion()
        {
            var logger = new AutoClickerLogger();
            var screenImage = AutoClicker.Service.ExtensionMethod.Image.CaptureCenterScreenshot();

            // Converti le immagini in Mat
            Mat sceneImage = screenImage.bitmap.ToImage<Bgra, byte>().Mat;
            Mat templateImage = SavedImageTemplate.ImagesTemplateMulo[0].Template.Mat;

            // Esegui template matching
            Mat result = new Mat();
            CvInvoke.MatchTemplate(sceneImage, templateImage, result, TemplateMatchingType.CcoeffNormed);

            double minVal = 0, maxVal = 0;
            Point minLoc = new Point(), maxLoc = new Point();
            CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

            // Se la corrispondenza è abbastanza buona (ad esempio, sopra 0.7)
            if (maxVal > 0.7)
            {
                // Crea rettangolo basato sulla posizione e dimensione del template
                Rectangle rect = new Rectangle(
                    maxLoc.X,
                    maxLoc.Y,
                    templateImage.Width,
                    templateImage.Height
                );

                // Simula click
                var service = new MouseInputSimulator();
                service.SimulateDoubleClick10Times(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

                return rect;
            }

            return Rectangle.Empty;
        }

    }
}

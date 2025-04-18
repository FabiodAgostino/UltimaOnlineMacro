using AutoClicker.Models.TM;
using AutoClicker.Utils;

namespace AutoClicker.Models.System
{
    public class Regions
    {
        public Pickaxe? Pickaxe { get; set; } // Definisce la regione del piccone selezionata
        public Rectangle BackpackRegion { get; set; } // Definisce la regione dello zaino selezionata
        public Rectangle PaperdollRegion { get; set; } // Definisce la regione del paperdoll selezionata
        public Rectangle PaperdollPickaxeRegion { get; set; } // Definisce la regione del piccone sul paperdoll selezionata
        public Rectangle GameRegion { get; set; } // Definisce la regione del game selezionata
        public Rectangle StatusRegion { get; set; } // Definisce la regione del game selezionata

        public User32DLL.POINT FoodXY { get; set; }
        public User32DLL.POINT WaterXY { get; set; }

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
    }
}
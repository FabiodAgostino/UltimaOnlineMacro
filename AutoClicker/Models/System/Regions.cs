using AutoClicker.Models.TM;
using System.Drawing;

namespace AutoClicker.Models.System
{
    public class Regions
    {
        public Pickaxe Pickaxe { get; set; } // Definisce la regione del piccone selezionata
        public Rectangle BackpackRegion { get; set; } // Definisce la regione dello zaino selezionata
        public Rectangle PaperdollRegion { get; set; } // Definisce la regione del paperdoll selezionata
        public Rectangle PaperdollPickaxeRegion { get; set; } // Definisce la regione del piccone sul paperdoll selezionata
        public Rectangle GameRegion { get; set; } // Definisce la regione del game selezionata
        public Rectangle MuloRegion { get; set; } // Definisce dov'è situato il mulo

        public string HaveValue()
        {
            if (BackpackRegion == Rectangle.Empty)
                return "Seleziona la regione dello zaino";
            //if (PaperdollRegion == Rectangle.Empty)
            //    return "Seleziona la regione del paperdoll";
            if (MuloRegion == Rectangle.Empty)
                return "Seleziona la regione dello zaino del mulo";
            if (Pickaxe is null || !Pickaxe.IsFound)
                return "Nessun piccone presente nello zaino";

            return string.Empty;
        }

    }
}

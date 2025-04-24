namespace AutoClicker.Models.TM
{
    public class RisorsaQuantita
    {
        public string Risorsa { get; set; }
        public int Quantita { get => QuantitaGrezza*2;}

        public int QuantitaGrezza { get; set; }
    }
}

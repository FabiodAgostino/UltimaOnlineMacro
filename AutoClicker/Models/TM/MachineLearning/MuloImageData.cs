namespace AutoClicker.Models.TM.MachineLearning
{
    public class MuloImageData
    {
        public string ImagePath { get; set; }

        // Aggiungi questa proprietà per allineare lo schema
        public string Label { get; set; } = "NonMulo";
    }
}
namespace AutoClicker.Models.TM
{
    public class Mulo
    {
        public MuloType Type { get; set; }
        public string Name { get => Type == MuloType.MULO_DA_SOMA ? "Mulo da Soma" : "Lama Portatore"; }

        public int ActualOre
        {
            get
            {
                return RisorseQuantita.Sum(x => x.QuantitaGrezza);
            }
        }

        private int _actualOre { get; set; }

        public List<RisorsaQuantita> RisorseQuantita { get; set; } = new();


        public const int MaxOre = 800;
        public bool Selected { get; set; } = false;

        public Mulo(MuloType type, bool selected, Action muloFull)
        {
            Type = type;
            Selected = selected;
        }

        public void AddRisorseQuantita(string risorsa, int quantita)
        {
            var risorsaEsistente = RisorseQuantita.FirstOrDefault(r => r.Risorsa == risorsa);

            if (risorsaEsistente != null)
            {
                risorsaEsistente.QuantitaGrezza += quantita;
            }
            else
            {
                RisorseQuantita.Add(new RisorsaQuantita { Risorsa = risorsa, QuantitaGrezza = quantita });
            }
        }
    }

    public enum MuloType
    {
        MULO_DA_SOMA,
        LAMA_PORTATORE
    }
}
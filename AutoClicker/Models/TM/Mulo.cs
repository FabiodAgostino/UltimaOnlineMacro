namespace AutoClicker.Models.TM
{
    public class Mulo
    {
        public MuloType Type { get; set; }

        public int ActualOre
        {
            get => _actualOre; set
            {
                _actualOre = value;
                if (_actualOre + 50 > MaxOre)
                {
                    MuloFull.Invoke();
                }
            }
        }

        private int _actualOre { get; set; }

        public const int MaxOre = 800;
        public bool Selected { get; set; } = false;
        public Action MuloFull { get; set; }

        public Mulo(MuloType type, bool selected, Action muloFull)
        {
            Type = type;
            Selected = selected;
            MuloFull = muloFull;
        }
    }

    public enum MuloType
    {
        MULO_DA_SOMA,
        LAMA_PORTATORE
    }
}
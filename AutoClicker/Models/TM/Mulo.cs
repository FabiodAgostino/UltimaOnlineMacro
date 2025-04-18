namespace AutoClicker.Models.TM
{
    public class Mulo
    {
        public MuloType Type { get; set; }

        public int ActualStone
        {
            get => _actualStone; set
            {
                _actualStone = value;
                if (_actualStone + 100 > MaxStone)
                {
                    MuloFull.Invoke();
                }
            }
        }

        private int _actualStone { get; set; }

        public const int MaxStone = 2000;
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
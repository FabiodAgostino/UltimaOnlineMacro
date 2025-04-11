namespace AutoClicker.Models.TM
{
    public class Status
    {
        public bool Stone { get; set; }
        public bool Stamina { get; set; }
        public bool MoveIrons { get; set; }
        public bool Move { get; set; }
        public bool PickaxeBroke { get; set; }
        public string Error { get; set; }
        public bool Macrocheck { get; set; }
    }

    public class StatusBar
    {
        public (int value, int max) Stamina { get; set; }
        public (int value, int max) Stone { get; set; }
    }
}

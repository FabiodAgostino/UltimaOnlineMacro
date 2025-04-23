namespace AutoClicker.Models.TM
{
    public class Macro
    {
        public List<Keys> MacroKeys { get; set; }
        public double Delay { get; set; }
        private int _repetitions { get; set; }
        public Action<int> UpdateRepetitions { get; set; }

        public List<Keys> MacroFuria { get; set; }
        public List<Keys> MacroSaccaRaccolta { get; set; }

        public Macro(List<Keys> macroKeys, double delay, Action<int> action)
        {
            MacroKeys = macroKeys;
            Delay = delay;
            UpdateRepetitions = action;
        }

        public void UpdateRepetitionsMethod()
        {
            _repetitions++;
            if (UpdateRepetitions != null)
                UpdateRepetitions.Invoke(_repetitions);
        }
    }
}
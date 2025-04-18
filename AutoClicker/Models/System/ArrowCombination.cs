namespace AutoClicker.Models.System
{
    public class ArrowCombination
    {
        public int Key1 { get; set; }
        public int Key2 { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }

        public ArrowCombination(int key1, string name1, int key2, string name2)
        {
            Key1 = key1;
            Key2 = key2;
            Name1 = name1;
            Name2 = name2;
        }

        public override string ToString()
        {
            return $"{Name1} + {Name2}";
        }
    }
}
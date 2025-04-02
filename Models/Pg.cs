namespace UltimaOnlineMacro.Models
{
    public class Pg
    {
        public Pickaxe? PickaxeInBackpack { get; set; }
        public Pickaxe? PickaxeInHand { get; set; }
        public int Stone { get; set; }
        public int Stamina { get; set; }

        public Pg()
        {
            
        }

        public Pg(Pickaxe pickaxeInBackpack, Pickaxe pickaxeInHand)
        {
            PickaxeInBackpack = pickaxeInBackpack;
            PickaxeInHand = pickaxeInHand;  
        }
    }
}


using AutoClicker.Models.System;
using AutoClicker.Service;
using System.Drawing;

namespace AutoClicker.Models.TM
{
    public class Pg
    {
        public Pickaxe? PickaxeInBackpack { get; set; }
        public Pickaxe? PickaxeInHand { get; set; }
        public int Stone { get; set; }
        public int Stamina { get; set; }
        public bool RunWork { get; set; } = true;
        private AutoClickerLogger _logger = new AutoClickerLogger();
        public Pg()
        {

        }

        public Pg(Pickaxe pickaxeInBackpack, Pickaxe pickaxeInHand)
        {
            PickaxeInBackpack = pickaxeInBackpack;
            PickaxeInHand = pickaxeInHand;
        }

        public void WearPickaxe(Regions regions)
        {
            if (!PaperdollHavePickaxeInHand(regions))
            {
                var pickaxeBackpack = new Pickaxe(regions.BackpackRegion, SavedImageTemplate.ImageTemplatePickaxe);
                var sendInput = new SendInputService();
                sendInput.DragAndDrop(pickaxeBackpack.X, pickaxeBackpack.Y, regions.PaperdollRegion.X, regions.PaperdollRegion.Y);

                //controllo per vedere se adesso ha il piccone in mano
                if (!PaperdollHavePickaxeInHand(regions))
                    _logger.Loggin("Qualcosa è andato storto, il pg non ha il piccone in mano dopo WearPickaxe", true);
            }
        }

        public bool PaperdollHavePickaxeInHand(Regions regions)
        {
            bool success = true;
            try
            {
                var pickaxePaperdoll = new Pickaxe(regions.PaperdollRegion, SavedImageTemplate.ImageTemplatePickaxe);
                if (pickaxePaperdoll.X == 0 && pickaxePaperdoll.Y == 0) //Se il paperdoll non ha il piccone
                {
                    _logger.Loggin($"Il paperdoll non utilizza il piccone.");
                    if (!regions.BackpackRegion.IsEmpty)
                    {
                        _logger.Loggin($"Seleziona una regione per lo zaino.");
                    }
                    success = false;
                }

            }
            catch (Exception e)
            {
                _logger.Loggin($"Errore: {e.Message}", true);
            }
            return success;
        }

        public void Work(Regions regions)
        {
            while(RunWork)
            {
                WearPickaxe(regions);
            }
        }
    }
}

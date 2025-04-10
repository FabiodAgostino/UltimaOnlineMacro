
using AutoClicker.Models.System;
using AutoClicker.Service;
using AutoClicker.Utils;

namespace AutoClicker.Models.TM
{
    public class Pg
    {
        public Pickaxe? PickaxeInBackpack { get; set; }
        public Pickaxe? PickaxeInHand { get; set; }
        public List<Keys> Macro {  get; set; }
        public string PathJuornalLog { get; set; }
        public User32DLL.POINT FoodXY { get; set; }
        public User32DLL.POINT WaterXY { get; set; }
        private readonly SendInputService _sendInputService = new();
        public bool RunWork { get; set; } = true;
        private AutoClickerLogger _logger = new AutoClickerLogger();
        private ReadLogTMService _readLogTMService;
        private Regions _regions;
        public Pg()
        {
            _readLogTMService = new(this);
        }

        public Pg(Pickaxe pickaxeInBackpack, Pickaxe pickaxeInHand)
        {
            PickaxeInBackpack = pickaxeInBackpack;
            PickaxeInHand = pickaxeInHand;
            _readLogTMService = new(this);
        }

        public void WearPickaxe()
        {
            if (!PaperdollHavePickaxeInHand(_regions))
            {
                var pickaxeBackpack = new Pickaxe(_regions.BackpackRegion, SavedImageTemplate.ImageTemplatePickaxe);
                _sendInputService.DragAndDrop(pickaxeBackpack.X, pickaxeBackpack.Y, _regions.PaperdollPickaxeRegion.X, _regions.PaperdollPickaxeRegion.Y);

                //controllo per vedere se adesso ha il piccone in mano
                if (!PaperdollHavePickaxeInHand(_regions))
                    _logger.Loggin("Qualcosa è andato storto, il pg non ha il piccone in mano dopo WearPickaxe", true);
            }
        }

        public bool PaperdollHavePickaxeInHand(Regions regions)
        {
            bool havePickaxe = false;
            try
            {
                var pickaxePaperdoll = new Pickaxe(regions.PaperdollRegion, SavedImageTemplate.ImageTemplatePaperdollWithPickaxe);
                if (pickaxePaperdoll.X == 0 && pickaxePaperdoll.Y == 0) //Se il paperdoll non ha il piccone
                {
                    _logger.Loggin($"Il paperdoll non utilizza il piccone.");
                    if (!regions.BackpackRegion.IsEmpty)
                        _logger.Loggin($"Seleziona una regione per lo zaino.");

                    havePickaxe = false;
                }
                else
                    havePickaxe = true;

            }
            catch (Exception e)
            {
                _logger.Loggin($"Errore: {e.Message}", true);
            }
            return havePickaxe;
        }

        public void Work(Regions regions)
        {
            _regions = regions;
            WearPickaxe();
            while (RunWork)
            {
                var status = _readLogTMService.ReadRecentLogs(this.PathJuornalLog);
                Actions(status);
            }
        }

        public async Task SetWater() => WaterXY = (await _sendInputService.BeginCaptureAsync());
        public async Task SetFood() => FoodXY = (await _sendInputService.BeginCaptureAsync());

        public string IsReady()
        {
            if (String.IsNullOrEmpty(this.PathJuornalLog))
                return "Seleziona un path per il journal";

            return string.Empty;
        }

        public void StopBeep() => _readLogTMService.StopSound();


        public void Actions(Status status)
        {
            if (status.PickaxeBroke)
                WearPickaxe();

            if(status.Stone)
            {
                var muloRegion = _regions.GetMuloRegion();
                while (true)
                {
                    var iron = new Iron(_regions.BackpackRegion, SavedImageTemplate.ImageTemplateIron);
                    if (iron.X == 0 && iron.Y == 0)
                        break;
                    _sendInputService.DragAndDrop(iron.X, iron.Y, muloRegion.X, muloRegion.Y);
                }
            }

            if (status.Move)
                _sendInputService.MoveRandomly(8);

            if (status.Stamina)
                Thread.Sleep(50000);
            else
                Thread.Sleep(5200);

            _sendInputService.RunMacro(Macro);

        }




    }
}


using AutoClicker.Models.System;
using AutoClicker.Service;
using AutoClicker.Utils;
using AutoCliecker.Service;

namespace AutoClicker.Models.TM
{
    public class Pg
    {
        public Pickaxe? PickaxeInBackpack { get; set; }
        public Pickaxe? PickaxeInHand { get; set; }
        public string PathJuornalLog { get; set; }
        public Macro Macro;
        private readonly SendInputService _sendInputService = new();
        public bool RunWork { get; set; } = true;
        private AutoClickerLogger _logger = new AutoClickerLogger();
        private ReadLogTMService _readLogTMService;
        private Regions _regions;
        private TesserActService _tesserActService;
        public Status StatusForced { get; set; } = new();
        public MuloDetectorService DetectorService { get; set; }
        public Pg()
        {
            _readLogTMService = new(this);
            _tesserActService = new TesserActService();

            // Registra l'evento di aggiornamento
            _tesserActService.StatusUpdated += OnStatusUpdated;
        }

        public Pg(Pickaxe pickaxeInBackpack, Pickaxe pickaxeInHand)
        {
            PickaxeInBackpack = pickaxeInBackpack;
            PickaxeInHand = pickaxeInHand;
            _readLogTMService = new(this);
        }

        public async Task WearPickaxe()
        {
            if (!PaperdollHavePickaxeInHand(_regions))
            {
                var pickaxeBackpack = new Pickaxe(_regions.BackpackRegion, SavedImageTemplate.ImageTemplatePickaxe);
                await _sendInputService.DragAndDrop(pickaxeBackpack.X, pickaxeBackpack.Y, _regions.PaperdollPickaxeRegion.X, _regions.PaperdollPickaxeRegion.Y);

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

        public async Task Work(Regions regions, bool enableRunWork=true)
        {
            _tesserActService.StartMonitoring(_regions.BackpackRegion, 10000);

            _regions = regions;
            await WearPickaxe();
            RunWork = enableRunWork;
            while (RunWork)
            {
                var status = _readLogTMService.ReadRecentLogs(this.PathJuornalLog);
                await Actions(status);
            }
        }

        public void Stop()
        {
            _tesserActService.StatusUpdated -= OnStatusUpdated;
            _tesserActService.StopMonitoring();
        }

        public async Task SetWater() => _regions.WaterXY = (await _sendInputService.BeginCaptureAsync());
        public async Task SetFood() => _regions.FoodXY = (await _sendInputService.BeginCaptureAsync());

        public string IsReady()
        {
            if (String.IsNullOrEmpty(this.PathJuornalLog))
                return "Seleziona un path per il journal";

            return string.Empty;
        }

        public void StopBeep() => _readLogTMService.StopSound();


        public async Task Actions(Status status)
        {
            if (status.PickaxeBroke)
                await WearPickaxe();

            if (status.Stone || StatusForced.Stone)
            {
                StatusForced.Stone = false;
                var point = DetectorService.DetectMuloPrecise();
                while (true)
                {
                    var iron = new Iron(_regions.BackpackRegion, SavedImageTemplate.ImageTemplateIron);
                    if (iron.X == 0 && iron.Y == 0)
                        break;
                    await _sendInputService.DragAndDropIron(iron.X, iron.Y, point.X, point.Y);
                }
            }

            if (status.Move)
                await _sendInputService.MoveRandomly(8);

            if (status.Stamina || StatusForced.Stamina)
            {
                StatusForced.Stamina = false;
                await Task.Delay(50000);
            }
            else
                await Task.Delay((int)Macro.Delay);

            await _sendInputService.RunMacro(Macro.MacroKeys);
            Macro.UpdateRepetitionsMethod();
        }

        private void OnStatusUpdated(object sender, StatusBar status)
        {
            if (status.Stone.value + 50 >= status.Stone.max)
                StatusForced.Stone = true;
            else
                StatusForced.Stone = false;

            if(status.Stamina.value<10)
                StatusForced.Stamina = true;
            else
                StatusForced.Stamina = false;
        }
    }
}

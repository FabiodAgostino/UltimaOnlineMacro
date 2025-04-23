using AutoClicker.Models.System;
using AutoClicker.Service;
using LogManager;

namespace AutoClicker.Models.TM
{
    public class Pg
    {
        public string Name { get; set; }
        public Pickaxe? PickaxeInBackpack { get; set; }
        public Pickaxe? PickaxeInHand { get; set; }
        public string PathJuornalLog { get; set; }
        public string PathMacro { get; set; }

        public Macro Macro;
        private SendInputService _sendInputService {  get; set; }
        private ProcessService _processService { get; set; }
        public bool RunWork { get; set; } = true;
        private ReadLogTMService _readLogTMService;
        private Regions _regions;
        private TesserActService _tesserActService;
        private int BaseWeight { get; set; }
        public Status StatusForced { get; set; } = new();
        public List<Mulo> Muli { get; set; } = new();
        public Action<Dictionary<string, int>> RefreshRisorse { get => _readLogTMService.RefreshRisorse; set => _readLogTMService.RefreshRisorse = value; }
        public bool HaveBaseFuria { get; set; }
        public bool FuriaChecked { get; set; }

        private Action<bool> _run { get; set; }
        public Pg(ProcessService processService, Action<bool> run)
        {
            _run = run;
            _processService = processService;
            _sendInputService = new(processService);
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
                    Logger.Loggin("Qualcosa è andato storto, il pg non ha il piccone in mano dopo WearPickaxe", true);
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
                    Logger.Loggin($"Il paperdoll non utilizza il piccone.");
                    if (!regions.BackpackRegion.IsEmpty)
                        Logger.Loggin($"Seleziona una regione per lo zaino.");

                    havePickaxe = false;
                }
                else
                    havePickaxe = true;
            }
            catch (Exception e)
            {
                Logger.Loggin($"Errore: {e.Message}", true);
            }
            return havePickaxe;
        }

        public async Task Work(Regions regions, bool enableRunWork = true)
        {
            _regions = regions;
            BaseWeight = _tesserActService.GetStatusBar(_regions.StatusRegion).Stone.value;
            _tesserActService.StartMonitoring(_regions.StatusRegion, 10000);

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
            if(HaveBaseFuria && FuriaChecked)
                await _sendInputService.RunMacro(Macro.MacroFuria);

            if (status.Macrocheck)
            {
                _readLogTMService._playerBeep.Play();
                Logger.Loggin("Macrocheck ricevuto... procedo al riavvio (non toccare nulla fino al termine del beep)");
                _run.Invoke(false);
                await _processService.HandleRestartClient(this.PathMacro, 120000);
                await _sendInputService.Login();
                await Task.Delay(10000);
                _run.Invoke(true);
            }
            if (status.PickaxeBroke)
                await WearPickaxe();

            if (status.Move)
                await _sendInputService.MoveRandomly(8);


            if(status.Stamina || StatusForced.Stamina)
            {
                await Task.Delay(60000);
                StatusForced.Stamina = false;

            }
            else
                await Task.Delay((int)Macro.Delay);

            await _sendInputService.RunMacro(Macro.MacroKeys);
            Macro.UpdateRepetitionsMethod();
        }

        private void OnStatusUpdated(object sender, StatusBar status)
        {
            if (status.Stone != (0, 0) && status.Stamina != (0, 0))
            {
                if (status.Stone.value + 50 >= status.Stone.max)
                {
                    _readLogTMService._playerBeep.Play();
                    Logger.Loggin("Troppo peso");
                }
                if (status.Stamina.value < 10)
                    StatusForced.Stamina = true;
                else
                    StatusForced.Stamina = false;
            }
        }

        public void ChangeMuloOrStop()
        {
            if (Muli.Count > 1)
            {
                var muloNonSelezionato = Muli.FirstOrDefault(x => !x.Selected);
                var muloSelezionato = Muli.FirstOrDefault(x => x.Selected);
                muloNonSelezionato.Selected = true;
                muloSelezionato.Selected = false;
            }
        }
    }
}
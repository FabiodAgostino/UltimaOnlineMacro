using AutoClicker.Const;
using AutoClicker.Models.System;
using AutoClicker.Service;
using LogManager;
using MQTT;
using MQTT.Models;
using static MQTT.Models.MqttNotificationModel;

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
        private SendInputService _sendInputService { get; set; }
        private ProcessService _processService { get; set; }
        public bool RunWork { get; set; } = true;
        private ReadLogTMService _readLogTMService;
        private Regions _regions;
        private TesserActService _tesserActService;
        public int BaseWeight { get; set; }
        public Status StatusForced { get; set; } = new();
        public List<Mulo> Muli { get; set; } = new();
        public Action RefreshRisorse {  get => _readLogTMService.RefreshRisorse; set => _readLogTMService.RefreshRisorse = value; }
        public bool HaveBaseFuria { get; set; }
        public bool FuriaChecked { get; set; }
        private int _countForStop { get; set; } = 0;
        public bool HaveMacrocheck { get; set; }
        private Action<bool> _run { get; set; }
        private MqttNotificationService _mqqtService { get; set; }
        public Pg(ProcessService processService, Action<bool> run, MqttNotificationService mqttService)
        {
            _run = run;
            _processService = processService;
            _sendInputService = new(processService);
            _readLogTMService = new(this);
            _tesserActService = new TesserActService();
            _mqqtService = mqttService;
            // Registra l'evento di aggiornamento
            _tesserActService.StatusUpdated += OnStatusUpdated;
        }

        public Pg(Pickaxe pickaxeInBackpack, Pickaxe pickaxeInHand)
        {
            PickaxeInBackpack = pickaxeInBackpack;
            PickaxeInHand = pickaxeInHand;
            _readLogTMService = new(this);
        }

        private int _counterNotFoundPickAxe = 0;
        public async Task WearPickaxe()
        {
            if (!PaperdollHavePickaxeInHand(_regions))
            {
                var pickaxeBackpack = new Pickaxe(_regions.BackpackRegion, SavedImageTemplate.ImageTemplatePickaxe);
                if(pickaxeBackpack.X == 0 || pickaxeBackpack.Y == 0)
                {
                    SoundsPlayerService.OnePlay(SoundsFile.Beep);
                    Logger.Loggin("Non riesco a trovare il piccone nello zaino.", true, true);
                    await Task.Delay(500);
                    await _processService.FocusWindowReliably();
                    _counterNotFoundPickAxe++;
                    await WearPickaxe();
                    if(_counterNotFoundPickAxe > 3)
                    {
                        await _mqqtService.SendNotificationAsync(new MqttNotificationModel
                        {
                            Title = "Arresto",
                            Message = "Non trovo il piccone quindi mi fermo.",
                            Type = NotificationSeverity.Error,
                        });
                        Logger.Loggin("Non trovo il piccone quindi mi fermo.", true, true);
                        _run.Invoke(false);
                        _counterNotFoundPickAxe = 0;
                    }
                }
                else
                {
                    await _sendInputService.DragAndDrop(pickaxeBackpack.X, pickaxeBackpack.Y, _regions.PaperdollPickaxeRegion.X, _regions.PaperdollPickaxeRegion.Y);
                    //controllo per vedere se adesso ha il piccone in mano
                    if (!PaperdollHavePickaxeInHand(_regions))
                    {
                        Logger.Loggin("Qualcosa è andato storto, il pg non ha il piccone in mano dopo WearPickaxe", true);
                        await _mqqtService.SendNotificationAsync(new MqttNotificationModel
                        {
                            Title = "Arresto",
                            Message = "Tentativo di impugnare il piccone fallito.",
                            Type = NotificationSeverity.Error,
                        });
                        _run.Invoke(false);
                    }
                }
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

        public async Task Actions(Status status)
        {
            
            if (status.Macrocheck)
            {
                HaveMacrocheck = true;
                SoundsPlayerService.OnePlay(SoundsFile.Beep);
                Logger.Loggin("Macrocheck ricevuto... procedo al riavvio (non toccare nulla fino al termine del beep)");
                _run.Invoke(false);
                await _mqqtService.SendNotificationAsync(new MqttNotificationModel
                {
                    Title = "Macrocheck",
                    Message = "Sto gestendo il riavvio per macrocheck",
                    Type = NotificationSeverity.Info,
                });
                await _processService.HandleRestartClient(this.PathMacro, 120000);
                await _sendInputService.Login();
                await Task.Delay(10000);
                await _processService.SetTMWindow(true);
                _run.Invoke(true);
                HaveMacrocheck = false;
            }

            if (!String.IsNullOrEmpty(status.Error))
            {
                SoundsPlayerService.OnePlay(SoundsFile.Beep);
                Logger.Loggin(status.Error,true,true);
                _run.Invoke(false);
            }

            if(status.Stone)
            {
                Logger.Loggin("I tuoi muli da soma sono pieni, mi fermo");
                await _mqqtService.SendNotificationAsync(new MqttNotificationModel
                {
                    Title = "Muli pieni",
                    Message = "I muli sono pieni quindi mi fermo",
                    Type = NotificationSeverity.Warning,
                });
                _run.Invoke(false);
            }

            if (status.PickaxeBroke)
                await WearPickaxe();

            if (HaveBaseFuria && FuriaChecked)
                await _sendInputService.RunMacro(Macro.MacroFuria);

            if (status.Move)
                await _sendInputService.NavigateSmartly(4);


            if (status.Stamina || StatusForced.Stamina)
            {
                await Task.Delay(60000);
                StatusForced.Stamina = false;

            }
            else
                await Task.Delay((int)Macro.Delay);

            await _sendInputService.RunMacro(Macro.MacroKeys);
            Macro.UpdateRepetitionsMethod();
        }

        private int _notUpdated = 0;
        private async void OnStatusUpdated(object sender, StatusBar status)
        {
            if (status.Stone != (0, 0) && status.Stamina != (0, 0))
            {
                if (BaseWeight == 0)
                    BaseWeight = status.Stone.value;

                if(BaseWeight < status.Stone.value)
                {
                    BaseWeight = status.Stone.value;
                    var iron = new Iron(_regions.BackpackRegion, SavedImageTemplate.ImageTemplateIron);
                    if(iron.IsFound)
                    {
                        SoundsPlayerService.OnePlay(SoundsFile.Notify);
                        Logger.Loggin("Hai finito lo spazio nei tuoi muli!");
                        _countForStop++;
                    }
                    if(_countForStop > 5)
                    {
                        SoundsPlayerService.LoopPlay(SoundsFile.Beep);
                        _countForStop = 0;
                        _run.Invoke(false);
                        await _mqqtService.SendNotificationAsync(new MqttNotificationModel
                        {
                            Title = "Muli pieni",
                            Message = "I muli sono pieni quindi mi fermo",
                            Type = NotificationSeverity.Warning,
                        });
                    }
                }
                if (status.Stone.value + 50 >= status.Stone.max)
                {
                    SoundsPlayerService.OnePlay(SoundsFile.Beep);
                    Logger.Loggin($"Troppo peso: {status.Stone.value}");
                }
                if (status.Stamina.value < 10)
                {
                    StatusForced.Stamina = true;
                    Logger.Loggin($"Stamina bassa: {status.Stamina.value}");
                }
                else
                    StatusForced.Stamina = false;
            }
            else
            {
                Logger.Loggin("Non sto leggendo correttamente la barra status");
                _notUpdated++;
                if(_notUpdated > 5)
                {
                    var iron = new Iron(_regions.BackpackRegion, SavedImageTemplate.ImageTemplateIron);
                    if (iron.IsFound)
                    {
                        SoundsPlayerService.OnePlay(SoundsFile.Notify);
                        Logger.Loggin("Hai finito lo spazio nei tuoi muli!");
                        _notUpdated = 0;
                        _run.Invoke(false);
                        await _mqqtService.SendNotificationAsync(new MqttNotificationModel
                        {
                            Title = "Muli pieni",
                            Message = "I muli sono pieni quindi mi fermo",
                            Type = NotificationSeverity.Warning,
                        });
                    }
                }
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
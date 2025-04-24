using AutoClicker.Const;
using AutoClicker.Library;
using LogManager;
using static AutoClicker.Const.KeyboardMouseConst;
using static User32DLL;

namespace AutoClicker.Service
{
    public class SendInputService
    {
        private MouseInputSimulator MouseInputSimulator { get; set; }
        private KeyboardInputSimulator KeyboardInputSimulator {  get; set; }
        private ProcessService processService { get; set; }
        public SendInputService(ProcessService processService)
        {
            this.processService = processService;
            MouseInputSimulator = new MouseInputSimulator(processService);
            KeyboardInputSimulator = new KeyboardInputSimulator(processService);
        }

        /// Simula la pressione prolungata del tasto freccia destra per 3 secondi
        /// </summary>
        public async Task TestMouse()
        {
            await processService.FocusWindowReliably();
            await MouseInputSimulator.SimulateDoubleClick10Times(100, 400);
            await processService.RestoreOldWindow();

        }

        public async Task TestKeyboard()
        {
            await processService.FocusWindowReliably();

            for (int i = 0; i < 10; i++)
            {
                await KeyboardInputSimulator.Move(KeyboardMouseConst.VK_LEFT);
            }
            await processService.RestoreOldWindow();

        }

        public async Task<bool> DragAndDrop(int startX, int startY, int endX, int endY, int duration = 500, bool installHook = false)
        {
            await processService.FocusWindowReliably();
            var value = await MouseInputSimulator.DragAndDrop(startX, startY, endX, endY);
            await processService.RestoreOldWindow();
            return value;
        }

        public async Task<bool> DragAndDropIron(int startX, int startY, int endX, int endY)
        {
            await processService.FocusWindowReliably();
            await KeyboardInputSimulator.ClickUnclickShift();
            var success = await MouseInputSimulator.DragAndDrop(startX, startY, endX, endY, 150);
            await KeyboardInputSimulator.ClickUnclickShift(false);
            await processService.RestoreOldWindow();
            return success;
        }

        public Task<POINT> BeginCaptureAsync()
        {
            return MouseInputSimulator.BeginCaptureAsync();
        }

        public async Task Login()
        {
            await KeyboardInputSimulator.LogIn();
        }

        public async Task MoveRandomly(int numberOfSteps)
        {
            try
            {
                await processService.FocusWindowReliably();
                Random random = new Random();

                // Scegli casualmente una sola direzione all'inizio
                byte selectedDirection = ArrowKeys[random.Next(ArrowKeys.Length)];


                for (int i = 0; i < numberOfSteps; i++)
                {
                    await KeyboardInputSimulator.Move(selectedDirection);
                }

                await processService.RestoreOldWindow();
            }
            catch (Exception e)
            {
                Logger.Loggin($"Errore durante la simulazione dei movimenti direzionali: {e.Message}", true,false);
            }
        }

        public async Task RunMacro(List<Keys> keys)
        {
            await KeyboardInputSimulator.SimulateMacroWithModifiers(keys);
        }
    }
}
using AutoClicker.Const;
using AutoClicker.Library;
using LogManager;
using static AutoClicker.Const.KeyboardMouseConst;
using static AutoClicker.Utils.User32DLL;

namespace AutoClicker.Service
{
    public class SendInputService
    {
        private MouseInputSimulator MouseInputSimulator = new MouseInputSimulator();
        private KeyboardInputSimulator KeyboardInputSimulator = new KeyboardInputSimulator();

        public SendInputService()
        {
        }

        /// Simula la pressione prolungata del tasto freccia destra per 3 secondi
        /// </summary>
        public async Task TestMouse()
        {
            await MouseInputSimulator.SimulateDoubleClick10Times(100, 400);
        }

        public async Task TestKeyboard()
        {
            for (int i = 0; i < 10; i++)
            {
                await KeyboardInputSimulator.Move(KeyboardMouseConst.VK_LEFT);
            }
        }

        public async Task<bool> DragAndDrop(int startX, int startY, int endX, int endY, int duration = 500, bool installHook = false)
        {
            return await MouseInputSimulator.DragAndDrop(startX, startY, endX, endY);
        }

        public async Task<bool> DragAndDropIron(int startX, int startY, int endX, int endY)
        {
            await KeyboardInputSimulator.ClickUnclickShift();
            var success = await MouseInputSimulator.DragAndDrop(startX, startY, endX, endY, 150);
            await KeyboardInputSimulator.ClickUnclickShift(false);
            return success;
        }

        public Task<POINT> BeginCaptureAsync()
        {
            return MouseInputSimulator.BeginCaptureAsync();
        }

        public async Task MoveRandomly(int numberOfSteps)
        {
            try
            {
                Random random = new Random();
                Logger.Loggin($"Iniziando sequenza di {numberOfSteps} movimenti randomici");

                for (int i = 0; i < numberOfSteps / 2; i++)
                {
                    byte direction = ArrowKeys[random.Next(ArrowKeys.Length)];
                    await KeyboardInputSimulator.Move(direction);
                    await KeyboardInputSimulator.Move(direction);
                    await KeyboardInputSimulator.Move(direction);
                }
            }
            catch (Exception e)
            {
                Logger.Loggin($"Errore durante la simulazione dei movimenti randomici: {e.Message}", true);
            }
        }

        public async Task RunMacro(List<Keys> keys)
        {
            await KeyboardInputSimulator.SimulateMacroWithModifiers(keys);
        }
    }
}
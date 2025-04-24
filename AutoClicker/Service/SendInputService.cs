using AutoClicker.Const;
using AutoClicker.Library;
using LogManager;
using static AutoClicker.Const.KeyboardMouseConst;
using static User32DLL;
using AutoClicker.Service.ExtensionMethod;
using Image = AutoClicker.Service.ExtensionMethod.Image;
using AutoClicker.Models.System;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
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
        //TO DO FINIRE QUESTA PARTE
        public byte? GetDirectionFromBlackArea()
        {
            var screen = Image.CaptureCenterScreenshot(300,300);

            var gray = screen.bitmap.ToImage<Gray, byte>();
            var threshold = gray.ThresholdBinaryInv(new Gray(30), new Gray(255)); // tutto sotto 30 diventa bianco (quindi nero originale)

            int midX = threshold.Width / 2;
            int midY = threshold.Height / 2;

            // 4 quadranti
            var top = threshold.GetSubRect(new Rectangle(0, 0, threshold.Width, midY));
            var bottom = threshold.GetSubRect(new Rectangle(0, midY, threshold.Width, midY));
            var left = threshold.GetSubRect(new Rectangle(0, 0, midX, threshold.Height));
            var right = threshold.GetSubRect(new Rectangle(midX, 0, midX, threshold.Height));

            int topBlack = CvInvoke.CountNonZero(top);
            int bottomBlack = CvInvoke.CountNonZero(bottom);
            int leftBlack = CvInvoke.CountNonZero(left);
            int rightBlack = CvInvoke.CountNonZero(right);

            var blackLevels = new Dictionary<byte, int>
    {
        { VK_DOWN, topBlack },    // se il nero è sopra, l'oggetto è a nord → direzione verso sud
        { VK_UP, bottomBlack },   // viceversa
        { VK_RIGHT, leftBlack },
        { VK_LEFT, rightBlack }
    };

            // Trova la direzione con più nero
            var direction = blackLevels.OrderByDescending(kv => kv.Value).First();

            if (direction.Value < 100) // soglia minima per "nero rilevante"
            {
                Logger.Loggin("Nero non rilevato in modo significativo.");
                return null;
            }

            Logger.Loggin($"Direzione opposta trovata: {direction.Key}");
            return direction.Key;
        }



        public async Task MoveRandomly(int numberOfSteps)
        {
            try
            {
                await processService.FocusWindowReliably();
                Random random = new Random();
                var value = GetDirectionFromBlackArea();


                // Scegli casualmente una sola direzione all'inizio
                byte selectedDirection = value.HasValue ? value.Value : ArrowKeys[random.Next(ArrowKeys.Length)];


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
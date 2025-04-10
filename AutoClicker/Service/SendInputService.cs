using AutoClicker.Const;
using AutoClicker.Library;
using AutoClicker.Models.System;
using AutoClicker.Utils;
using System.Net;
using System.Runtime.InteropServices;
using static AutoClicker.Const.KeyboardMouseConst;
using static AutoClicker.Utils.User32DLL;


namespace AutoClicker.Service
{
    public class SendInputService
    {
        AutoClickerLogger Logger = new();
        MouseInputSimulator MouseInputSimulator = new MouseInputSimulator();
        KeyboardInputSimulator KeyboardInputSimulator = new KeyboardInputSimulator();

        public SendInputService()
        {
        }

        /// Simula la pressione prolungata del tasto freccia destra per 3 secondi
        /// </summary>
        public void TestMouse()
        {
            MouseInputSimulator.SimulateDoubleClick10Times(100,400);
        }

        public void TestKeyboard()
        {
            for (int i = 0; i < 10; i++)
            {
                KeyboardInputSimulator.Move(KeyboardMouseConst.VK_LEFT);
            }
        }


        /// <summary>
        /// Esegue un'operazione di click, trascinamento e rilascio tra due punti
        /// </summary>
        /// <param name="startX">Coordinata X iniziale</param>
        /// <param name="startY">Coordinata Y iniziale</param>
        /// <param name="endX">Coordinata X finale</param>
        /// <param name="endY">Coordinata Y finale</param>
        /// <param name="duration">Durata del trascinamento in millisecondi</param>
        /// <param name="installHook">Se true, installa un hook di tastiera durante l'operazione</param>
        public bool DragAndDrop(int startX, int startY, int endX, int endY, int duration = 500, bool installHook = false)
        {
            return MouseInputSimulator.DragAndDrop(startX, startY, endX, endY);
        }

      

        public Task<POINT> BeginCaptureAsync()
        {
            return MouseInputSimulator.BeginCaptureAsync();
        }

        public void MoveRandomly(int numberOfSteps)
        {
            try
            {
                Random random = new Random();
                Logger.Loggin($"Iniziando sequenza di {numberOfSteps} movimenti randomici");

                for (int i = 0; i < numberOfSteps/2; i++)
                {
                    byte direction = ArrowKeys[random.Next(ArrowKeys.Length)];
                    KeyboardInputSimulator.Move(direction);
                    KeyboardInputSimulator.Move(direction);
                    KeyboardInputSimulator.Move(direction);

                }
            }
            catch (Exception e)
            {
                Logger.Loggin($"Errore durante la simulazione dei movimenti randomici: {e.Message}", true);
            }
        }

        public void RunMacro(List<Keys> keys)
        {
            KeyboardInputSimulator.SimulateMacroWithModifiers(keys);
        }

    }
}
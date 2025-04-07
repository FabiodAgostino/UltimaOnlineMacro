using System.Runtime.InteropServices;

namespace AutoClicker.Service
{
    public class PostMessageService
    {
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        ProcessService ProcessService = new ProcessService();
        const uint WM_KEYDOWN = 0x0100;
        const uint WM_KEYUP = 0x0101;
        const int VK_RIGHT = 0x27; // Codice tasto freccia destra

        public void SimulateRightArrowFor3Seconds()
        {
            Console.WriteLine("Inizio simulazione tasto destro con PostMessage...");

            // Tempo di inizio
            DateTime endTime = DateTime.Now.AddSeconds(3);

            // Continua a inviare il tasto destro per 3 secondi
            while (DateTime.Now < endTime)
            {
                // Invia pressione tasto
                PostMessage(ProcessService.TheMiracleWindow.Hwnd, WM_KEYDOWN, (IntPtr)VK_RIGHT, IntPtr.Zero);

                // Piccola pausa tra i messaggi per non sovraccaricarlo
                Thread.Sleep(10);

                // Non inviamo il keyup per mantenere il tasto premuto
                // Per una simulazione più realistica potresti voler inviare
                // anche WM_KEYUP alla fine del ciclo
            }

            // Rilascia il tasto alla fine
            PostMessage(ProcessService.TheMiracleWindow.Hwnd, WM_KEYUP, (IntPtr)VK_RIGHT, IntPtr.Zero);

            Console.WriteLine("Simulazione completata.");
        }
    }
}

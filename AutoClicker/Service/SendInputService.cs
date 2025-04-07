using System.Runtime.InteropServices;

namespace AutoClicker.Service
{
    public class SendInputService
    {
        // Strutture necessarie per SendInput
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public int type;
            public INPUTUNION union;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct INPUTUNION
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // Costanti
        const int INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_KEYDOWN = 0x0000;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const ushort VK_RIGHT = 0x27; // Codice tasto freccia destra

        // Import delle funzioni di sistema
        [DllImport("user32.dll")]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        delegate IntPtr KeyboardHookProc(int code, IntPtr wParam, IntPtr lParam);

        const int WH_KEYBOARD_LL = 13;
        private IntPtr hookID = IntPtr.Zero;
        private KeyboardHookProc hookProc;

        public void SimulateRightArrowFor3Seconds()
        {
            Console.WriteLine("Installazione hook di tastiera...");

            // Installa l'hook per monitorare gli eventi della tastiera
            hookProc = KeyboardHookCallback;
            hookID = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, IntPtr.Zero, 0);

            Console.WriteLine("Inizio simulazione tasto destro con SendInput...");

            // Prepara l'input per premere il tasto freccia destra
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].union.ki.wVk = VK_RIGHT;
            inputs[0].union.ki.wScan = 0;
            inputs[0].union.ki.dwFlags = KEYEVENTF_KEYDOWN;
            inputs[0].union.ki.time = 0;
            inputs[0].union.ki.dwExtraInfo = IntPtr.Zero;

            // Tieni premuto il tasto per 3 secondi
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));

            // Aspetta 3 secondi
            Thread.Sleep(3000);

            // Rilascia il tasto
            inputs[0].union.ki.dwFlags = KEYEVENTF_KEYUP;
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));

            Console.WriteLine("Simulazione completata.");

            // Rimuovi l'hook
            UnhookWindowsHookEx(hookID);
            Console.WriteLine("Hook rimosso.");
        }

        private IntPtr KeyboardHookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            // Qui puoi intercettare e monitorare gli eventi di tastiera
            // Per esempio, potresti salvare in log quando il gioco risponde a un tasto

            // Passa l'evento al prossimo hook
            return CallNextHookEx(hookID, code, wParam, lParam);
        }
    }
}

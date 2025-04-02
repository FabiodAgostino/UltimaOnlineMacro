using System.Runtime.InteropServices;

namespace UltimaOnlineMacro
{
    public class UtilityClass
    {
        #region WinAPI
        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public static void DoubleClick(int x, int y)
        {
            // Prima clicca
            mouse_event(0x02, x, y, 0, 0); // Mouse down (sinistro)
            mouse_event(0x04, x, y, 0, 0); // Mouse up (sinistro)

            // Poi un altro clic per il doppio clic
            mouse_event(0x02, x, y, 0, 0); // Mouse down (sinistro)
            mouse_event(0x04, x, y, 0, 0); // Mouse up (sinistro)
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, uint dwExtraInfo);

        // Costanti per il mouse_event
        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;

        public const int WH_MOUSE_LL = 14; // Hook per intercettare eventi del mouse a basso livello
        public const int WM_LBUTTONDOWN = 0x0201; // Evento per il clic sinistro del mouse
        public const int WM_LBUTTONUP = 0x0202; // Evento per il rilascio del tasto sinistro del mouse


        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int x, int y);
        #endregion
    }
}

using AutoClicker.Models.System;
using System.Runtime.InteropServices;
using System.Text;

namespace AutoClicker.Service
{
    public class ProcessService
    {
        public TMWindow TheMiracleWindow { get; set; }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowExA(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextA(IntPtr hwnd, StringBuilder lpString, int nMaxCount);

        public ProcessService()
        {
            SetTMWindow();
        }

        public void SetTMWindow()
        {
            IntPtr hwnd = IntPtr.Zero;
            StringBuilder windowText = new StringBuilder(256);

            while ((hwnd = FindWindowExA(IntPtr.Zero, hwnd, null, null)) != IntPtr.Zero)
            {
                GetWindowTextA(hwnd, windowText, windowText.Capacity);
                if (windowText.ToString().Contains("ClassicUO") && windowText.ToString().Contains("TM Client"))
                {
                    TheMiracleWindow = new TMWindow() { Title = windowText.ToString(), Hwnd = hwnd };
                    break;
                }
            }

            throw new Exception("Nessuna finestra TM trovata");
        }
    }
}

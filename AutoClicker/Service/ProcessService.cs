using AutoClicker.Models.System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static AutoClicker.Const.KeyboardMouseConst;
using static AutoClicker.Utils.User32DLL;
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

        public async Task SetTMWindow()
        {
            if(TheMiracleWindow == null)
            {
                IntPtr hwnd = IntPtr.Zero;
                StringBuilder windowText = new StringBuilder(256);
                GetModuleHandleProcess();

                while ((hwnd = FindWindowExA(IntPtr.Zero, hwnd, null, null)) != IntPtr.Zero)
                {
                    GetWindowTextA(hwnd, windowText, windowText.Capacity);
                    if (windowText.ToString().Contains("ClassicUO") && windowText.ToString().Contains("TM Client"))
                    {
                        TheMiracleWindow.Title = windowText.ToString();
                        TheMiracleWindow.Hwnd = hwnd;
                        break;
                    }
                }
                if (TheMiracleWindow != null)
                    await FocusWindowReliably(TheMiracleWindow.Hwnd);
            }
        }

        /// <summary>
        /// Porta una finestra in primo piano in modo affidabile usando tecniche
        /// riscontrate in MacroUO
        /// </summary>
        private async Task FocusWindowReliably(IntPtr hWnd)
        {
            // Ottiene l'ID del thread corrente
            uint currentThreadId = GetCurrentThreadId();

            // Ottiene l'ID del thread della finestra di destinazione
            uint targetThreadId = GetWindowThreadProcessId(hWnd, out _);

            bool attachSuccess = false;

            try
            {
                // Collega i thread di input se sono diversi
                if (currentThreadId != targetThreadId)
                {
                    attachSuccess = AttachThreadInput(currentThreadId, targetThreadId, true);
                }

                // MacroUO utilizza varie tecniche per assicurarsi che la finestra
                // riceva il focus, incluse queste chiamate multiple e il ripristino
                // della finestra se è minimizzata
                SetForegroundWindow(hWnd);

                // Breve pausa per consentire alla finestra di ottenere il focus
                await Task.Delay(100);

                // Secondo tentativo per assicurarsi che la finestra abbia il focus
                SetForegroundWindow(hWnd);
            }
            finally
            {
                // Scollega i thread se erano stati collegati
                if (attachSuccess)
                {
                    AttachThreadInput(currentThreadId, targetThreadId, false);
                }
            }
        }

        public void GetModuleHandleProcess()
        {
            var process = Process.GetCurrentProcess();
            if (process != null && process.MainModule != null)
            {
                var moduleName = process.MainModule.ModuleName;
                if (TheMiracleWindow == null)
                    TheMiracleWindow = new();

                TheMiracleWindow.ModuleHandle = GetModuleHandle(moduleName);
            }
            else
                throw new Exception("Process non valido");
        }
    }
}

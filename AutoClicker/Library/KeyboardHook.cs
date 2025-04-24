using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static User32DLL;
using static AutoClicker.Const.KeyboardMouseConst;
using LogManager;

namespace AutoClicker.Library
{
    public class KeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private readonly LowLevelKeyboardProc _proc;
        private GCHandle _procHandle;
        private IntPtr _hookId = IntPtr.Zero;

        public KeyboardHook()
        {
            // Prepara il delegate da pin­nare
            _proc = HookCallback;
        }

        public void Install()
        {
            // Pin del delegate in memoria
            _procHandle = GCHandle.Alloc(_proc);
            // Ottieni handle del modulo corrente
            var moduleHandle = GetModuleHandle(Process.GetCurrentProcess().MainModule?.ModuleName);
            // Imposta l’hook
            _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, moduleHandle, 0);
            if (_hookId == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public void Uninstall()
        {
            if (_hookId != IntPtr.Zero)
            {
                // Rimuovi l’hook e libera il GCHandle
                UnhookWindowsHookEx(_hookId);
                if (_procHandle.IsAllocated)
                    _procHandle.Free();
                _hookId = IntPtr.Zero;
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var keyInfo = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

                if ((int)wParam == WM_KEYDOWN || (int)wParam == WM_SYSKEYDOWN)
                {
                    if (keyInfo.vkCode == VK_RIGHT)
                    {
                        Logger.Loggin($"HOOK: Tasto freccia destra rilevato (VK:{keyInfo.vkCode}, SC:{keyInfo.scanCode}, FL:{keyInfo.flags})", false, false);
                    }
                }
            }
            // Passa sempre l’evento al prossimo hook
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Dispose() => Uninstall();
    }

}

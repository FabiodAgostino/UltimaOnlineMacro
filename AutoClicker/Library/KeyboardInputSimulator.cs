using AutoClicker.Service;
using LogManager;
using System.Runtime.InteropServices;
using static AutoClicker.Const.KeyboardMouseConst;
using static User32DLL;

namespace AutoClicker.Library
{
    public class KeyboardInputSimulator
    {
        private ProcessService processService;
        private nint hookID = nint.Zero;
        private static readonly List<LowLevelKeyboardProc> _activeHooks = new List<LowLevelKeyboardProc>();
        private LowLevelKeyboardProc _currentHookProc; // Mantieni un riferimento al delegate corrente
        private KeyboardHook _keyboardHook;

        public KeyboardInputSimulator(ProcessService process)
        {
            this.processService = process;
        }

        public async Task Move(byte tasto)
        {
            Logger.Loggin("Inizializzazione simulazione freccia destra con hook a basso livello...", false,false);
            var tm = processService.TheMiracleWindow;
            var hWnd = tm.Hwnd;
            try
            {
                InstallKeyboardHook();
                Logger.Loggin("Hook a basso livello inizializzato correttamente",false,false);
                Random random = new Random();
                keybd_event(tasto, 0, KEYEVENTF_EXTENDEDKEY, nint.Zero);
                await Task.Delay(random.Next(50, 150));
                keybd_event(tasto, 0, KEYEVENTF_KEYUP | KEYEVENTF_EXTENDEDKEY, nint.Zero);
                await Task.Delay(random.Next(50, 150));
                Logger.Loggin("Simulazione completata con successo - 30 pressioni della freccia destra eseguite",false,false);
            }
            catch (Exception e)
            {
                Logger.Loggin($"Errore durante la simulazione: {e.Message}", true,false);
            }
            finally
            {
                _keyboardHook.Uninstall();
            }
        }

       

        private void InstallKeyboardHook()
        {
            // Salva il delegate in un campo membro
            _keyboardHook = new KeyboardHook();
            _keyboardHook.Install();
        }

        //private nint HookCallback(int nCode, nint wParam, nint lParam)
        //{
        //    if (nCode >= 0)
        //    {
        //        var keyInfo = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

        //        if ((int)wParam == WM_KEYDOWN || (int)wParam == WM_SYSKEYDOWN)
        //        {
        //            if (keyInfo.vkCode == VK_RIGHT)
        //            {
        //                Logger.Loggin($"HOOK: Tasto freccia destra rilevato (VK:{keyInfo.vkCode}, SC:{keyInfo.scanCode}, FL:{keyInfo.flags})", false, false);
        //            }
        //        }
        //    }

        //    // Passa sempre l'evento al prossimo hook
        //    return CallNextHookEx(hookID, nCode, wParam, lParam);
        //}

        public async Task SimulateButtonPressMacro(int tasto, int times = 1)
        {
            IntPtr tastoPtr = (IntPtr)tasto;
            var processService = new ProcessService();
            var hWnd = processService.TheMiracleWindow.Hwnd;

            try
            {
                Random random = new Random();

                for (int i = 0; i < times; i++)
                {
                    uint scanCode = MapVirtualKey((uint)tasto, 0);

                    IntPtr lParamDown = (IntPtr)((scanCode << 16) | 0x00000001);
                    IntPtr lParamUp = (IntPtr)((scanCode << 16) | 0xC0000001);

                    PostMessage(hWnd, WM_KEYDOWN, tastoPtr, lParamDown);
                    await Task.Delay(random.Next(40, 100));
                    PostMessage(hWnd, WM_KEYUP, tastoPtr, lParamUp);
                    if (i < times - 1)
                        await Task.Delay(random.Next(100, 300));

                }
            }
            catch (Exception ex)
            {
            }
        }

        public async Task ClickUnclickShift(bool click = true)
        {
            var hWnd = processService.TheMiracleWindow.Hwnd;

            uint scan = MapVirtualKey((uint)Keys.ShiftKey, 0);

            if (click)
            {
                // Premi il tasto Shift
                IntPtr lParamDown = (IntPtr)((scan << 16) | 0x00000001);
                PostMessage(hWnd, WM_KEYDOWN, (IntPtr)Keys.ShiftKey, lParamDown);
            }
            else
            {
                IntPtr lParamUp = (IntPtr)((scan << 16) | 0xC0000001);
                PostMessage(hWnd, WM_KEYUP, (IntPtr)Keys.ShiftKey, lParamUp);
            }
        }

        public async Task SimulateMacroWithModifiers(List<Keys> keys, int times = 1)
        {
            var hWnd = processService.TheMiracleWindow.Hwnd;
            var modifiers = keys.Where(k => ModifierKeys.Contains(k)).ToList();
            var actionKey = keys.FirstOrDefault(k => !ModifierKeys.Contains(k));

            if (actionKey == Keys.None)
                return;

            Random random = new Random();

            for (int i = 0; i < times; i++)
            {
                // Premi i modificatori prima
                foreach (var mod in modifiers)
                {
                    uint scan = MapVirtualKey((uint)mod, 0);
                    IntPtr lParamDown = (IntPtr)((scan << 16) | 0x00000001);
                    PostMessage(hWnd, WM_KEYDOWN, (IntPtr)mod, lParamDown);
                }

                // Premi il tasto azione
                uint scanCode = MapVirtualKey((uint)actionKey, 0);
                IntPtr lParamDownAction = (IntPtr)((scanCode << 16) | 0x00000001);
                IntPtr lParamUpAction = (IntPtr)((scanCode << 16) | 0xC0000001);

                PostMessage(hWnd, WM_KEYDOWN, (IntPtr)actionKey, lParamDownAction);
                await Task.Delay(random.Next(40, 100));
                PostMessage(hWnd, WM_KEYUP, (IntPtr)actionKey, lParamUpAction);

                // Rilascia i modificatori dopo
                foreach (var mod in modifiers)
                {
                    uint scan = MapVirtualKey((uint)mod, 0);
                    IntPtr lParamUp = (IntPtr)((scan << 16) | 0xC0000001);
                    PostMessage(hWnd, WM_KEYUP, (IntPtr)mod, lParamUp);
                }

                if (i < times - 1)
                    await Task.Delay(random.Next(100, 300));
            }
        }


        public async Task LogIn()
        {
            await Task.Delay(2000);

            var result = processService.GetClientPtr();
            if(result.HasValue)
            {
                var hWnd = result.Value;
                Random random = new Random();
                int times = 3;
                for (int i = 0; i < times; i++)
                {
                    await Task.Delay(2000);
                    // Premi il tasto azione
                    uint scanCode = MapVirtualKey((uint)Keys.Enter, 0);
                    IntPtr lParamDownAction = (IntPtr)((scanCode << 16) | 0x00000001);
                    IntPtr lParamUpAction = (IntPtr)((scanCode << 16) | 0xC0000001);

                    PostMessage(hWnd, WM_KEYDOWN, (IntPtr)Keys.Enter, lParamDownAction);
                    await Task.Delay(random.Next(40, 100));
                    PostMessage(hWnd, WM_KEYUP, (IntPtr)Keys.Enter, lParamUpAction);
                }
            }
            
        }
    }
}
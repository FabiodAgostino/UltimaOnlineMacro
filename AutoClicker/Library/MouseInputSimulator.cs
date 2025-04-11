using AutoClicker.Service;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static AutoClicker.Const.KeyboardMouseConst;
using static AutoClicker.Utils.User32DLL;

namespace AutoClicker.Library
{

    public class MouseInputSimulator
    {
        AutoClickerLogger Logger = new();

        private ProcessService processService;
        private nint hookID = nint.Zero;

        public MouseInputSimulator()
        {
            processService = new ProcessService();
        }

        public async Task MoveMouse(int x, int y)
        {
            await Task.Delay(1000);
            SetCursorPos(x, y);
        }

        public async Task SimulateDoubleClick10Times(int x, int y)
        {
            Logger.Loggin($"Inizializzazione simulazione doppio clic alle coordinate ({x}, {y})...");
            var tm = processService.TheMiracleWindow;
            var hWnd = tm.Hwnd;

            try
            {
                SetForegroundWindow(hWnd);

                InstallMouseHook();

                Logger.Loggin("Hook mouse a basso livello inizializzato correttamente");

                SetCursorPos(x, y);
                Logger.Loggin($"Cursore posizionato alle coordinate ({x}, {y})");

                Random random = new Random();

                for (int i = 0; i < 10; i++)
                {
                    Logger.Loggin($"Simulazione doppio clic #{i + 1}");

                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, nint.Zero);
                    await Task.Delay(random.Next(10, 30)); // Breve attesa tra pressione e rilascio

                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, nint.Zero);
                    await Task.Delay(random.Next(10, 40)); // Attesa tra il primo e il secondo clic del doppio clic

                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, nint.Zero);
                    await Task.Delay(random.Next(10, 30)); // Breve attesa tra pressione e rilascio

                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, nint.Zero);
                    await Task.Delay(random.Next(100, 300));
                }

                Logger.Loggin("Simulazione completata con successo - 100 doppi clic eseguiti");
            }
            catch (Exception e)
            {
                Logger.Loggin($"Errore durante la simulazione: {e.Message}", true);
            }
            finally
            {
                // Rimuovi l'hook se installato
                if (hookID != nint.Zero)
                {
                    UnhookWindowsHookEx(hookID);
                    Logger.Loggin("Hook mouse a basso livello rimosso");
                }
            }
        }

        private void InstallMouseHook()
        {

            // Installa un hook che ci permetterà di studiare i messaggi del mouse nel sistema
            LowLevelMouseProc proc = HookCallback;
            hookID = SetWindowsHookEx(WH_MOUSE_LL, proc, processService.TheMiracleWindow.ModuleHandle, 0);

            if (hookID == nint.Zero)
            {
                Logger.Loggin("Impossibile installare l'hook mouse a basso livello", true);
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private nint HookCallback(int nCode, nint wParam, nint lParam)
        {
            // Questo è solo per monitoraggio, non blocchiamo o modifichiamo gli eventi
            if (nCode >= 0)
            {
                var mouseInfo = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

                // Logga solo gli eventi di clic per debug, non tutti gli eventi mouse
                if ((int)wParam == WM_LBUTTONDOWN)
                {
                    Logger.Loggin($"HOOK: Clic sinistro rilevato alle coordinate ({mouseInfo.pt.X}, {mouseInfo.pt.Y})");
                }
            }

            // Passa sempre l'evento al prossimo hook
            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        public async Task<bool> DragAndDrop(int startX, int startY, int endX, int endY, int duration = 500)
        {
            Logger.Loggin($"Esecuzione drag and drop a basso livello da ({startX},{startY}) a ({endX},{endY})");
            bool success = true;

            // Attiva la finestra del gioco
            var tm = processService.TheMiracleWindow;
            var hWnd = tm.Hwnd;
            SetForegroundWindow(hWnd);
            await Task.Delay(200); // Attendi che la finestra sia in primo piano


           
            try
            {
                // Installa anche un hook per il mouse per monitoraggio
                LowLevelMouseProc mouseProc = HookCallback;
                IntPtr mouseHookID = SetWindowsHookEx(WH_MOUSE_LL, mouseProc, processService.TheMiracleWindow.ModuleHandle, 0);

                // Posiziona il cursore al punto iniziale
                SetCursorPos(startX, startY);
                await Task.Delay(100);

                // Esegui click sinistro down con API a basso livello
                Logger.Loggin($"Pressione pulsante sinistro alle coordinate ({startX},{startY})");
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
                await Task.Delay(100);

                // Calcola intervalli per il movimento
                int steps = Math.Max(1, duration / 10); // Un movimento ogni 10ms
                for (int i = 0; i <= steps; i++)
                {
                    // Calcola posizione intermedia
                    int curX = startX + ((endX - startX) * i / steps);
                    int curY = startY + ((endY - startY) * i / steps);

                    // Sposta il cursore alla posizione intermedia
                    SetCursorPos(curX, curY);

                    // Piccola attesa tra i movimenti
                    await Task.Delay(10);
                }

                // Assicurati che il cursore sia esattamente alla posizione finale
                SetCursorPos(endX, endY);
                await Task.Delay(50);

                // Esegui rilascio click sinistro con API a basso livello
                Logger.Loggin($"Rilascio pulsante sinistro alle coordinate ({endX},{endY})");
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);

                Logger.Loggin("Operazione di drag and drop completata con successo.");

                // Rimuovi l'hook del mouse
                if (mouseHookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(mouseHookID);
                }
            }
            catch (Exception e)
            {
                Logger.Loggin($"Errore durante drag and drop: {e.Message}", true);
                success = false;
            }
            finally
            {
                // Rimuovi l'hook della tastiera se è stato installato
                if (hookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(hookID);
                    Logger.Loggin("Hook di tastiera rimosso.");
                }
            }

            return success;
        }

        public Task<POINT> BeginCaptureAsync()
        {
            var tcs = new TaskCompletionSource<POINT>();
            captureProc = (int nCode, IntPtr wParam, IntPtr lParam) =>
            {
                if (nCode >= 0 && (int)wParam == WM_LBUTTONDOWN)
                {
                    var mouseInfo = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    Logger.Loggin($"Coordinate catturate: X={mouseInfo.pt.X}, Y={mouseInfo.pt.Y}");

                    // Rimuovi l'hook
                    UnhookWindowsHookEx(hookID);
                    hookID = IntPtr.Zero;

                    // Completa il Task con il punto catturato
                    tcs.TrySetResult(mouseInfo.pt);
                    return (IntPtr)1;
                }

                return CallNextHookEx(hookID, nCode, wParam, lParam);
            };

            hookID = SetWindowsHookEx(WH_MOUSE_LL, captureProc, processService.TheMiracleWindow.ModuleHandle, 0);

            if (hookID == IntPtr.Zero)
            {
                Logger.Loggin("Impossibile installare l'hook mouse per cattura coordinate", true);
                tcs.TrySetException(new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()));
            }

            Logger.Loggin("Hook per cattura coordinate attivo. Clicca in un punto dello schermo.");

            return tcs.Task;
        }


    }
}

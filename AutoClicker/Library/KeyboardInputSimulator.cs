using AutoClicker.Service;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static AutoClicker.Const.KeyboardMouseConst;
using static AutoClicker.Utils.User32DLL;
namespace AutoClicker.Library
{
    public class KeyboardInputSimulator
    {
        AutoClickerLogger Logger = new();

        private ProcessService processService;
        private nint hookID = nint.Zero;


        public KeyboardInputSimulator()
        {
            processService = new ProcessService();
        }



        public void SimulateRightArrowFor30Times()
        {
            Logger.Loggin("Inizializzazione simulazione freccia destra con hook a basso livello...");
            var tm = processService.TheMiracleWindow;
            var hWnd = tm.Hwnd;

            try
            {
                // Attiva la finestra del gioco
                SetForegroundWindow(hWnd);
                Thread.Sleep(500); // Attendi che la finestra sia in primo piano

                // Installa un hook a basso livello per poter intercettare e studiare l'input di tastiera
                InstallKeyboardHook();

                Logger.Loggin("Hook a basso livello inizializzato correttamente");

                Random random = new Random();

                for (int i = 0; i < 30; i++)
                {
                    Logger.Loggin($"Simulazione pressione freccia destra #{i + 1}");

                    // Simula una pressione della freccia destra a basso livello
                    // La freccia destra è un tasto esteso, quindi usiamo il flag KEYEVENTF_EXTENDEDKEY
                    keybd_event(VK_RIGHT, 0, KEYEVENTF_EXTENDEDKEY, nint.Zero);

                    // Attendi un tempo casuale tra pressione e rilascio
                    Thread.Sleep(random.Next(50, 150));

                    // Simula un rilascio della freccia destra a basso livello
                    keybd_event(VK_RIGHT, 0, KEYEVENTF_KEYUP | KEYEVENTF_EXTENDEDKEY, nint.Zero);

                    // Piccola pausa tra le pressioni per simulare comportamento umano
                    Thread.Sleep(random.Next(50, 150));
                }

                Logger.Loggin("Simulazione completata con successo - 30 pressioni della freccia destra eseguite");
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
                    Logger.Loggin("Hook a basso livello rimosso");
                }
            }
        }

        private void InstallKeyboardHook()
        {
            // Ottieni il modulo corrente per l'hook
            var moduleHandle = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);

            // Installa un hook che ci permetterà di studiare i messaggi di tastiera nel sistema
            LowLevelKeyboardProc proc = HookCallback;
            hookID = SetWindowsHookEx(WH_KEYBOARD_LL, proc, moduleHandle, 0);

            if (hookID == nint.Zero)
            {
                Logger.Loggin("Impossibile installare l'hook a basso livello", true);
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private nint HookCallback(int nCode, nint wParam, nint lParam)
        {
            // Questo è solo per monitoraggio, non blocchiamo o modifichiamo gli eventi
            if (nCode >= 0)
            {
                var keyInfo = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

                // Logga gli eventi di tastiera per debug
                if ((int)wParam == WM_KEYDOWN || (int)wParam == WM_SYSKEYDOWN)
                {
                    // Non logghiamo tutto o rischieremmo di riempire i log
                    if (keyInfo.vkCode == VK_RIGHT)
                    {
                        Logger.Loggin($"HOOK: Tasto freccia destra rilevato (VK:{keyInfo.vkCode}, SC:{keyInfo.scanCode}, FL:{keyInfo.flags})");
                    }
                }
            }

            // Passa sempre l'evento al prossimo hook
            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        public void SimulateF10Press(int times = 1)
        {
            Logger.Loggin("Inizializzazione simulazione tasto F10...");
            var tm = processService.TheMiracleWindow;
            var hWnd = tm.Hwnd;

            try
            {
                // Attiva la finestra del gioco (come nel metodo originale)
                SetForegroundWindow(hWnd);
                Thread.Sleep(500);

                // Installa l'hook esattamente come fai nel metodo originale
                InstallKeyboardHook();

                Logger.Loggin("Hook a basso livello inizializzato correttamente");

                Random random = new Random();

                // Utilizza la stessa costante VK_F10 che è definita nel tuo contesto
                // Assumo che sia importata da KeyboardMouseConst
                for (int i = 0; i < times; i++)
                {
                    Logger.Loggin($"Simulazione pressione F10 #{i + 1}");

                    // Usa lo stesso pattern del metodo della freccia destra
                    // Ma con il codice per F10 (0x79 / 121)
                    keybd_event(VK_F10, 0, KEYEVENTF_EXTENDEDKEY, nint.Zero);  // Niente flag KEYEVENTF_EXTENDEDKEY qui perché F10 non è un tasto esteso

                    // Stesso timing random che funziona per le frecce
                    Thread.Sleep(random.Next(50, 150));

                    // Rilascio del tasto
                    keybd_event(VK_F10, 0, KEYEVENTF_KEYUP, nint.Zero);

                    // Stessa pausa tra le pressioni
                    Thread.Sleep(random.Next(50, 150));
                }

                Logger.Loggin($"Simulazione completata con successo - {times} pressioni di F10 eseguite");
            }
            catch (Exception e)
            {
                Logger.Loggin($"Errore durante la simulazione: {e.Message}", true);
            }
            finally
            {
                // Rimuovi l'hook come nel metodo originale
                if (hookID != nint.Zero)
                {
                    UnhookWindowsHookEx(hookID);
                    Logger.Loggin("Hook a basso livello rimosso");
                }
            }
        }
    }
}

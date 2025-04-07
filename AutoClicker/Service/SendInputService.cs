using AutoClicker.Models.System;
using AutoClicker.Utils;
using System.Runtime.InteropServices;
using static AutoClicker.Const.KeyboardMouseConst;
using static AutoClicker.Utils.User32DLL;


namespace AutoClicker.Service
{
    public class SendInputService
    {
        AutoClickerLogger Logger = new();
        delegate IntPtr KeyboardHookProc(int code, IntPtr wParam, IntPtr lParam);

        private IntPtr hookID = IntPtr.Zero;
        private KeyboardHookProc hookProc;

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookProc callback, IntPtr hInstance, uint threadId);

        public SendInputService()
        {
            // Inizializza il callback per l'hook
            hookProc = KeyboardHookCallback;
        }

        /// <summary>
        /// Simula la pressione prolungata del tasto freccia destra per 3 secondi
        /// </summary>
        public void SimulateRightArrowFor3Seconds()
        {
            Logger.Loggin("Installazione hook di tastiera...");
            // Installa l'hook per monitorare gli eventi della tastiera
            hookID = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, IntPtr.Zero, 0);
            Logger.Loggin("Inizio simulazione tasto destro con SendInput...");

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

            Logger.Loggin("Simulazione completata.");

            // Rimuovi l'hook
            UnhookWindowsHookEx(hookID);
            Logger.Loggin("Hook rimosso.");
        }

        private IntPtr KeyboardHookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            // Qui puoi intercettare e monitorare gli eventi di tastiera
            // Per esempio, potresti salvare in log quando il gioco risponde a un tasto
            // Passa l'evento al prossimo hook
            return CallNextHookEx(hookID, code, wParam, lParam);
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
            Logger.Loggin($"Esecuzione drag and drop da ({startX},{startY}) a ({endX},{endY})");
            bool success = false;

            if (installHook)
            {
                Logger.Loggin("Installazione hook di tastiera...");
                hookID = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, IntPtr.Zero, 0);
            }

            try
            {
                // Posiziona il cursore al punto iniziale
                SetCursorPos(startX, startY);
                Thread.Sleep(100);

                // Prepara l'input per click sinistro
                INPUT[] mouseDown = new INPUT[1];
                mouseDown[0].type = INPUT_MOUSE;
                mouseDown[0].union.mi.dx = 0;
                mouseDown[0].union.mi.dy = 0;
                mouseDown[0].union.mi.mouseData = 0;
                mouseDown[0].union.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
                mouseDown[0].union.mi.time = 0;
                mouseDown[0].union.mi.dwExtraInfo = IntPtr.Zero;

                // Click sinistro down
                SendInput(1, mouseDown, Marshal.SizeOf(typeof(INPUT)));
                Thread.Sleep(100);

                // Calcola intervalli per il movimento
                int steps = duration / 10; // Un movimento ogni 10ms
                for (int i = 0; i <= steps; i++)
                {
                    // Calcola posizione intermedia
                    int curX = startX + ((endX - startX) * i / steps);
                    int curY = startY + ((endY - startY) * i / steps);

                    // Sposta il cursore
                    SetCursorPos(curX, curY);
                    Thread.Sleep(10);
                }

                // Prepara l'input per rilascio click sinistro
                INPUT[] mouseUp = new INPUT[1];
                mouseUp[0].type = INPUT_MOUSE;
                mouseUp[0].union.mi.dx = 0;
                mouseUp[0].union.mi.dy = 0;
                mouseUp[0].union.mi.mouseData = 0;
                mouseUp[0].union.mi.dwFlags = MOUSEEVENTF_LEFTUP;
                mouseUp[0].union.mi.time = 0;
                mouseUp[0].union.mi.dwExtraInfo = IntPtr.Zero;

                // Rilascio click sinistro
                SendInput(1, mouseUp, Marshal.SizeOf(typeof(INPUT)));

                Logger.Loggin("Operazione di drag and drop completata.");
            }
            catch(Exception e)
            {
                Logger.Loggin(e.Message, true);
                success = false;
            }
            finally
            {
                if (installHook && hookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(hookID);
                    Logger.Loggin("Hook rimosso.");
                }
            }
            return success;

        }

        /// <summary>
        /// Simula la pressione di una combinazione di tasti
        /// </summary>
        /// <param name="key1">Primo tasto da premere</param>
        /// <param name="key2">Secondo tasto da premere</param>
        /// <param name="pressDuration">Durata della pressione in millisecondi</param>
        public void SimulateKeyCombo(ushort key1, ushort key2, int pressDuration = 100)
        {
            // Crea array per contenere 4 eventi: due key down e due key up
            INPUT[] inputs = new INPUT[4];

            // Configurazione primo keydown
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].union.ki.wVk = key1;
            inputs[0].union.ki.wScan = 0;
            inputs[0].union.ki.dwFlags = KEYEVENTF_KEYDOWN;
            inputs[0].union.ki.time = 0;
            inputs[0].union.ki.dwExtraInfo = IntPtr.Zero;

            // Configurazione secondo keydown
            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].union.ki.wVk = key2;
            inputs[1].union.ki.wScan = 0;
            inputs[1].union.ki.dwFlags = KEYEVENTF_KEYDOWN;
            inputs[1].union.ki.time = 0;
            inputs[1].union.ki.dwExtraInfo = IntPtr.Zero;

            // Configurazione secondo keyup (invertiamo l'ordine per i keyup)
            inputs[2].type = INPUT_KEYBOARD;
            inputs[2].union.ki.wVk = key2;
            inputs[2].union.ki.wScan = 0;
            inputs[2].union.ki.dwFlags = KEYEVENTF_KEYUP;
            inputs[2].union.ki.time = 0;
            inputs[2].union.ki.dwExtraInfo = IntPtr.Zero;

            // Configurazione primo keyup
            inputs[3].type = INPUT_KEYBOARD;
            inputs[3].union.ki.wVk = key1;
            inputs[3].union.ki.wScan = 0;
            inputs[3].union.ki.dwFlags = KEYEVENTF_KEYUP;
            inputs[3].union.ki.time = 0;
            inputs[3].union.ki.dwExtraInfo = IntPtr.Zero;

            // Invia i primi due eventi (keydown)
            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));

            // Attendi per la durata specificata
            Thread.Sleep(pressDuration);

            // Invia gli ultimi due eventi (keyup)
            SendInput(2, new INPUT[] { inputs[2], inputs[3] }, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>
        /// Esegue una camminata randomizzata con le frecce, includendo combinazioni diagonali
        /// </summary>
        /// <param name="totalSteps">Numero totale di passi da eseguire</param>
        /// <param name="minKeyPressTime">Tempo minimo di pressione tasto in ms</param>
        /// <param name="maxKeyPressTime">Tempo massimo di pressione tasto in ms</param>
        /// <param name="minPauseBetweenSteps">Pausa minima tra due passi in ms</param>
        /// <param name="maxPauseBetweenSteps">Pausa massima tra due passi in ms</param>
        /// <param name="comboChance">Probabilità di usare una combinazione diagonale (0-100)</param>
        /// <param name="installHook">Se true, installa un hook di tastiera durante la camminata</param>
        public void RandomizedWalk(int totalSteps = 8,
                                  int minKeyPressTime = 100,
                                  int maxKeyPressTime = 300,
                                  int minPauseBetweenSteps = 200,
                                  int maxPauseBetweenSteps = 500,
                                  int comboChance = 30,
                                  bool installHook = false)
        {
            Logger.Loggin($"Inizio camminata randomizzata con {totalSteps} passi...");

            if (installHook)
            {
                Logger.Loggin("Installazione hook di tastiera...");
                hookID = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, IntPtr.Zero, 0);
            }

            try
            {
                Random random = new Random();

                // Contatori per ogni direzione e combinazione
                int[] singleDirectionCounts = new int[4] { 0, 0, 0, 0 };
                int[] combinationCounts = new int[ValidCombinations.Length];

                // Contatori per tipo di passo
                int singleKeySteps = 0;
                int comboSteps = 0;

                for (int step = 0; step < totalSteps; step++)
                {
                    // Determina se la pressione sarà semplice o combinata
                    bool useCombo = random.Next(100) < comboChance;

                    // Tempo di pressione casuale
                    int keyPressTime = random.Next(minKeyPressTime, maxKeyPressTime + 1);

                    if (useCombo && comboSteps < totalSteps / 2) // Limita le combinazioni a massimo metà dei passi totali
                    {
                        // Scegli una combinazione valida
                        int comboIndex = random.Next(ValidCombinations.Length);
                        ArrowCombination combo = ValidCombinations[comboIndex];

                        Logger.Loggin($"Passo {step + 1}/{totalSteps}: {combo} per {keyPressTime}ms");

                        // Simula combinazione di tasti
                        SimulateKeyCombo(((ushort)combo.Key1), ((ushort)combo.Key2), keyPressTime);

                        // Aggiorna il contatore di passi combinati
                        combinationCounts[comboIndex]++;
                        comboSteps++;
                    }
                    else
                    {
                        // Sceglie una direzione casuale
                        int directionIndex;

                        // Se siamo oltre la metà dei passi, assicuriamoci di coprire le direzioni mancanti
                        if (step >= totalSteps / 2)
                        {
                            // Trova la direzione meno utilizzata
                            int minDirectionCount = singleDirectionCounts.Min();

                            if (minDirectionCount < 2)
                            {
                                // Trova tutte le direzioni che hanno meno di 2 passi
                                var underutilizedDirections = Enumerable.Range(0, 4)
                                    .Where(i => singleDirectionCounts[i] < 2)
                                    .ToList();

                                // Scegli casualmente una di queste direzioni
                                directionIndex = underutilizedDirections[random.Next(underutilizedDirections.Count)];
                            }
                            else
                            {
                                // Tutte le direzioni hanno almeno 2 passi, scelta completamente casuale
                                directionIndex = random.Next(4);
                            }
                        }
                        else
                        {
                            // Scelta completamente casuale
                            directionIndex = random.Next(4);
                        }

                        ushort arrowKey = (ushort)ArrowKeys[directionIndex];
                        string arrowName = ArrowNames[directionIndex];

                        Logger.Loggin($"Passo {step + 1}/{totalSteps}: {arrowName} per {keyPressTime}ms");

                        // Prepara l'input per premere il tasto freccia
                        INPUT[] keyDown = new INPUT[1];
                        keyDown[0].type = INPUT_KEYBOARD;
                        keyDown[0].union.ki.wVk = arrowKey;
                        keyDown[0].union.ki.wScan = 0;
                        keyDown[0].union.ki.dwFlags = KEYEVENTF_KEYDOWN;
                        keyDown[0].union.ki.time = 0;
                        keyDown[0].union.ki.dwExtraInfo = IntPtr.Zero;

                        // Pressione tasto
                        SendInput(1, keyDown, Marshal.SizeOf(typeof(INPUT)));
                        Thread.Sleep(keyPressTime);

                        // Prepara l'input per rilasciare il tasto freccia
                        INPUT[] keyUp = new INPUT[1];
                        keyUp[0].type = INPUT_KEYBOARD;
                        keyUp[0].union.ki.wVk = arrowKey;
                        keyUp[0].union.ki.wScan = 0;
                        keyUp[0].union.ki.dwFlags = KEYEVENTF_KEYUP;
                        keyUp[0].union.ki.time = 0;
                        keyUp[0].union.ki.dwExtraInfo = IntPtr.Zero;

                        // Rilascio tasto
                        SendInput(1, keyUp, Marshal.SizeOf(typeof(INPUT)));

                        // Aggiorna conteggio per questa direzione
                        singleDirectionCounts[directionIndex]++;
                        singleKeySteps++;
                    }

                    // Pausa casuale tra i passi (se non è l'ultimo passo)
                    if (step < totalSteps - 1)
                    {
                        int pauseTime = random.Next(minPauseBetweenSteps, maxPauseBetweenSteps + 1);
                        Thread.Sleep(pauseTime);
                    }
                }

                // Verifica che tutte le direzioni siano state utilizzate
                bool allDirectionsUsed = singleDirectionCounts.All(count => count > 0);
                bool allDirectionsUsedTwice = singleDirectionCounts.All(count => count >= 2);

                // Stampa un riepilogo
                Logger.Loggin("Camminata randomizzata completata.");
                Logger.Loggin($"Passi singoli: {singleKeySteps}, Passi diagonali: {comboSteps}");
                Logger.Loggin($"Direzioni: SU={singleDirectionCounts[0]}, DESTRA={singleDirectionCounts[1]}, GIÙ={singleDirectionCounts[2]}, SINISTRA={singleDirectionCounts[3]}");

                if (comboSteps > 0)
                {
                    Logger.Loggin("Combinazioni diagonali utilizzate:");
                    for (int i = 0; i < ValidCombinations.Length; i++)
                    {
                        if (combinationCounts[i] > 0)
                        {
                            Logger.Loggin($"  - {ValidCombinations[i]}: {combinationCounts[i]} volte");
                        }
                    }
                }

                if (!allDirectionsUsed)
                {
                    Logger.Loggin("Attenzione: non tutte le direzioni sono state utilizzate!");
                }
                else if (!allDirectionsUsedTwice)
                {
                    Logger.Loggin("Attenzione: non tutte le direzioni sono state utilizzate almeno due volte.");
                }
                else
                {
                    Logger.Loggin("Tutte le direzioni sono state utilizzate almeno due volte.");
                }
            }
            catch (Exception e)
            {
                Logger.Loggin(e.Message, true);
            }
            finally
            {
                if (installHook && hookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(hookID);
                    Logger.Loggin("Hook rimosso.");
                }
            }
        }

    }
}
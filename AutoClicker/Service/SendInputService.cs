using AutoClicker.Library;
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
        MouseInputSimulator MouseInputSimulator = new MouseInputSimulator();
        KeyboardInputSimulator KeyboardInputSimulator = new KeyboardInputSimulator();

        public SendInputService()
        {
        }

        /// Simula la pressione prolungata del tasto freccia destra per 3 secondi
        /// </summary>
        public void SimulateRightArrowFor3Seconds()
        {
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
            return MouseInputSimulator.DragAndDrop(startX, startY, endX, endY);
        }

        public Task<POINT> BeginCaptureAsync()
        {
            return MouseInputSimulator.BeginCaptureAsync();
        }

        /// <summary>
        /// Simula la pressione di una combinazione di tasti
        /// </summary>
        /// <param name="key1">Primo tasto da premere</param>
        /// <param name="key2">Secondo tasto da premere</param>
        /// <param name="pressDuration">Durata della pressione in millisecondi</param>
        public void SimulateKeyCombo(ushort key1, ushort key2, int pressDuration = 100)
        {
            //// Crea array per contenere 4 eventi: due key down e due key up
            //INPUT[] inputs = new INPUT[4];

            //// Configurazione primo keydown
            //inputs[0].type = INPUT_KEYBOARD;
            //inputs[0].union.ki.wVk = key1;
            //inputs[0].union.ki.wScan = 0;
            //inputs[0].union.ki.dwFlags = KEYEVENTF_KEYDOWN;
            //inputs[0].union.ki.time = 0;
            //inputs[0].union.ki.dwExtraInfo = IntPtr.Zero;

            //// Configurazione secondo keydown
            //inputs[1].type = INPUT_KEYBOARD;
            //inputs[1].union.ki.wVk = key2;
            //inputs[1].union.ki.wScan = 0;
            //inputs[1].union.ki.dwFlags = KEYEVENTF_KEYDOWN;
            //inputs[1].union.ki.time = 0;
            //inputs[1].union.ki.dwExtraInfo = IntPtr.Zero;

            //// Configurazione secondo keyup (invertiamo l'ordine per i keyup)
            //inputs[2].type = INPUT_KEYBOARD;
            //inputs[2].union.ki.wVk = key2;
            //inputs[2].union.ki.wScan = 0;
            //inputs[2].union.ki.dwFlags = KEYEVENTF_KEYUP;
            //inputs[2].union.ki.time = 0;
            //inputs[2].union.ki.dwExtraInfo = IntPtr.Zero;

            //// Configurazione primo keyup
            //inputs[3].type = INPUT_KEYBOARD;
            //inputs[3].union.ki.wVk = key1;
            //inputs[3].union.ki.wScan = 0;
            //inputs[3].union.ki.dwFlags = KEYEVENTF_KEYUP;
            //inputs[3].union.ki.time = 0;
            //inputs[3].union.ki.dwExtraInfo = IntPtr.Zero;

            //// Invia i primi due eventi (keydown)
            //SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));

            //// Attendi per la durata specificata
            //Thread.Sleep(pressDuration);

            //// Invia gli ultimi due eventi (keyup)
            //SendInput(2, new INPUT[] { inputs[2], inputs[3] }, Marshal.SizeOf(typeof(INPUT)));
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
            //Logger.Loggin($"Inizio camminata randomizzata con {totalSteps} passi...");

            //if (installHook)
            //{
            //    Logger.Loggin("Installazione hook di tastiera...");
            //    hookID = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, IntPtr.Zero, 0);
            //}

            //try
            //{
            //    Random random = new Random();

            //    // Contatori per ogni direzione e combinazione
            //    int[] singleDirectionCounts = new int[4] { 0, 0, 0, 0 };
            //    int[] combinationCounts = new int[ValidCombinations.Length];

            //    // Contatori per tipo di passo
            //    int singleKeySteps = 0;
            //    int comboSteps = 0;

            //    for (int step = 0; step < totalSteps; step++)
            //    {
            //        // Determina se la pressione sarà semplice o combinata
            //        bool useCombo = random.Next(100) < comboChance;

            //        // Tempo di pressione casuale
            //        int keyPressTime = random.Next(minKeyPressTime, maxKeyPressTime + 1);

            //        if (useCombo && comboSteps < totalSteps / 2) // Limita le combinazioni a massimo metà dei passi totali
            //        {
            //            // Scegli una combinazione valida
            //            int comboIndex = random.Next(ValidCombinations.Length);
            //            ArrowCombination combo = ValidCombinations[comboIndex];

            //            Logger.Loggin($"Passo {step + 1}/{totalSteps}: {combo} per {keyPressTime}ms");

            //            // Simula combinazione di tasti
            //            SimulateKeyCombo(((ushort)combo.Key1), ((ushort)combo.Key2), keyPressTime);

            //            // Aggiorna il contatore di passi combinati
            //            combinationCounts[comboIndex]++;
            //            comboSteps++;
            //        }
            //        else
            //        {
            //            // Sceglie una direzione casuale
            //            int directionIndex;

            //            // Se siamo oltre la metà dei passi, assicuriamoci di coprire le direzioni mancanti
            //            if (step >= totalSteps / 2)
            //            {
            //                // Trova la direzione meno utilizzata
            //                int minDirectionCount = singleDirectionCounts.Min();

            //                if (minDirectionCount < 2)
            //                {
            //                    // Trova tutte le direzioni che hanno meno di 2 passi
            //                    var underutilizedDirections = Enumerable.Range(0, 4)
            //                        .Where(i => singleDirectionCounts[i] < 2)
            //                        .ToList();

            //                    // Scegli casualmente una di queste direzioni
            //                    directionIndex = underutilizedDirections[random.Next(underutilizedDirections.Count)];
            //                }
            //                else
            //                {
            //                    // Tutte le direzioni hanno almeno 2 passi, scelta completamente casuale
            //                    directionIndex = random.Next(4);
            //                }
            //            }
            //            else
            //            {
            //                // Scelta completamente casuale
            //                directionIndex = random.Next(4);
            //            }

            //            ushort arrowKey = (ushort)ArrowKeys[directionIndex];
            //            string arrowName = ArrowNames[directionIndex];

            //            Logger.Loggin($"Passo {step + 1}/{totalSteps}: {arrowName} per {keyPressTime}ms");

            //            // Prepara l'input per premere il tasto freccia
            //            INPUT[] keyDown = new INPUT[1];
            //            keyDown[0].type = INPUT_KEYBOARD;
            //            keyDown[0].union.ki.wVk = arrowKey;
            //            keyDown[0].union.ki.wScan = 0;
            //            keyDown[0].union.ki.dwFlags = KEYEVENTF_KEYDOWN;
            //            keyDown[0].union.ki.time = 0;
            //            keyDown[0].union.ki.dwExtraInfo = IntPtr.Zero;

            //            // Pressione tasto
            //            SendInput(1, keyDown, Marshal.SizeOf(typeof(INPUT)));
            //            Thread.Sleep(keyPressTime);

            //            // Prepara l'input per rilasciare il tasto freccia
            //            INPUT[] keyUp = new INPUT[1];
            //            keyUp[0].type = INPUT_KEYBOARD;
            //            keyUp[0].union.ki.wVk = arrowKey;
            //            keyUp[0].union.ki.wScan = 0;
            //            keyUp[0].union.ki.dwFlags = KEYEVENTF_KEYUP;
            //            keyUp[0].union.ki.time = 0;
            //            keyUp[0].union.ki.dwExtraInfo = IntPtr.Zero;

            //            // Rilascio tasto
            //            SendInput(1, keyUp, Marshal.SizeOf(typeof(INPUT)));

            //            // Aggiorna conteggio per questa direzione
            //            singleDirectionCounts[directionIndex]++;
            //            singleKeySteps++;
            //        }

            //        // Pausa casuale tra i passi (se non è l'ultimo passo)
            //        if (step < totalSteps - 1)
            //        {
            //            int pauseTime = random.Next(minPauseBetweenSteps, maxPauseBetweenSteps + 1);
            //            Thread.Sleep(pauseTime);
            //        }
            //    }

            //    // Verifica che tutte le direzioni siano state utilizzate
            //    bool allDirectionsUsed = singleDirectionCounts.All(count => count > 0);
            //    bool allDirectionsUsedTwice = singleDirectionCounts.All(count => count >= 2);

            //    // Stampa un riepilogo
            //    Logger.Loggin("Camminata randomizzata completata.");
            //    Logger.Loggin($"Passi singoli: {singleKeySteps}, Passi diagonali: {comboSteps}");
            //    Logger.Loggin($"Direzioni: SU={singleDirectionCounts[0]}, DESTRA={singleDirectionCounts[1]}, GIÙ={singleDirectionCounts[2]}, SINISTRA={singleDirectionCounts[3]}");

            //    if (comboSteps > 0)
            //    {
            //        Logger.Loggin("Combinazioni diagonali utilizzate:");
            //        for (int i = 0; i < ValidCombinations.Length; i++)
            //        {
            //            if (combinationCounts[i] > 0)
            //            {
            //                Logger.Loggin($"  - {ValidCombinations[i]}: {combinationCounts[i]} volte");
            //            }
            //        }
            //    }

            //    if (!allDirectionsUsed)
            //    {
            //        Logger.Loggin("Attenzione: non tutte le direzioni sono state utilizzate!");
            //    }
            //    else if (!allDirectionsUsedTwice)
            //    {
            //        Logger.Loggin("Attenzione: non tutte le direzioni sono state utilizzate almeno due volte.");
            //    }
            //    else
            //    {
            //        Logger.Loggin("Tutte le direzioni sono state utilizzate almeno due volte.");
            //    }
            //}
            //catch (Exception e)
            //{
            //    Logger.Loggin(e.Message, true);
            //}
            //finally
            //{
            //    if (installHook && hookID != IntPtr.Zero)
            //    {
            //        UnhookWindowsHookEx(hookID);
            //        Logger.Loggin("Hook rimosso.");
            //    }
            //}
        }

        public void SimulateF10Press(int times = 1)
        {
            KeyboardInputSimulator.SimulateF10Press(times);
        }

    }
}
using AutoClicker.Const;
using AutoClicker.Models.System;
using AutoClicker.Utils;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace AutoClicker.Service
{
    public class PostMessageService
    {

        AutoClickerLogger Logger = new();
        ProcessService ProcessService = new ProcessService();

        // Metodo per convertire coordinate per LPARAM
        private IntPtr MakeLParam(int x, int y)
        {
            return (IntPtr)((y << 16) | (x & 0xFFFF));
        }

        /// <summary>
        /// Simula la pressione prolungata del tasto freccia destra per 3 secondi
        /// </summary>
        public void SimulateRightArrowFor3Seconds()
        {
            Logger.Loggin("Inizio simulazione tasto destro con PostMessage...");
            // Tempo di inizio
            DateTime endTime = DateTime.Now.AddSeconds(3);
            // Continua a inviare il tasto destro per 3 secondi
            int i = 100;
            while (i<100)
            {
                i++;
                // Invia pressione tasto
                User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_KEYDOWN, (IntPtr)KeyboardMouseConst.VK_RIGHT, IntPtr.Zero);
                // Piccola pausa tra i messaggi per non sovraccaricarlo
                Thread.Sleep(10);
                // Non inviamo il keyup per mantenere il tasto premuto
                // Per una simulazione più realistica potresti voler inviare
                // anche WM_KEYUP alla fine del ciclo
            }
            // Rilascia il tasto alla fine
            User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_KEYUP, (IntPtr)KeyboardMouseConst.VK_RIGHT, IntPtr.Zero);
            
            Logger.Loggin("Simulazione completata.");
        }

        /// <summary>
        /// Esegue un'operazione di click, trascinamento e rilascio tra due punti
        /// </summary>
        /// <param name="startX">Coordinata X iniziale</param>
        /// <param name="startY">Coordinata Y iniziale</param>
        /// <param name="endX">Coordinata X finale</param>
        /// <param name="endY">Coordinata Y finale</param>
        /// <param name="duration">Durata del trascinamento in millisecondi</param>
        public void DragAndDrop(int startX, int startY, int endX, int endY, int duration = 500)
        {
            Logger.Loggin($"Esecuzione drag and drop da ({startX},{startY}) a ({endX},{endY})");

            // Converti le coordinate al sistema di coordinate dello schermo
            User32DLL.POINT startPoint = new User32DLL.POINT { X = startX, Y = startY };
            User32DLL.ClientToScreen(ProcessService.TheMiracleWindow.Hwnd, ref startPoint);

            User32DLL.POINT endPoint = new User32DLL.POINT { X = endX, Y = endY };
            User32DLL.ClientToScreen(ProcessService.TheMiracleWindow.Hwnd, ref endPoint);

            // Posiziona il cursore al punto iniziale
            User32DLL.SetCursorPos(startPoint.X, startPoint.Y);
            Thread.Sleep(100);

            // Invia click sinistro down
            User32DLL.mouse_event(KeyboardMouseConst.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(100);

            // Esegui il trascinamento con interpolazione
            int steps = duration / 10; // Un movimento ogni 10ms
            for (int i = 0; i <= steps; i++)
            {
                // Calcola posizione intermedia
                int curX = startPoint.X + ((endPoint.X - startPoint.X) * i / steps);
                int curY = startPoint.Y + ((endPoint.Y - startPoint.Y) * i / steps);

                // Sposta il cursore
                User32DLL.SetCursorPos(curX, curY);
                Thread.Sleep(10);
            }

            // Rilascia il pulsante sinistro del mouse
            User32DLL.mouse_event(KeyboardMouseConst.MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);

            Logger.Loggin("Operazione di drag and drop completata.");
        }

        /// <summary>
        /// Esegue un'operazione di drag and drop usando PostMessage invece di mouse_event
        /// </summary>
        public void DragAndDropUsingPostMessage(int startX, int startY, int endX, int endY, int duration = 500)
        {
            Logger.Loggin($"Esecuzione drag and drop con PostMessage da ({startX},{startY}) a ({endX},{endY})");

            // Invia click sinistro down
            User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_LBUTTONDOWN, IntPtr.Zero, MakeLParam(startX, startY));
            Thread.Sleep(100);

            // Esegui il trascinamento con interpolazione
            int steps = duration / 10; // Un movimento ogni 10ms
            for (int i = 0; i <= steps; i++)
            {
                // Calcola posizione intermedia
                int curX = startX + ((endX - startX) * i / steps);
                int curY = startY + ((endY - startY) * i / steps);

                // Invia messaggio di movimento mouse
                User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(curX, curY));
                Thread.Sleep(10);
            }

            // Rilascia il pulsante sinistro
            User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_LBUTTONUP, IntPtr.Zero, MakeLParam(endX, endY));

            Logger.Loggin("Operazione di drag and drop completata.");
        }

        /// <summary>
        /// Simula la pressione di una combinazione di tasti
        /// </summary>
        /// <param name="modifier">Tasto modificatore (es. CTRL, ALT, SHIFT)</param>
        /// <param name="key">Tasto principale</param>
        public void SimulateKeyCombo(int modifier, int key)
        {
            // Premere il tasto modificatore
            User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_KEYDOWN, (IntPtr)modifier, IntPtr.Zero);
            Thread.Sleep(50);

            // Premere il tasto principale
            User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_KEYDOWN, (IntPtr)key, IntPtr.Zero);
            Thread.Sleep(50);

            // Rilasciare il tasto principale
            User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_KEYUP, (IntPtr)key, IntPtr.Zero);
            Thread.Sleep(50);

            // Rilasciare il tasto modificatore
            User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_KEYUP, (IntPtr)modifier, IntPtr.Zero);
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
        public void RandomizedWalk(int totalSteps = 8,
                                  int minKeyPressTime = 100,
                                  int maxKeyPressTime = 300,
                                  int minPauseBetweenSteps = 200,
                                  int maxPauseBetweenSteps = 500,
                                  int comboChance = 30)
        {
            Logger.Loggin($"Inizio camminata randomizzata con {totalSteps} passi...");

            Random random = new Random();

            // Contatori per ogni direzione e combinazione
            int[] singleDirectionCounts = new int[4] { 0, 0, 0, 0 };
            int[] combinationCounts = new int[KeyboardMouseConst.ValidCombinations.Length];

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
                    int comboIndex = random.Next(KeyboardMouseConst.ValidCombinations.Length);
                    ArrowCombination combo = KeyboardMouseConst.ValidCombinations[comboIndex];

                    Logger.Loggin($"Passo {step + 1}/{totalSteps}: {combo} per {keyPressTime}ms");

                    // Premi entrambi i tasti
                    User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_KEYDOWN, (IntPtr)combo.Key1, IntPtr.Zero);
                    Thread.Sleep(20);
                    User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_KEYDOWN, (IntPtr)combo.Key2, IntPtr.Zero);
                    Thread.Sleep(keyPressTime);

                    // Rilascia entrambi i tasti
                    User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_KEYUP, (IntPtr)combo.Key2, IntPtr.Zero);
                    Thread.Sleep(20);
                    User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_KEYUP, (IntPtr)combo.Key1, IntPtr.Zero);

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

                    int arrowKey = KeyboardMouseConst.ArrowKeys[directionIndex];
                    string arrowName = KeyboardMouseConst.ArrowNames[directionIndex];

                    Logger.Loggin($"Passo {step + 1}/{totalSteps}: {arrowName} per {keyPressTime}ms");

                    // Invia pressione tasto freccia
                    User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_KEYDOWN, (IntPtr)arrowKey, IntPtr.Zero);
                    Thread.Sleep(keyPressTime);

                    // Rilascia tasto freccia
                    User32DLL.PostMessage(ProcessService.TheMiracleWindow.Hwnd, KeyboardMouseConst.WM_KEYUP, (IntPtr)arrowKey, IntPtr.Zero);

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
                for (int i = 0; i < KeyboardMouseConst.ValidCombinations.Length; i++)
                {
                    if (combinationCounts[i] > 0)
                    {
                        Logger.Loggin($"  - {KeyboardMouseConst.ValidCombinations[i]}: {combinationCounts[i]} volte");
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
    }
}
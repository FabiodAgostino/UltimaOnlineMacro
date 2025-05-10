using AutoClicker.Const;
using AutoClicker.Library;
using LogManager;
using static AutoClicker.Const.KeyboardMouseConst;
using static User32DLL;
using AutoClicker.Service.ExtensionMethod;
using Image = AutoClicker.Service.ExtensionMethod.Image;
using AutoClicker.Models.System;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
namespace AutoClicker.Service
{
    public class SendInputService
    {
        private MouseInputSimulator MouseInputSimulator { get; set; }
        private KeyboardInputSimulator KeyboardInputSimulator {  get; set; }
        private ProcessService processService { get; set; }
        public SendInputService(ProcessService processService)
        {
            this.processService = processService;
            MouseInputSimulator = new MouseInputSimulator(processService);
            KeyboardInputSimulator = new KeyboardInputSimulator(processService);
        }

        /// Simula la pressione prolungata del tasto freccia destra per 3 secondi
        /// </summary>
        public async Task TestMouse()
        {
            await processService.FocusWindowReliably();
            await MouseInputSimulator.SimulateDoubleClick10Times(100, 400);
            await processService.RestoreOldWindow();

        }

        public async Task TestKeyboard()
        {
            await processService.FocusWindowReliably();

            for (int i = 0; i < 10; i++)
            {
                await KeyboardInputSimulator.Move(KeyboardMouseConst.VK_LEFT);
            }
            await processService.RestoreOldWindow();

        }

        public async Task<bool> DragAndDrop(int startX, int startY, int endX, int endY, int duration = 500, bool installHook = false)
        {
            await processService.FocusWindowReliably();
            var value = await MouseInputSimulator.DragAndDrop(startX, startY, endX, endY);
            await processService.RestoreOldWindow();
            return value;
        }

        public async Task<bool> DragAndDropIron(int startX, int startY, int endX, int endY)
        {
            await processService.FocusWindowReliably();
            await KeyboardInputSimulator.ClickUnclickShift();
            var success = await MouseInputSimulator.DragAndDrop(startX, startY, endX, endY, 150);
            await KeyboardInputSimulator.ClickUnclickShift(false);
            await processService.RestoreOldWindow();
            return success;
        }

        public Task<POINT> BeginCaptureAsync()
        {
            return MouseInputSimulator.BeginCaptureAsync();
        }

        public async Task Login()
        {
            await KeyboardInputSimulator.LogIn();
        }
        //TO DO FINIRE QUESTA PARTE
        public byte? GetDirectionFromBlackArea()
        {
            var screen = Image.CaptureCenterScreenshot(300, 300);
            var gray = screen.bitmap.ToImage<Gray, byte>();

            // Migliora il contrasto dell'immagine prima della soglia
            gray = gray.SmoothGaussian(3); // Riduce il rumore

            // Usa una soglia adattiva che funziona meglio in diverse condizioni di illuminazione
            var threshold = gray.ThresholdBinaryInv(new Gray(30), new Gray(255));

            int midX = threshold.Width / 2;
            int midY = threshold.Height / 2;

            // Definisci i quadranti con maggiore precisione
            // Considera un'area più vicina al personaggio (che presumibilmente è al centro)
            int innerRadius = 50; // Più piccolo per concentrarsi sull'area immediata attorno al personaggio
            int centerX = threshold.Width / 2;
            int centerY = threshold.Height / 2;

            // Definisci 8 direzioni invece di 4 per una navigazione più precisa
            var topArea = threshold.GetSubRect(new Rectangle(centerX - midX / 2, 0, midX, centerY - innerRadius));
            var bottomArea = threshold.GetSubRect(new Rectangle(centerX - midX / 2, centerY + innerRadius, midX, midY - innerRadius));
            var leftArea = threshold.GetSubRect(new Rectangle(0, centerY - midY / 2, centerX - innerRadius, midY));
            var rightArea = threshold.GetSubRect(new Rectangle(centerX + innerRadius, centerY - midY / 2, midX - innerRadius, midY));

            // Calcola la densità di pixel neri in ciascuna area (normalizzata per dimensione)
            float topDensity = (float)CvInvoke.CountNonZero(topArea) / (topArea.Width * topArea.Height);
            float bottomDensity = (float)CvInvoke.CountNonZero(bottomArea) / (bottomArea.Width * bottomArea.Height);
            float leftDensity = (float)CvInvoke.CountNonZero(leftArea) / (leftArea.Width * leftArea.Height);
            float rightDensity = (float)CvInvoke.CountNonZero(rightArea) / (rightArea.Width * rightArea.Height);

            var blackDensity = new Dictionary<byte, float>
    {
        { VK_DOWN, topDensity },    // se il nero è sopra, muoviti verso il basso
        { VK_UP, bottomDensity },   // se il nero è sotto, muoviti verso l'alto
        { VK_RIGHT, leftDensity },  // se il nero è a sinistra, muoviti verso destra
        { VK_LEFT, rightDensity }   // se il nero è a destra, muoviti verso sinistra
    };

            // Trova la direzione con più nero (normalizzata per dimensione)
            var direction = blackDensity.OrderByDescending(kv => kv.Value).First();

            // Usa una soglia di densità invece di conteggio assoluto
            if (direction.Value < 0.1) // 10% di densità come soglia minima
            {
                Logger.Loggin("Nero non rilevato in modo significativo in nessuna direzione.", false,false);
                return null;
            }

            Logger.Loggin($"Direzione con più nero: {GetDirectionName(direction.Key)}, Densità: {direction.Value:F2}", false, false);

            return direction.Key;
        }

        private string GetDirectionName(byte direction)
        {
            switch (direction)
            {
                case VK_UP: return "UP";
                case VK_DOWN: return "DOWN";
                case VK_LEFT: return "LEFT";
                case VK_RIGHT: return "RIGHT";
                default: return direction.ToString();
            }
        }



        public async Task MoveRandomly(int numberOfSteps)
        {
            try
            {
                await processService.FocusWindowReliably();
                Random random = new Random();
                var value = GetDirectionFromBlackArea();


                // Scegli casualmente una sola direzione all'inizio
                byte selectedDirection = value.HasValue ? value.Value : ArrowKeys[random.Next(ArrowKeys.Length)];


                for (int i = 0; i < numberOfSteps; i++)
                {
                    await KeyboardInputSimulator.Move(selectedDirection);
                }

                await processService.RestoreOldWindow();
            }
            catch (Exception e)
            {
                Logger.Loggin($"Errore durante la simulazione dei movimenti direzionali: {e.Message}", true,false);
            }
        }

        public async Task RunMacro(List<Keys> keys)
        {
            await KeyboardInputSimulator.SimulateMacroWithModifiers(keys);
        }




































        private (byte BestDirection, float TotalDensity) AnalyzeBlackAreas()
        {
            var screen = Image.CaptureCenterScreenshot(300, 300);
            var gray = screen.bitmap.ToImage<Gray, byte>();

            // Migliora il contrasto dell'immagine prima della soglia
            gray = gray.SmoothGaussian(3);

            // Usa una soglia adattiva per individuare meglio le aree nere
            var threshold = gray.ThresholdBinaryInv(new Gray(30), new Gray(255));

            // Parametri per l'analisi
            int width = threshold.Width;
            int height = threshold.Height;
            int centerX = width / 2;
            int centerY = height / 2;

            // Dimensioni per le aree di analisi - più semplici e dirette
            int innerRadius = 20;  // Area più vicina al personaggio

            // Crea solo i 4 settori principali (N, E, S, W) per un'analisi più diretta
            var sectors = new Dictionary<string, Rectangle>
    {
        { "N", new Rectangle(0, 0, width, centerY - innerRadius) },
        { "E", new Rectangle(centerX + innerRadius, 0, width - (centerX + innerRadius), height) },
        { "S", new Rectangle(0, centerY + innerRadius, width, height - (centerY + innerRadius)) },
        { "W", new Rectangle(0, 0, centerX - innerRadius, height) }
    };

            // Calcola la densità di pixel neri in ciascun settore
            var sectorDensities = new Dictionary<string, float>();
            float totalArea = 0;
            float totalNonZeroPixels = 0;

            foreach (var sector in sectors)
            {
                // Estrai il settore
                var roi = threshold.GetSubRect(sector.Value);
                int nonZeroPixels = CvInvoke.CountNonZero(roi);
                int totalPixels = roi.Width * roi.Height;

                // Calcola la densità (percentuale di pixel neri)
                float density = (float)nonZeroPixels / totalPixels;
                sectorDensities[sector.Key] = density;

                totalArea += totalPixels;
                totalNonZeroPixels += nonZeroPixels;

                Logger.Loggin($"Settore {sector.Key}: {density:P2} nero", false, false);
            }

            // Calcola la densità totale di nero
            float totalDensity = totalNonZeroPixels / totalArea;
            Logger.Loggin($"Densità totale di nero: {totalDensity:P2}", false, false);

            // Mappa diretta e chiara dei settori alle direzioni
            var directionMapping = new Dictionary<string, byte>
    {
        { "N", VK_UP },    // Se il nero è a Nord, muoviti verso Sud (DOWN)
        { "E", VK_LEFT },  // Se il nero è a Est, muoviti verso Ovest (LEFT)
        { "S", VK_DOWN },  // Se il nero è a Sud, muoviti verso Nord (UP)
        { "W", VK_RIGHT }  // Se il nero è a Ovest, muoviti verso Est (RIGHT)
    };

            // Trova il settore con la maggiore densità di nero
            var darkestSector = sectorDensities.OrderByDescending(kv => kv.Value).First();

            // La direzione corrisponde all'opposto del settore più scuro (se hai nero a Nord, vai a Sud)
            byte bestDirection = GetOppositeDirection(directionMapping[darkestSector.Key]);

            Logger.Loggin($"Settore più scuro: {darkestSector.Key} ({darkestSector.Value:P2}), Direzione scelta: {GetDirectionName(bestDirection)}", false, false);

            return (bestDirection, totalDensity);
        }









        public async Task NavigateSmartly(int maxSteps = 6)
        {
            await processService.FocusWindowReliably();

            try
            {
                // Variabili per tenere traccia dello stato
                float previousDensity = 1.0f;
                int stepsWithoutImprovement = 0;
                HashSet<byte> triedDirections = new HashSet<byte>();
                Random random = new Random();

                // Analisi iniziale
                var initialAnalysis = AnalyzeBlackAreas();
                float initialDensity = initialAnalysis.TotalDensity;

                Logger.Loggin($"Densità iniziale di nero: {initialDensity:P2}", false, false);

                // Se non c'è nero significativo, muoviti casualmente e termina
                if (initialDensity < 0.1f)
                {
                    byte randomDirection = ArrowKeys[random.Next(ArrowKeys.Length)];
                    Logger.Loggin($"Nessun nero significativo rilevato. Mi sposto di 4 passi in direzione casuale: {GetDirectionName(randomDirection)}", false, false);

                    for (int i = 0; i < 4; i++)
                    {
                        await KeyboardInputSimulator.Move(randomDirection);
                        await Task.Delay(100);
                    }

                    return;
                }

                // Ciclo principale di navigazione - check del nero ad ogni passo
                for (int step = 0; step < maxSteps; step++)
                {
                    // Analizza la situazione attuale
                    var analysis = AnalyzeBlackAreas();
                    Logger.Loggin($"Passo {step + 1}/{maxSteps} - Densità di nero: {analysis.TotalDensity:P2}", false, false);

                    // Se siamo in un'area molto chiara, possiamo terminare
                    if (analysis.TotalDensity < 0.1f)
                    {
                        Logger.Loggin("Area sufficientemente chiara raggiunta, termino navigazione", false, false);
                        break;
                    }

                    // Verifica se c'è stato un miglioramento rispetto al passo precedente
                    bool isImproving = step == 0 || analysis.TotalDensity < previousDensity * 0.9f;

                    // Aggiorna la densità precedente per il prossimo confronto
                    previousDensity = analysis.TotalDensity;

                    if (isImproving)
                    {
                        // Se stiamo migliorando, resettiamo il contatore
                        stepsWithoutImprovement = 0;
                    }
                    else
                    {
                        // Se non stiamo migliorando, incrementiamo il contatore
                        stepsWithoutImprovement++;

                        // Se siamo rimasti bloccati per più passi, cambia direzione
                        if (stepsWithoutImprovement >= 2)
                        {
                            Logger.Loggin("Bloccato da troppi passi senza miglioramento, cambio strategia", false, false);

                            // Aggiungi l'ultima direzione tentata all'insieme delle direzioni provate
                            if (step > 0)
                            {
                                triedDirections.Add(analysis.BestDirection);
                            }

                            // Ripristina il contatore
                            stepsWithoutImprovement = 0;
                        }
                    }

                    // Determina la direzione per questo passo
                    byte directionToMove;

                    // Se abbiamo evidenza di blocco o stiamo usando una strategia alternativa
                    if (stepsWithoutImprovement >= 2 || (step > 0 && !isImproving))
                    {
                        // Trova direzioni non ancora provate
                        var availableDirections = ArrowKeys.Where(d => !triedDirections.Contains(d)).ToList();

                        if (availableDirections.Count > 0)
                        {
                            // Scegli una direzione non ancora provata
                            directionToMove = availableDirections[random.Next(availableDirections.Count)];
                            Logger.Loggin($"Provo una nuova direzione: {GetDirectionName(directionToMove)}", false, false);
                        }
                        else
                        {
                            // Se tutte le direzioni sono state provate, ripristina e scegline una casuale
                            triedDirections.Clear();
                            directionToMove = ArrowKeys[random.Next(ArrowKeys.Length)];
                            Logger.Loggin($"Tutte le direzioni provate, ricomincio con: {GetDirectionName(directionToMove)}", false, false);
                        }
                    }
                    else
                    {
                        // Usa la direzione migliore dall'analisi corrente
                        directionToMove = analysis.BestDirection;
                        Logger.Loggin($"Direzione scelta dall'analisi: {GetDirectionName(directionToMove)}", false, false);
                    }

                    // Esegui il movimento
                    await KeyboardInputSimulator.Move(directionToMove);
                    await Task.Delay(200);

                    // Analizza immediatamente dopo il movimento per verificare l'efficacia
                    var postMoveAnalysis = AnalyzeBlackAreas();

                    // Se il movimento ha peggiorato significativamente la situazione, torna indietro
                    if (postMoveAnalysis.TotalDensity > analysis.TotalDensity * 1.2) // Più di 20% peggio
                    {
                        Logger.Loggin($"Movimento in direzione {GetDirectionName(directionToMove)} ha peggiorato significativamente la situazione, torno indietro", false, false);

                        // Torna indietro
                        await KeyboardInputSimulator.Move(GetOppositeDirection(directionToMove));
                        await Task.Delay(200);
                       
                        // Segna questa direzione come provata
                        triedDirections.Add(directionToMove);

                        // Non contiamo questo passo
                        step--;
                    }
                    else if (postMoveAnalysis.TotalDensity < analysis.TotalDensity * 0.7) // Più di 30% meglio
                    {
                        // Se il movimento ha migliorato significativamente, fai un altro passo nella stessa direzione
                        Logger.Loggin($"Movimento in direzione {GetDirectionName(directionToMove)} ha migliorato significativamente la situazione, continuo", false, false);

                        await KeyboardInputSimulator.Move(directionToMove);
                        await Task.Delay(200);

                        // Verifica nuovamente
                        var secondMoveAnalysis = AnalyzeBlackAreas();

                        // Se ancora migliora o è stabile, fai un altro passo
                        if (secondMoveAnalysis.TotalDensity <= postMoveAnalysis.TotalDensity * 1.1)
                        {
                            await KeyboardInputSimulator.Move(directionToMove);
                            await Task.Delay(200);

                            Logger.Loggin($"Direzione {GetDirectionName(directionToMove)} continua a essere efficace", false, false);
                        }

                        // Questo conta come un solo passo aggiuntivo, anche se facciamo più movimenti
                        previousDensity = secondMoveAnalysis.TotalDensity;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Loggin($"Errore durante la navigazione: {e.Message}", true, false);
            }
            finally
            {
                await processService.RestoreOldWindow();
            }
        }

        

        // Metodo per ottenere la direzione opposta
        private byte GetOppositeDirection(byte direction)
        {
            switch (direction)
            {
                case VK_UP: return VK_DOWN;
                case VK_DOWN: return VK_UP;
                case VK_LEFT: return VK_RIGHT;
                case VK_RIGHT: return VK_LEFT;
                default: return direction;
            }
        }





        public async Task MoveAwayFromDarkness(int numberOfSteps)
        {
            try
            {
                await processService.FocusWindowReliably();
                Random random = new Random();

                // Esegui più analisi durante il movimento per adattarti agli ostacoli
                for (int i = 0; i < numberOfSteps; i++)
                {
                    // Rileva la direzione ad ogni passo
                    var directionValue = GetDirectionFromBlackArea();
                    byte selectedDirection;

                    if (directionValue.HasValue)
                    {
                        // Usa la direzione opposta a quella con più nero
                        selectedDirection = directionValue.Value;
                        Logger.Loggin($"Passo {i + 1}: Muovendosi nella direzione opposta al nero ({selectedDirection})",false,false);
                    }
                    else
                    {
                        // Se non c'è una direzione chiara, prova una direzione casuale
                        // ma favorisci direzioni non ancora provate
                        selectedDirection = ArrowKeys[random.Next(ArrowKeys.Length)];
                        Logger.Loggin($"Passo {i + 1}: Nessuna direzione chiara, provo casualmente ({selectedDirection})", false, false);
                    }

                    // Esegui il movimento
                    await KeyboardInputSimulator.Move(selectedDirection);

                    // Attendi brevemente per poter vedere gli effetti del movimento
                    await Task.Delay(100);

                    // Opzionale: Verifica se siamo ancora bloccati da nero dopo il movimento
                    var checkAfterMove = GetDirectionFromBlackArea();
                    if (checkAfterMove == null)
                    {
                        // Se non c'è più nero significativo attorno, possiamo ridurre i passi rimanenti
                        Logger.Loggin("Nessun nero significativo rilevato dopo il movimento, riduco i passi", false, false);
                        i += 1; // Saltiamo un passo
                    }
                }

                await processService.RestoreOldWindow();
            }
            catch (Exception e)
            {
                Logger.Loggin($"Errore durante la navigazione intelligente: {e.Message}", true, false);
            }
        }
    }
}
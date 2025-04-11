using AutoClicker.Library;
using AutoClicker.Models.TM;
using Microsoft.ML;
using Microsoft.ML.Data;
using static AutoClicker.Utils.User32DLL;
namespace AutoCliecker.Service
{
    public class MuloDetectorService
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<MuloImageData, MuloPrediction> _predictionEngine;
        private readonly string _tempImagePath;

        public MuloDetectorService(string modelPath)
        {
            _mlContext = new MLContext();
            _tempImagePath = Path.Combine(Path.GetTempPath(), "temp_screenshot.png");

            // Carica il modello
            ITransformer loadedModel = _mlContext.Model.Load(modelPath, out var modelInputSchema);

            // Crea il motore di predizione
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<MuloImageData, MuloPrediction>(loadedModel);
        }

        public Rectangle DetectMulo()
        {
            // Cattura screenshot
            var screenshot = AutoClicker.Service.ExtensionMethod.Image.CaptureCenterScreenshot();
            screenshot.bitmap.Save(_tempImagePath);

            // Predizione
            var prediction = _predictionEngine.Predict(
                new MuloImageData { ImagePath = _tempImagePath });

            float muloConfidence = 0;
            if (prediction.Score.Length >= 2)
            {
                var scoreColumn = _predictionEngine.OutputSchema["Score"];

                // Creiamo un buffer vuoto per ricevere i nomi degli slot
                VBuffer<ReadOnlyMemory<char>> slotNamesBuffer = default;
                scoreColumn.GetSlotNames(ref slotNamesBuffer);

                // Convertiamo i nomi degli slot in un array di stringhe
                var slotNames = new string[slotNamesBuffer.Length];
                for (int i = 0; i < slotNamesBuffer.Length; i++)
                {
                    var value = slotNamesBuffer.GetItemOrDefault(i);
                    slotNames[i] = value.IsEmpty ? string.Empty : value.ToString();
                }

                // Ora possiamo cercare l'indice di "Mulo"
                int muloIndex = Array.IndexOf(slotNames, "Mulo");
                if (muloIndex >= 0 && muloIndex < prediction.Score.Length)
                {
                    muloConfidence = prediction.Score[muloIndex];
                }
            }

            // Se la confidenza è alta, ritorna un rettangolo al centro dello screenshot
            if (prediction.PredictedLabelValue == "Mulo" && muloConfidence > 0.7)
            {
                // Qui dovresti implementare un metodo più preciso per trovare 
                // la posizione esatta del mulo all'interno dello screenshot
                // Per ora, ritorniamo semplicemente un rettangolo al centro
                int centerX = screenshot.bitmap.Width / 2;
                int centerY = screenshot.bitmap.Height / 2;
                return new Rectangle(
                    centerX - 25, // posizione X stimata
                    centerY - 25, // posizione Y stimata
                    50,          // larghezza stimata
                    50           // altezza stimata
                );
            }

            return Rectangle.Empty;
        }

        // Versione migliorata che divide lo screenshot in tile/regioni
        // per rilevare la posizione precisa del mulo
        public POINT DetectMuloPrecise()
        {
            // Cattura screenshot
            var screenshot = AutoClicker.Service.ExtensionMethod.Image.CaptureCenterScreenshot();
            // Definisci dimensione dei tile
            int tileWidth = 64;
            int tileHeight = 64;
            // Definisci overlap tra tile per evitare di perdere il mulo ai bordi
            int overlapX = 16;
            int overlapY = 16;
            float bestConfidence = 0;
            Rectangle bestRect = Rectangle.Empty;

            // Scansiona lo screenshot con tile sovrapposti
            for (int y = 0; y < screenshot.bitmap.Height - tileHeight; y += tileHeight - overlapY)
            {
                for (int x = 0; x < screenshot.bitmap.Width - tileWidth; x += tileWidth - overlapX)
                {
                    // Estrai il tile
                    using (Bitmap tile = new Bitmap(tileWidth, tileHeight))
                    {
                        using (Graphics g = Graphics.FromImage(tile))
                        {
                            g.DrawImage(screenshot.bitmap,
                                new Rectangle(0, 0, tileWidth, tileHeight),
                                new Rectangle(x, y, tileWidth, tileHeight),
                                GraphicsUnit.Pixel);
                        }
                        // Salva il tile temporaneamente
                        tile.Save(_tempImagePath);
                        // Predizione
                        var prediction = _predictionEngine.Predict(
                            new MuloImageData { ImagePath = _tempImagePath });

                        // Ottieni confidenza per la classe "Mulo" con il metodo corretto
                        float muloConfidence = 0;
                        var scoreColumn = _predictionEngine.OutputSchema["Score"];

                        // Creiamo un buffer vuoto per ricevere i nomi degli slot
                        VBuffer<ReadOnlyMemory<char>> slotNamesBuffer = default;
                        scoreColumn.GetSlotNames(ref slotNamesBuffer);

                        // Convertiamo i nomi degli slot in un array di stringhe
                        var slotNames = new string[slotNamesBuffer.Length];
                        for (int i = 0; i < slotNamesBuffer.Length; i++)
                        {
                            var value = slotNamesBuffer.GetItemOrDefault(i);
                            slotNames[i] = value.IsEmpty ? string.Empty : value.ToString();
                        }

                        // Ora possiamo cercare l'indice di "Mulo"
                        int muloIndex = Array.IndexOf(slotNames, "Mulo");
                        if (muloIndex >= 0 && muloIndex < prediction.Score.Length)
                        {
                            muloConfidence = prediction.Score[muloIndex];
                        }

                        // Se questo tile ha la confidenza più alta finora
                        if (prediction.PredictedLabelValue == "Mulo" && muloConfidence > bestConfidence)
                        {
                            bestConfidence = muloConfidence;
                            bestRect = new Rectangle(x, y, tileWidth, tileHeight);
                        }
                    }
                }
            }

            // Ritorna il rettangolo con la confidenza più alta, se supera la soglia
            if (bestConfidence > 0.7)
            {
                int centerX = bestRect.X + bestRect.Width / 2;
                int centerY = bestRect.Y + bestRect.Height / 2;
                return new POINT { X = centerX, Y = centerY };
            }
            else
                return new POINT();
        }

       
    }
}
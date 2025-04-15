using AutoClicker.Models.TM.MachineLearning;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using static AutoClicker.Utils.User32DLL;

namespace AutoClicker.Service
{
    public class MuloDetectorService
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<MuloImageData, MuloPrediction> _classifierPredictionEngine;
        private readonly PredictionEngine<MuloImageData, CoordinatePrediction> _xRegressor;
        private readonly PredictionEngine<MuloImageData, CoordinatePrediction> _yRegressor;
        private readonly string _tempImagePath;

        public MuloDetectorService(string classifierPath, string xModelPath, string yModelPath)
        {
            _mlContext = new MLContext();
            _tempImagePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "temp_screenshot.png");

            // Carica il modello classificatore
            var classifierModel = _mlContext.Model.Load(classifierPath, out _);
            _classifierPredictionEngine = _mlContext.Model.CreatePredictionEngine<MuloImageData, MuloPrediction>(classifierModel);

            // Carica i modelli regressori
            var xModel = _mlContext.Model.Load(xModelPath, out _);
            var yModel = _mlContext.Model.Load(yModelPath, out _);
            _xRegressor = _mlContext.Model.CreatePredictionEngine<MuloImageData, CoordinatePrediction>(xModel);
            _yRegressor = _mlContext.Model.CreatePredictionEngine<MuloImageData, CoordinatePrediction>(yModel);
        }

        public void DetectAndDrawMulo()
        {
            // 1. Cattura lo screenshot dal centro dello schermo
            var screenshot = AutoClicker.Service.ExtensionMethod.Image.CaptureCenterScreenshot();
            // Salva il file nel percorso _tempImagePath
            screenshot.bitmap.Save(_tempImagePath, ImageFormat.Png);

            // 2. Predizione classificazione
            var input = new MuloImageData { ImagePath = _tempImagePath };
            var classification = _classifierPredictionEngine.Predict(input);

            // Estrae l'indice della classe "Mulo"
            int muloIndex = GetMuloClassIndex(_classifierPredictionEngine);
            float confidence = (muloIndex >= 0 && classification.Score != null && muloIndex < classification.Score.Length)
                ? classification.Score[muloIndex]
                : 0f;

            Console.WriteLine($"🎯 Predizione: {classification.PredictedLabelValue} (confidenza: {confidence:P2})");

            // 3. Se la predizione è positiva e con confidenza > 0.8, procede
            if (classification.PredictedLabelValue == "Mulo" && confidence > 0.8f)
            {
                // Predice la coordinata X e Y
                var xPrediction = _xRegressor.Predict(input);
                var yPrediction = _yRegressor.Predict(input);

                int centerX = (int)xPrediction.Score;
                int centerY = (int)yPrediction.Score;

                Console.WriteLine($"📍 Mulo trovato a X: {centerX}, Y: {centerY}");

                // 4. Disegna un punto rosso sullo screenshot
                using (var bitmap = new Bitmap(screenshot.bitmap))
                using (var g = Graphics.FromImage(bitmap))
                using (var brush = new SolidBrush(Color.Red))
                {
                    int radius = 5;
                    g.FillEllipse(brush, centerX - radius, centerY - radius, radius * 2, radius * 2);

                    // 5. Salva l'immagine per debug
                    string debugPath = $"MuloDetected_{DateTime.Now:HHmmss}.png";
                    bitmap.Save(debugPath, ImageFormat.Png);
                    Console.WriteLine($"🖼️  Immagine salvata: {debugPath}");
                }
            }
            else
            {
                Console.WriteLine("🚫 Nessun mulo rilevato con sufficiente confidenza.");
            }
        }

        private int GetMuloClassIndex(PredictionEngine<MuloImageData, MuloPrediction> engine)
        {
            var scoreColumn = engine.OutputSchema["Score"];
            VBuffer<ReadOnlyMemory<char>> slotNames = default;
            scoreColumn.GetSlotNames(ref slotNames);
            var slotArray = slotNames.DenseValues().Select(v => v.ToString()).ToArray();
            // Stampa per debug gli slot trovati
            Console.WriteLine("Slot names: " + string.Join(", ", slotArray));
            return Array.IndexOf(slotArray, "Mulo");
        }
    }
}

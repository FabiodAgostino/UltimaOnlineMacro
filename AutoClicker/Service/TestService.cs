using AutoClicker.Models.System;
using System.Diagnostics;

namespace AutoClicker.Service
{
    public class TestService
    {
        public void MeasureExecutionTime(Rectangle rectangle)
        {
            // Inizializza e avvia il timer
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Esegui il metodo da misurare
            var tesseract = new TesserActService();
            var status = tesseract.GetStatusBar(rectangle);

            // Ferma il timer
            stopwatch.Stop();

            Console.WriteLine($"Tempo di esecuzione: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Tempo di esecuzione preciso: {stopwatch.Elapsed}");

            Debug.WriteLine($"[DEBUG] GetStatusBar ha impiegato {stopwatch.ElapsedMilliseconds} ms");

        }
    }
}

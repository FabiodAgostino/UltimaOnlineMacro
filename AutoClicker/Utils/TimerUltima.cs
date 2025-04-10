using System;
using System.Timers;
using Serilog;

namespace AutoClicker.Utils
{
    public class TimerUltima
    {
        private System.Timers.Timer timer;
        private DateTime startTime;
        private bool isRunning = false;
        private Action<string> updateLabelAction; // Azione per aggiornare il testo
        private ILogger logger;

        public event EventHandler<TimeSpan> OnTimerUpdate;
        public TimeSpan ElapsedTime { get; private set; }

        // Costruttore che accetta un'azione per aggiornare l'UI
        public TimerUltima(Action<string> updateLabelAction)
        {
            this.updateLabelAction = updateLabelAction;

            // Inizializza il timer
            timer = new System.Timers.Timer();
            timer.Interval = 100; // 100 millisecondi
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
        }

        // Avvia il timer
        public void Start()
        {
            if (!isRunning)
            {
                startTime = DateTime.Now;
                timer.Start();
                isRunning = true;
            }
        }

        public void Stop()
        {
            if (isRunning)
            {
                timer.Stop();
                isRunning = false;
            }
        }

        public void Reset()
        {
            Stop();
            ElapsedTime = TimeSpan.Zero;
            UpdateLabel();
        }

        public TimeSpan GetCurrentElapsedTime()
        {
            if (isRunning)
            {
                return DateTime.Now - startTime;
            }
            return ElapsedTime;
        }

        public string GetFormattedTime()
        {
            return ElapsedTime.ToString(@"hh\:mm\:ss\.fff");
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ElapsedTime = DateTime.Now - startTime;

            // Dato che siamo in un thread diverso dal thread UI,
            // dobbiamo stare attenti a come aggiorniamo l'UI
            try
            {
                // Lasciamo che chi utilizza la classe gestisca il cross-threading
                UpdateLabel();

                // Notifica ascoltatori dell'evento
                var handler = OnTimerUpdate;
                if (handler != null)
                {
                    handler(this, ElapsedTime);
                }
            }
            catch (Exception ex)
            {
                // Se c'è un errore di threading, lo registriamo
                if (logger != null)
                {
                    logger.Error(ex, "Errore durante l'aggiornamento del timer");
                }
            }
        }

        private void UpdateLabel()
        {
            // Lasciamo che l'azione fornita dall'esterno si occupi del cross-threading se necessario
            updateLabelAction?.Invoke(GetFormattedTime());
        }

        public bool IsRunning => isRunning;
    }
}
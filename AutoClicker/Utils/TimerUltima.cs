using Serilog;
using System.Timers;

namespace AutoClicker.Utils
{
    public class TimerUltima
    {
        private System.Timers.Timer timer;
        private DateTime startTime = DateTime.Now;
        private bool isRunning = false;
        private Action<string> updateLabelAction; // Azione per aggiornare il testo
        private ILogger logger;
        private TimeSpan accumulatedTime = TimeSpan.Zero;

        public event EventHandler<TimeSpan> OnTimerUpdate;

        public TimeSpan ElapsedTime { get; private set; }

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
                accumulatedTime += DateTime.Now - startTime;
                isRunning = false;
            }
        }

        public string GetFormattedTime()
        {
            return ElapsedTime.ToString(@"hh\:mm\:ss\.fff");
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ElapsedTime = accumulatedTime + (DateTime.Now - startTime);

            try
            {
                UpdateLabel();

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
            updateLabelAction?.Invoke(GetFormattedTime());
        }

        public bool IsRunning => isRunning;
    }
}
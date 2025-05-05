using System.Media;

namespace AutoClicker.Service
{
    public static class SoundsPlayerService
    {
        private static SoundPlayer _playerBeep = new SoundPlayer();

        public static void OnePlay(string file)
        {
            string soundFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", file);
            if (_playerBeep.SoundLocation != soundFile)
            {
                _playerBeep.SoundLocation = soundFile;
                _playerBeep.LoadAsync();  // Carica il file in modo asincrono
            }
            _playerBeep.Play();
        }

        public static void LoopPlay(string file)
        {
            string soundFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", file);
            if (_playerBeep.SoundLocation != soundFile)
            {
                _playerBeep.SoundLocation = soundFile;
                _playerBeep.LoadAsync();  // Carica il file in modo asincrono
            }
            _playerBeep.PlayLooping();
        }

        // Metodo opzionale per fermare la riproduzione
        public static void Stop()
        {
            _playerBeep.Stop();
        }

        // Metodo opzionale per il dispose quando non serve più
        public static void Dispose()
        {
            _playerBeep?.Dispose();
        }
    }
}

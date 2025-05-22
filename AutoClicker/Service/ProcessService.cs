using AutoClicker.Models.System;
using AutoClicker.Models.TM;
using LogManager;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Interop;
using static User32DLL;

namespace AutoClicker.Service
{
    public class ProcessService
    {
        public TMWindow TheMiracleWindow { get; set; }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowExA(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextA(IntPtr hwnd, StringBuilder lpString, int nMaxCount);
        private IntPtr previousForegroundWindow;

        public ProcessService()
        {
            SetTMWindow();
        }

        public async Task SetTMWindow(bool force = false)
        {
            if (TheMiracleWindow == null || force)
            {
                IntPtr hwnd = IntPtr.Zero;
                StringBuilder windowText = new StringBuilder(256);
                GetModuleHandleProcess();

                while ((hwnd = FindWindowExA(IntPtr.Zero, hwnd, null, null)) != IntPtr.Zero)
                {
                    GetWindowTextA(hwnd, windowText, windowText.Capacity);
                    if (windowText.ToString().Contains("ClassicUO") && windowText.ToString().Contains("TM Client"))
                    {
                        TheMiracleWindow.Title = windowText.ToString();
                        TheMiracleWindow.Hwnd = hwnd;
                        break;
                    }
                }
            }
        }

        public nint? GetClientPtr()
        {
            IntPtr hwnd = IntPtr.Zero;
            StringBuilder windowText = new StringBuilder(256);
            GetModuleHandleProcess();

            while ((hwnd = FindWindowExA(IntPtr.Zero, hwnd, null, null)) != IntPtr.Zero)
            {
                GetWindowTextA(hwnd, windowText, windowText.Capacity);
                if (windowText.ToString().Contains("ClassicUO"))
                {
                    return hwnd;
                }
            }
            return null;
        }

        /// <summary>
        /// Porta una finestra in primo piano in modo affidabile usando tecniche
        /// riscontrate in MacroUO
        /// </summary>
        public async Task FocusWindowReliably()
        {
            SaveCurrentForegroundWindow();
            // Ottiene l'ID del thread corrente
            uint currentThreadId = GetCurrentThreadId();

            // Ottiene l'ID del thread della finestra di destinazione
            uint targetThreadId = GetWindowThreadProcessId(TheMiracleWindow.Hwnd, out _);

            bool attachSuccess = false;

            try
            {
                ShowWindow(TheMiracleWindow.Hwnd, 3);

                // Collega i thread di input se sono diversi
                if (currentThreadId != targetThreadId)
                {
                    attachSuccess = AttachThreadInput(currentThreadId, targetThreadId, true);
                }

                SetForegroundWindow(TheMiracleWindow.Hwnd);

                await Task.Delay(100);

                SetForegroundWindow(TheMiracleWindow.Hwnd);
            }
            finally
            {
                // Scollega i thread se erano stati collegati
                if (attachSuccess)
                {
                    AttachThreadInput(currentThreadId, targetThreadId, false);
                }
            }
        }

        public async Task RestoreOldWindow()
        {
            await Task.Delay(500);
            SetForegroundWindow(previousForegroundWindow);
        }

        public void GetModuleHandleProcess()
        {
            var process = Process.GetCurrentProcess();
            if (process != null && process.MainModule != null)
            {
                var moduleName = process.MainModule.ModuleName;
                if (TheMiracleWindow == null)
                    TheMiracleWindow = new();

                TheMiracleWindow.ModuleHandle = GetModuleHandle(moduleName);
            }
            else
                throw new Exception("Process non valido");
        }


        public void SaveCurrentForegroundWindow()
        {
            IntPtr foregroundHwnd = GetForegroundWindow();
            string title = GetWindowTitle(foregroundHwnd);

            if (!string.IsNullOrEmpty(title) && !title.Contains("UltimaOnlineMacro"))
            {
                previousForegroundWindow = foregroundHwnd;
            }
            else
            {
                previousForegroundWindow = IntPtr.Zero;
            }
        }

        private string GetWindowTitle(IntPtr hwnd)
        {
            var sb = new System.Text.StringBuilder(256);
            GetWindowText(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }

        /// <summary>
        /// Gestisce il riavvio dell'updater
        /// </summary>
        /// <param name="macroFilePath">Percorso del file macro</param>
        /// <returns>true se il riavvio è stato eseguito</returns>
        public bool HandleClientRestartUpdater(string macroFilePath)
        {
            try
            {
                // Trova il percorso della directory "The Miracle" (cartella principale)
                string dataDir = Path.GetDirectoryName(macroFilePath); // Ottiene il percorso di "Zaltarish en'Loke"
                string profilesDir = Directory.GetParent(dataDir)?.FullName; // Ottiene il percorso di "Profiles"
                string tmClientDir = null;

                // Risali nella gerarchia delle cartelle fino a trovare la cartella principale
                DirectoryInfo currentDir = new DirectoryInfo(dataDir);
                while (currentDir != null)
                {
                    // Controlla se la directory corrente contiene TM Updater.exe
                    if (File.Exists(Path.Combine(currentDir.FullName, "TM Updater.exe")))
                    {
                        tmClientDir = currentDir.FullName;
                        break;
                    }

                    // Verifica se siamo nella cartella Data
                    if (currentDir.Name.Equals("Data", StringComparison.OrdinalIgnoreCase))
                    {
                        // La cartella principale è probabilmente il genitore
                        if (currentDir.Parent != null)
                        {
                            tmClientDir = currentDir.Parent.FullName;
                            break;
                        }
                    }

                    // Vai su di un livello
                    currentDir = currentDir.Parent;
                }

                if (tmClientDir != null)
                {
                    string tmUpdaterPath = Path.Combine(tmClientDir, "TM Updater.exe");

                    // Verifica se il file TM Updater.exe esiste
                    if (File.Exists(tmUpdaterPath))
                    {
                        // Termina tutti i processi che contengono "TM Client" nel nome
                        Process[] processes = Process.GetProcesses();
                        foreach (Process process in processes)
                        {
                            try
                            {
                                if (process.ProcessName.Contains("TM Client") ||
                                    (process.MainWindowTitle != null && process.MainWindowTitle.Contains("TM Client")))
                                {
                                    process.Kill();
                                    process.WaitForExit(5000); // Attende fino a 5 secondi che il processo termini
                                    Console.WriteLine($"Processo terminato: {process.ProcessName}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Errore nel terminare il processo: {ex.Message}");
                            }
                        }

                        // Avvia TM Updater.exe
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = tmUpdaterPath,
                            WorkingDirectory = tmClientDir,
                            UseShellExecute = true
                        };

                        Process.Start(startInfo);
                        Console.WriteLine($"Avviato: {tmUpdaterPath}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"TM Updater.exe non trovato in: {tmClientDir}");
                    }
                }
                else
                {
                    Console.WriteLine("Non è stato possibile trovare la directory principale di The Miracle");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il riavvio del client: {ex.Message}");
            }

            return false;
        }

        public async Task Logout(string macroFilePath)
        {
            string dataDir = Path.GetDirectoryName(macroFilePath); // Ottiene il percorso di "Zaltarish en'Loke"
            string profilesDir = Directory.GetParent(dataDir)?.FullName; // Ottiene il percorso di "Profiles"
            string tmClientDir = null;
            DirectoryInfo currentDir = new DirectoryInfo(dataDir);

            while (currentDir != null)
            {
                // Controlla se la directory corrente contiene TM Updater.exe
                if (File.Exists(Path.Combine(currentDir.FullName, "ClassicUO.exe")))
                {
                    tmClientDir = currentDir.FullName;
                    break;
                }

                // Verifica se siamo nella cartella Data
                if (currentDir.Name.Equals("Data", StringComparison.OrdinalIgnoreCase))
                {
                    // La cartella principale è probabilmente il genitore
                    if (currentDir.Parent != null)
                    {
                        tmClientDir = currentDir.Parent.FullName;
                        break;
                    }
                }

                // Vai su di un livello
                currentDir = currentDir.Parent;
            }

            if (tmClientDir != null)
            {
                string tmUpdaterPath = Path.Combine(tmClientDir, "ClassicUO.exe");

                // Verifica se il file TM ClassicUO.exe esiste
                if (File.Exists(tmUpdaterPath))
                {
                    // Termina tutti i processi che contengono "TM Client" nel nome
                    Process[] processes = Process.GetProcesses();
                    foreach (Process process in processes)
                    {
                        try
                        {
                            if (process.ProcessName.Contains("TM Client") ||
                                (process.MainWindowTitle != null && process.MainWindowTitle.Contains("TM Client")))
                            {
                                process.Kill();
                                process.WaitForExit(5000); // Attende fino a 5 secondi che il processo termini
                                Logger.Loggin($"Processo terminato: {process.ProcessName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Loggin($"Errore nel terminare il processo: {ex.Message}");
                        }
                    }
                    await Task.Delay(100);
                    // Avvia TM Updater.exe
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = tmUpdaterPath,
                        WorkingDirectory = tmClientDir,
                        UseShellExecute = true
                    };

                    Process.Start(startInfo);
                    Logger.Loggin($"Avviato: {tmUpdaterPath}");
                }
                else
                {
                    Logger.Loggin($"ClassicUO.exe non trovato in: {tmClientDir}", true);
                }
            }
        }

        /// <summary>
        /// Gestisce il riavvio dell'updater
        /// </summary>
        /// <param name="macroFilePath">Percorso del file macro</param>
        /// <returns>true se il riavvio è stato eseguito</returns>
        public async Task<bool> HandleRestartClient(string macroFilePath, int delay=100000)
        {
            try
            {
                // Trova il percorso della directory "The Miracle" (cartella principale)
                string dataDir = Path.GetDirectoryName(macroFilePath); // Ottiene il percorso di "Zaltarish en'Loke"
                string profilesDir = Directory.GetParent(dataDir)?.FullName; // Ottiene il percorso di "Profiles"
                string tmClientDir = null;

                // Risali nella gerarchia delle cartelle fino a trovare la cartella principale
                DirectoryInfo currentDir = new DirectoryInfo(dataDir);
                while (currentDir != null)
                {
                    // Controlla se la directory corrente contiene TM Updater.exe
                    if (File.Exists(Path.Combine(currentDir.FullName, "ClassicUO.exe")))
                    {
                        tmClientDir = currentDir.FullName;
                        break;
                    }

                    // Verifica se siamo nella cartella Data
                    if (currentDir.Name.Equals("Data", StringComparison.OrdinalIgnoreCase))
                    {
                        // La cartella principale è probabilmente il genitore
                        if (currentDir.Parent != null)
                        {
                            tmClientDir = currentDir.Parent.FullName;
                            break;
                        }
                    }

                    // Vai su di un livello
                    currentDir = currentDir.Parent;
                }

                if (tmClientDir != null)
                {
                    string tmUpdaterPath = Path.Combine(tmClientDir, "ClassicUO.exe");

                    // Verifica se il file TM ClassicUO.exe esiste
                    if (File.Exists(tmUpdaterPath))
                    {
                        // Termina tutti i processi che contengono "TM Client" nel nome
                        Process[] processes = Process.GetProcesses();
                        foreach (Process process in processes)
                        {
                            try
                            {
                                if (process.ProcessName.Contains("TM Client") ||
                                    (process.MainWindowTitle != null && process.MainWindowTitle.Contains("TM Client")))
                                {
                                    process.Kill();
                                    process.WaitForExit(5000); // Attende fino a 5 secondi che il processo termini
                                    Logger.Loggin($"Processo terminato: {process.ProcessName}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Loggin($"Errore nel terminare il processo: {ex.Message}");
                            }
                        }
                        await Task.Delay(delay);
                        // Avvia TM Updater.exe
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = tmUpdaterPath,
                            WorkingDirectory = tmClientDir,
                            UseShellExecute = true
                        };

                        Process.Start(startInfo);
                        Logger.Loggin($"Avviato: {tmUpdaterPath}");
                        return true;
                    }
                    else
                    {
                        Logger.Loggin($"ClassicUO.exe non trovato in: {tmClientDir}",true);
                    }
                }
                else
                {
                    Logger.Loggin("Non è stato possibile trovare la directory principale di The Miracle");
                }
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore durante il riavvio del client: {ex.Message}");
            }

            return false;
        }

      

    }
}
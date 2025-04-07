using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;
using UltimaOnlineMacro.Service;

namespace UltimaOnlineMacro
{
    public partial class OverlayWindow : Window
    {
        private Point selectionStart;
        private Point selectionEnd;
        private bool isSelecting = false;
        Rect SelectionRect { get; set; }
        private string suffix { get; set; }
        public Logger LogManager { get; }

        public OverlayWindow(string suffix, Logger logManager)
        {
            LogManager = logManager;
            InitializeComponent();

            // Finestra senza bordi e trasparente
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            this.Topmost = false;  // La finestra sarà sempre in primo piano

            // Imposta la finestra per coprire tutto lo schermo
            this.WindowState = WindowState.Normal;
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
            this.ResizeMode = ResizeMode.NoResize;

            // Aggiungi un cursore a croce per migliorare l'esperienza utente durante la selezione
            this.Cursor = Cursors.Cross;

            // Aggiungi la possibilità di annullare la selezione con Escape
            this.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    this.Close();
                }
            };

            // IMPORTANTE: Gestisci gli eventi nell'ordine corretto
            // Prima registra gli handler delle azioni, poi quelli per bloccare
            this.MouseLeftButtonDown += OverlayWindow_MouseLeftButtonDown;
            this.MouseMove += OverlayWindow_MouseMove;
            this.MouseLeftButtonUp += OverlayWindow_MouseLeftButtonUp;
            this.suffix = suffix;
            this.LogManager = LogManager;

            // IMPORTANTE: Non bloccare completamente gli eventi ma permettili per questa window
            // Evita di usare PreviewMouseDown/Up/Move qui in quanto potrebbero interferire
        }

        private void OverlayWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Inizia la selezione
            isSelecting = true;
            selectionStart = e.GetPosition(this);
            selectionEnd = selectionStart; // Inizializza anche selectionEnd per evitare errori
            this.CaptureMouse(); // IMPORTANTE: cattura il mouse per ricevere tutti gli eventi anche fuori dalla finestra
            this.InvalidateVisual(); // Aggiorna la visualizzazione

            LogManager.Loggin($"Inizio selezione {suffix}: " + selectionStart.ToString());
            e.Handled = true;
        }

        private void OverlayWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                selectionEnd = e.GetPosition(this);
                this.InvalidateVisual();  // Ridisegna la finestra con la selezione aggiornata
            }
        }

        private void OverlayWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isSelecting)
            {
                // Rilascia la cattura del mouse
                this.ReleaseMouseCapture();

                // Ottieni la posizione finale
                selectionEnd = e.GetPosition(this);

                // La selezione è terminata, passiamo la selezione alla finestra principale
                Rectangle selectedRegion = new Rectangle(
                    (int)Math.Min(selectionStart.X, selectionEnd.X),
                    (int)Math.Min(selectionStart.Y, selectionEnd.Y),
                    (int)Math.Abs(selectionStart.X - selectionEnd.X),
                    (int)Math.Abs(selectionStart.Y - selectionEnd.Y)
                );

                // IMPORTANTE: imposta isSelecting a false PRIMA di passare il controllo altrove
                isSelecting = false;

                // Passa la selezione alla finestra principale
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Dispatcher.Invoke(() => {
                        this.Close();
                        switch(suffix)
                        {
                            case "Piccone":
                                mainWindow.SetBackpackRegion(selectedRegion);
                                break;
                            case "Paperdoll":
                                mainWindow.PaperdollHavePickaxe(selectedRegion);
                                break;
                            case "Mulo":
                                mainWindow.SetBackpackMuloRegion(selectedRegion);
                                break;
                        }
                    });
                }
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // Crea un overlay semitrasparente su tutta la finestra
            drawingContext.DrawRectangle(
                new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),  // Overlay scuro semitrasparente
                null,  // Nessun bordo
                new Rect(0, 0, this.ActualWidth, this.ActualHeight)
            );

            if (isSelecting)
            {
                // Calcola il rettangolo di selezione
                SelectionRect = new Rect(
                    Math.Min(selectionStart.X, selectionEnd.X),
                    Math.Min(selectionStart.Y, selectionEnd.Y),
                    Math.Abs(selectionStart.X - selectionEnd.X),
                    Math.Abs(selectionStart.Y - selectionEnd.Y)
                );


                // Disegna un bordo rosso più visibile
                drawingContext.DrawRectangle(
                    null,  // Nessun riempimento
                    new Pen(Brushes.Red, 3),  // Bordo rosso più spesso
                    SelectionRect
                );

                // Mostra le dimensioni del rettangolo selezionato
                FormattedText text = new FormattedText(
                    $"{(int)SelectionRect.Width} x {(int)SelectionRect.Height}",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    14,
                    Brushes.White,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                // Posiziona il testo vicino al puntatore del mouse
                Point textPosition = new Point(
                    selectionEnd.X + 10,
                    selectionEnd.Y + 10
                );

                // Se il testo va fuori dallo schermo, riposizionalo
                if (textPosition.X + text.Width > this.ActualWidth)
                    textPosition.X = selectionEnd.X - text.Width - 10;
                if (textPosition.Y + text.Height > this.ActualHeight)
                    textPosition.Y = selectionEnd.Y - text.Height - 10;

                // Aggiungi uno sfondo al testo per migliorare la leggibilità
                drawingContext.DrawRectangle(
                    new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                    null,
                    new Rect(textPosition.X - 3, textPosition.Y - 3, text.Width + 6, text.Height + 6)
                );

                // Disegna il testo
                drawingContext.DrawText(text, textPosition);
            }
        }
    }
}
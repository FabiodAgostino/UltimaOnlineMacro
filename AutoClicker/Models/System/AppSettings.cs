namespace AutoClicker.Models.System
{
    namespace UltimaOnlineMacro.Models.System
    {
        public class AppSettings
        {
            // Regioni
            public RectangleSettings BackpackRegion { get; set; }
            public RectangleSettings PaperdollRegion { get; set; }
            public RectangleSettings StatusRegion { get; set; }
            public RectangleSettings PaperdollPickaxeRegion { get; set; }

            // Impostazioni macro
            public string SelectedKey { get; set; }
            public bool CtrlModifier { get; set; }
            public bool AltModifier { get; set; }
            public bool ShiftModifier { get; set; }
            public double DelayValue { get; set; }

            // File selezionato
            public string JournalPath { get; set; }
            public string MacroPath { get; set; }


            // Animali selezionati
            public bool MuloDaSomaSelected { get; set; }
            public bool LamaPortatoreSelected { get; set; }

            // Posizioni cibo e acqua
            public PointSettings FoodPosition { get; set; }
            public PointSettings WaterPosition { get; set; }
            public string PgName { get; set; }
            public bool HaveBaseFuria { get; set; }
            public bool FuriaChecked { get; set; }
        }

        // Classe per salvare Rectangle
        public class RectangleSettings
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public RectangleSettings() { }

            public RectangleSettings(Rectangle rect)
            {
                X = rect.X;
                Y = rect.Y;
                Width = rect.Width;
                Height = rect.Height;
            }

            public Rectangle ToRectangle()
            {
                return new Rectangle(X, Y, Width, Height);
            }
        }

        // Classe per salvare Point
        public class PointSettings
        {
            public int X { get; set; }
            public int Y { get; set; }

            public PointSettings() { }

            public PointSettings(AutoClicker.Utils.User32DLL.POINT point)
            {
                X = point.X;
                Y = point.Y;
            }

            public AutoClicker.Utils.User32DLL.POINT ToPoint()
            {
                return new AutoClicker.Utils.User32DLL.POINT { X = X, Y = Y };
            }
        }
    }
}

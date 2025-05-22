using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace UltimaOnlineMacro.Service
{
    public class MaterialToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string material = value as string;

            if (material == null)
                return new SolidColorBrush(Color.FromRgb(62, 62, 66)); // Colore predefinito

            switch (material.ToLower())
            {
                case "bronzo grezzo":
                case "lingotti di bronzo":
                    return new SolidColorBrush(Color.FromRgb(205, 127, 50)); // Colore bronzo

                case "ferro grezzo":
                case "lingotti di ferro":
                    return new SolidColorBrush(Color.FromRgb(135, 135, 135)); // Colore ferro

                case "rame grezzo":
                case "lingotti di rame":
                    return new SolidColorBrush(Color.FromRgb(184, 115, 51)); // Colore rame

                case "lingotti di argento":
                    return new SolidColorBrush(Color.FromRgb(192, 192, 192)); // Colore argento

                case "lingotti di oro":
                case "oro grezzo":
                    return new SolidColorBrush(Color.FromRgb(255, 215, 0)); // Colore oro

                default:
                    return new SolidColorBrush(Color.FromRgb(62, 62, 66)); // Colore predefinito
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public class WidthToLayoutConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is double width)
                {
                    // Se la larghezza è minore di 500px, usa layout verticale
                    return width < 500;
                }
                return false;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AgroChem.Laboratory.Converters
{
    public class ResultToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = value?.ToString() ?? "";
            if (val.Contains("pass")) return new SolidColorBrush(Colors.Green);
            if (val.Contains("fail")) return new SolidColorBrush(Colors.Red);
            if (val.Contains("✅")) return new SolidColorBrush(Colors.Green);
            if (val.Contains("❌")) return new SolidColorBrush(Colors.Red);
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
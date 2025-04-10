using System.Globalization;
using System.Windows.Data;

namespace WebView2Traffic.Converters
{
    public class NonEmptyStringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = (string)value;
            return !string.IsNullOrEmpty(text);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

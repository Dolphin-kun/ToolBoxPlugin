using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ToolBox.Commons
{
    public class StringToGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string pathData && !string.IsNullOrWhiteSpace(pathData))
            {
                try
                {
                    return Geometry.Parse(pathData);
                }
                catch { }
            }
            return Geometry.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

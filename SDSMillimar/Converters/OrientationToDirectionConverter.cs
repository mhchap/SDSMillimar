using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace SDSMillimar.Converters
{
    public class OrientationToDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Orientation orientation)
            {
                // 垂直方向时，默认滚动条需要反向
                return orientation == Orientation.Vertical;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

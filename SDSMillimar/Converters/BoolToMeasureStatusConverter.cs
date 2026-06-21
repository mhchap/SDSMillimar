using System;
using System.Globalization;
using System.Windows.Data;

namespace SDSMillimar.Converters
{
    internal class BoolToMeasureStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? "已配置测量项目" : "未配置测量项目";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
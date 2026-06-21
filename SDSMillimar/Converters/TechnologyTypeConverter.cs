using System;
using System.Globalization;
using System.Windows.Data;

namespace SDSMillimar.Converters
{
    public class TechnologyTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            int type;
            if (!int.TryParse(value.ToString(), out type))
                return value.ToString();

            switch (type)
            {
                case 1: return "测量";
                case 2: return "校准";
                default: return "未知";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 一般 DataGrid 不需要反向转换
            throw new NotImplementedException();
        }
    }
}

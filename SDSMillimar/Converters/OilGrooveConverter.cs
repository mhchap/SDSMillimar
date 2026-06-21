using System;
using System.Globalization;
using System.Windows.Data;

namespace SDSMillimar.Converters
{
    public class OilGrooveConverter : IValueConverter
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
                case 0: return "不开启";
                case 1: return "开启";
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

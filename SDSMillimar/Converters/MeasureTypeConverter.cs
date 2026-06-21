using System;
using System.Globalization;
using System.Windows.Data;

namespace SDSMillimar.Converters
{
    public class MeasureTypeConverter : IValueConverter
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
                case 1: return "直径";
                case 2: return "圆度";
                case 3: return "圆柱度";
                case 4: return "跳动";
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

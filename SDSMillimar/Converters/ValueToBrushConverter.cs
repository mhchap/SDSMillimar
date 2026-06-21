using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace SDSMillimar.Converters
{
    internal class ValueToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double measured = ToDouble(values[0]);
            double upper = ToDouble(values[1]);
            double lower = ToDouble(values[2]);
            if (measured == 0)
            {
                return Brushes.YellowGreen;
            }

            if (measured > upper || measured < lower)
                return Brushes.Red;

            return Brushes.Green;
        }

        private double ToDouble(object value)
        {
            if (value == null) return 0;

            try
            {
                return System.Convert.ToDouble(value);
            }
            catch
            {
                return 0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

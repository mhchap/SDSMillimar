using System;
using System.Globalization;
using System.Windows.Data;

namespace SDSMillimar.Converters
{
    public class OffsetToWidthConverter : IValueConverter
    {
        public double MaxValue { get; set; } = 2.1;
        public double MaxWidth { get; set; } = 100;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0d;

            if (!double.TryParse(value.ToString(), out double measured))
                return 0d;

            string direction = parameter?.ToString(); // "Left" or "Right"

            if (direction == "Left" && measured < 0)
            {
                return Math.Abs(measured) / MaxValue * MaxWidth;
            }

            if (direction == "Right" && measured > 0)
            {
                return measured / MaxValue * MaxWidth;
            }

            return 0d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}

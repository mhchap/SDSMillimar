using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SDSMillimar.Converters
{
    public class TestItemOffsetToWidthConverter : IMultiValueConverter
    {
        // 👉 UI总宽度（例如整条200）
        public double TotalWidth { get; set; } = 200;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 4) return 0;

            double measured = ToDouble(values[0]);
            double upperInput = ToDouble(values[1]);
            double lowerInput = ToDouble(values[2]);
            double typeInput = System.Convert.ToInt32(values[3]);
            double cmInput = ToDouble(values[4]);
            double cuInput = ToDouble(values[5]);
            double clInput = ToDouble(values[6]);
            bool isMeasured = values[7] is bool b && b;

            string direction = parameter?.ToString();

            if (!isMeasured) return 0;

            // 👉 半宽（关键）
            double halfWidth = TotalWidth / 2.0;

            // 👉 防止上下公差传反
            double lower = Math.Min(lowerInput, upperInput);
            double upper = Math.Max(lowerInput, upperInput);

            if (upper == lower) return 0;

            // =========================
            // ✅ 情况1：下公差 = 0（单边：左→右）
            // =========================
            //if (Math.Abs(lower) < 1e-10)
            if (typeInput != 1)
            {
                double ratio = measured / upper;

                ratio = Math.Max(0, Math.Min(1, ratio));

                if (direction == "Left")
                {
                    return ratio * halfWidth;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                double center = (cuInput + clInput) / 2.0;
                double halfRange = (cuInput - clInput) / 2.0;

                if (halfRange == 0) return 0;

                double ratioCenter = (cmInput - center) / halfRange;

                ratioCenter = Math.Max(-1, Math.Min(1, ratioCenter));

                if (direction == "Left")
                {
                    return ratioCenter > 0 ? ratioCenter * halfWidth : 0;
                }
                else if (direction == "Right")
                {
                    return ratioCenter < 0 ? -ratioCenter * halfWidth : 0;
                }
            }

            // =========================
            // ✅ 情况2：上公差 = 0（单边：右→左）
            // =========================
            //if (Math.Abs(upper) < 1e-10)
            //{
            //    // ✅ 改成基于“距离0”的比例
            //    double ratio = (0 - measured) / (0 - lower);

            //    ratio = Math.Max(0, Math.Min(1, ratio));

            //    if (direction == "Left")
            //    {
            //        return ratio * halfWidth;
            //    }
            //    else
            //    {
            //        return 0;
            //    }
            //}

            //// =========================
            //// ✅ 情况3：双边公差（以中心展开）
            //// 0.025 0.009
            //// =========================
            //double center = (upper + lower) / 2.0;
            //double halfRange = (upper - lower) / 2.0;

            //if (halfRange == 0) return 0;

            //double ratioCenter = (measured - center) / halfRange;

            //ratioCenter = Math.Max(-1, Math.Min(1, ratioCenter));

            //if (direction == "Right")
            //{
            //    return ratioCenter > 0 ? ratioCenter * halfWidth : 0;
            //}
            //else if (direction == "Left")
            //{
            //    return ratioCenter < 0 ? -ratioCenter * halfWidth : 0;
            //}

            return 0;
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

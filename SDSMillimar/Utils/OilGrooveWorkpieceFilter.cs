using SDSMillimar.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDSMillimar.Utils
{
    /// <summary>
    /// 带油槽工件的测量数据过滤。
    /// 
    /// 普通中值滤波更适合处理孤立尖峰；油槽是连续低谷，直接参与最大/最小值会导致
    /// 直径、圆度、跳动偏差。因此这里按“低于正常表面的一段连续区域”识别油槽。
    /// </summary>
    internal class OilGrooveWorkpieceFilter
    {
        private const double DefaultDepthThreshold = 0.01;
        private const double DefaultRecoverRatio = 0.5;

        /// <summary>
        /// 识别并删除油槽段。适合直径、跳动这类只依赖有效最大/最小值的算法。
        /// </summary>
        public double[] RemoveGrooveSegments(string paramValue, IEnumerable<double> values)
        {
            var data = values?.ToArray() ?? new double[0];
            if (data.Length < 3)
                return data;

            var mask = BuildGrooveMask(data);
            var filtered = data
                .Where((_, index) => !mask[index])
                .ToArray();

            LogFilterResult(paramValue, data, filtered, mask, "剔除");

            // 防止配置或数据异常时把整圈数据删空。
            if (filtered.Length == 0)
            {
                AppLog.FilterData.Warn($"{paramValue}->油槽过滤后无有效数据，回退使用原始数据");
                return data;
            }

            return filtered;
        }

        /// <summary>
        /// 识别油槽段并用两侧正常面线性桥接。适合圆度这类需要保持采样点数量的算法。
        /// </summary>
        public double[] ReplaceGrooveSegments(string paramValue, IEnumerable<double> values)
        {
            var data = values?.ToArray() ?? new double[0];
            if (data.Length < 3)
                return data;

            var mask = BuildGrooveMask(data);
            if (!mask.Any(x => x))
                return data;

            var replaced = data.ToArray();
            int n = replaced.Length;
            int i = 0;

            while (i < n)
            {
                if (!mask[i])
                {
                    i++;
                    continue;
                }

                int start = i;
                while (i < n && mask[i])
                    i++;
                int end = i - 1;

                int leftIndex = start - 1;
                int rightIndex = end + 1;

                double leftValue = leftIndex >= 0 ? replaced[leftIndex] : replaced[Math.Min(rightIndex, n - 1)];
                double rightValue = rightIndex < n ? replaced[rightIndex] : leftValue;

                int length = end - start + 1;
                for (int j = 0; j < length; j++)
                {
                    double ratio = (j + 1.0) / (length + 1.0);
                    replaced[start + j] = leftValue + (rightValue - leftValue) * ratio;
                }
            }

            LogFilterResult(paramValue, data, replaced, mask, "替换");
            return replaced;
        }

        private bool[] BuildGrooveMask(double[] data)
        {
            int n = data.Length;
            var mask = new bool[n];

            var sorted = data.OrderBy(x => x).ToArray();
            double median = sorted[n / 2];
            double mad = Median(data.Select(x => Math.Abs(x - median)).ToArray());
            double depthThreshold = Math.Max(DefaultDepthThreshold, mad * 6.0);
            double coreThreshold = median - depthThreshold;
            double recoverThreshold = median - depthThreshold * DefaultRecoverRatio;

            int i = 0;
            while (i < n)
            {
                if (data[i] > coreThreshold)
                {
                    i++;
                    continue;
                }

                int start = i;
                while (i < n && data[i] <= coreThreshold)
                    i++;
                int end = i - 1;

                int left = start;
                while (left > 0 && data[left - 1] <= recoverThreshold)
                    left--;

                int right = end;
                while (right < n - 1 && data[right + 1] <= recoverThreshold)
                    right++;

                for (int j = left; j <= right; j++)
                    mask[j] = true;
            }

            return mask;
        }

        private double Median(double[] data)
        {
            if (data == null || data.Length == 0)
                return 0;

            var sorted = data.OrderBy(x => x).ToArray();
            int middle = sorted.Length / 2;
            if (sorted.Length % 2 == 1)
                return sorted[middle];

            return (sorted[middle - 1] + sorted[middle]) / 2.0;
        }

        private void LogFilterResult(string paramValue, double[] original, double[] result, bool[] mask, string action)
        {
            var removed = original
                .Where((_, index) => mask[index])
                .ToArray();

            AppLog.FilterData.Info($"{paramValue}->油槽过滤{action}->原始数据->{string.Join(",", original.Select(x => x.ToString("F6")))}");
            AppLog.FilterData.Info($"{paramValue}->油槽过滤{action}->油槽数据->{string.Join(",", removed.Select(x => x.ToString("F6")))}");
            AppLog.FilterData.Info($"{paramValue}->油槽过滤{action}->结果数据->{string.Join(",", result.Select(x => x.ToString("F6")))}");
        }
    }
}

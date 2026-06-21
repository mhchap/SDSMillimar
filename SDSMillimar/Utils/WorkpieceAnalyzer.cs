using DocumentFormat.OpenXml.Drawing.Charts;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;
using SDSMillimar.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDSMillimar.Utils
{
    internal class WorkpieceAnalyzer
    {
        private readonly double _samplePeriod;
        private readonly double _fs;

        public WorkpieceAnalyzer(double samplePeriod = 0.012)
        {
            _samplePeriod = samplePeriod;
            _fs = 1.0 / _samplePeriod;
        }

        /// <summary>
        /// 去除测量数据中的凹槽或异常突变点
        /// 使用中值滤波 + 3σ异常值判断
        /// </summary>
        /// <param name="data">原始测量数据</param>
        /// <param name="windowSize">滤波窗口大小（必须为奇数，默认11）</param>
        /// <returns>去除异常后的数据</returns>
        /// 中值滤波：去凹槽
        public double[] RemoveGrooves(double[] data, int windowSize = 13)
        {
            int n = data.Length;
            double[] result = new double[n];
            int radius = windowSize / 2;

            for (int i = 0; i < n; i++)
            {
                List<double> window = new List<double>();

                for (int j = -radius; j <= radius; j++)
                {
                    int idx = i + j;
                    if (idx < 0) idx += n;
                    if (idx >= n) idx -= n;

                    window.Add(data[idx]);
                }

                window.Sort();
                result[i] = window[radius];
            }

            double[] cleaned = new double[n];
            double stdDev = ArrayStatistics.StandardDeviation(
                data.Zip(result, (a, b) => a - b).ToArray());

            for (int i = 0; i < n; i++)
            {
                if (Math.Abs(data[i] - result[i]) > 3 * stdDev)
                    cleaned[i] = result[i];
                else
                    cleaned[i] = data[i];
            }

            return cleaned;
        }

        /// <summary>
        /// 低通滤波（移动平均滤波）
        /// 用于平滑数据，减少高频噪声
        /// 转速高 → span小
        /// 转速低 → span大
        /// 3	轻度平滑 5	常规 10	强平滑
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="span">平均窗口半径（默认5）</param>
        /// <returns>滤波后的数据</returns>
        public double[] LowPassFilter(double[] data, int span = 5)
        {
            double[] output = new double[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                int start = Math.Max(0, i - span);
                int end = Math.Min(data.Length - 1, i + span);

                double sum = 0;

                for (int j = start; j <= end; j++)
                    sum += data[j];

                output[i] = sum / (end - start + 1);
            }

            return output;
        }

        /// 数据预处理
        public double[] Preprocess(double[] data)
        {
            var clean = RemoveGrooves(data, GlobalSession.Instance.WindowSize);
            var filtered = LowPassFilter(clean, GlobalSession.Instance.Span);
            return filtered;
        }

        /// 计算直径
        public double CalculateDiameter(double[] dataA, double[] dataB, double baseDiameter, double compensationValue, string ParamValue)
        {
            var filteredA = Preprocess(dataA);
            var filteredB = Preprocess(dataB);
            AppLog.ProcessData.Info($"{ParamValue} 过滤后数据A -> {string.Join(",", filteredA.Select(p => p.ToString("F5")))}");
            AppLog.ProcessData.Info($"{ParamValue} 过滤后数据B -> {string.Join(",", filteredB.Select(p => p.ToString("F5")))}");
            int n = filteredA.Length;

            double[] compensated = new double[n];

            for (int i = 0; i < n; i++)
            {
                compensated[i] = (filteredA[i] + filteredB[i]) / 2.0;
            }
            double max, min;
            // 排除前N个最大值和最小值
            int excludeCount = GlobalSession.Instance.ExcludeCount;
            if (excludeCount > 0)
            {

                if (compensated.Length <= excludeCount * 2)
                {
                    AppLog.ProcessData.Info($"数据量不足，无法排除 {excludeCount} 个最大值和最小值");
                }

                var validData = compensated
                    .OrderBy(x => x)
                    .Skip(excludeCount)                    // 去掉最小N个
                    .Take(compensated.Length - excludeCount * 2) // 去掉最大N个
                    .ToArray();

                max = validData.Max();
                min = validData.Min();
            }
            else
            {
                max = compensated.Max();
                min = compensated.Min();
            }

            double diameterOffset = max + min;
            AppLog.ProcessData.Info($"{ParamValue} Max={max:F5}, Min={min:F5}, Offset={diameterOffset:F5}");

            return baseDiameter + diameterOffset + compensationValue;
        }

        /// 计算径向跳动
        public double CalculateRunout(double[] data, string ParamValue)
        {
            var filtered = Preprocess(data);

            AppLog.ProcessData.Info($"{ParamValue} 过滤后数据 -> {string.Join(",", filtered.Select(p => p.ToString("F5")))}");
            return filtered.Max() - filtered.Min();
        }

        /// 计算圆度
        public double CalculateRoundness(double[] dataA, double[] dataB, string ParamValue)
        {
            var filteredA = Preprocess(dataA);
            var filteredB = Preprocess(dataB);

            AppLog.ProcessData.Info($"{ParamValue} 过滤后数据A -> {string.Join(",", filteredA.Select(p => p.ToString("F5")))}");
            AppLog.ProcessData.Info($"{ParamValue} 过滤后数据B -> {string.Join(",", filteredB.Select(p => p.ToString("F5")))}");
            int n = filteredA.Length;

            double[] compensated = new double[n];

            for (int i = 0; i < n; i++)
                compensated[i] = (filteredA[i] + filteredB[i]) / 2.0;

            double[] theta = Generate.LinearSpaced(n, 0, 2 * Math.PI);

            double sumX = 0;
            double sumY = 0;

            for (int i = 0; i < n; i++)
            {
                sumX += compensated[i] * Math.Cos(theta[i]);
                sumY += compensated[i] * Math.Sin(theta[i]);
            }

            double xc = sumX / n;
            double yc = sumY / n;

            double[] trueShape = new double[n];

            for (int i = 0; i < n; i++)
            {
                double x = compensated[i] * Math.Cos(theta[i]);
                double y = compensated[i] * Math.Sin(theta[i]);

                trueShape[i] = Math.Sqrt(
                    Math.Pow(x - xc, 2) +
                    Math.Pow(y - yc, 2));
            }

            return trueShape.Max() - trueShape.Min();
        }
    }
}
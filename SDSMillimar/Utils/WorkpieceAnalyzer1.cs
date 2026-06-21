using MathNet.Numerics;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDSMillimar.Utils
{
    internal class WorkpieceAnalyzer1
    {
        private readonly double _samplePeriod; // 采样周期，例如 0.012s
        private readonly double _fs;           // 采样频率

        public WorkpieceAnalyzer1(double samplePeriod = 0.012)
        {
            _samplePeriod = samplePeriod;
            _fs = 1.0 / _samplePeriod;
        }

        /// <summary>
        /// 中值滤波：剔除表面凹槽点
        /// </summary>
        public double[] RemoveGrooves(double[] data, int windowSize = 11)
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
                    // 边界处理：循环卷积模式（适合旋转工件）
                    if (idx < 0) idx += n;
                    if (idx >= n) idx -= n;
                    window.Add(data[idx]);
                }
                window.Sort();
                result[i] = window[radius]; // 取中值
            }

            // 阈值修补：如果原始点偏离中值太大（掉入深坑），则用中值替换
            double[] cleaned = new double[n];
            double stdDev = ArrayStatistics.StandardDeviation(data.Zip(result, (a, b) => a - b).ToArray());

            for (int i = 0; i < n; i++)
            {
                if (Math.Abs(data[i] - result[i]) > 3.0 * stdDev)
                    cleaned[i] = result[i];
                else
                    cleaned[i] = data[i];
            }
            return cleaned;
        }

        /// <summary>
        /// 简易低通滤波（滑动平均或一阶RC，此处示例为移动平均以简化逻辑）
        /// 注：高性能场景建议使用 MathNet 的 Windowing 或引入 Butterworth 递归公式
        /// </summary>
        public double[] LowPassFilter(double[] data, int span = 5)
        {
            double[] output = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                int start = Math.Max(0, i - span);
                int end = Math.Min(data.Length - 1, i + span);
                double sum = 0;
                for (int j = start; j <= end; j++) sum += data[j];
                output[i] = sum / (end - start + 1);
            }
            return output;
        }

        /// <summary>
        /// 核心处理逻辑 , double tolRoundness, double tolRunout
        /// </summary>
        public MeasurementResult Process(double[] dataA, double[] dataB, double baseDiameter)
        {
            int n = dataA.Length;

            // 1. 独立剔除凹槽
            double[] cleanA = RemoveGrooves(dataA);
            double[] cleanB = RemoveGrooves(dataB);

            // 2. 低通平滑
            double[] filteredA = LowPassFilter(cleanA);
            double[] filteredB = LowPassFilter(cleanB);

            // 3. 计算径向跳动 (单探头滤波后峰峰值)
            double actualRunout = filteredA.Max() - filteredA.Min();

            // 4. 180度差分补偿 (A+B)/2 消除安装偏心
            double[] compensated = new double[n];
            for (int i = 0; i < n; i++)
            {
                compensated[i] = (filteredA[i] + filteredB[i]) / 2.0;
            }

            // 5. 最小二乘圆拟合 (简化版：计算重心平移)
            double avgR = compensated.Average();
            double[] theta = Generate.LinearSpaced(n, 0, 2 * Math.PI);

            double sumX = 0, sumY = 0;
            for (int i = 0; i < n; i++)
            {
                sumX += compensated[i] * Math.Cos(theta[i]);
                sumY += compensated[i] * Math.Sin(theta[i]);
            }
            double xc = sumX / n;
            double yc = sumY / n;

            // 计算真实形状 (到拟合中心的距离)
            double[] trueShape = new double[n];
            for (int i = 0; i < n; i++)
            {
                double x = compensated[i] * Math.Cos(theta[i]);
                double y = compensated[i] * Math.Sin(theta[i]);
                trueShape[i] = Math.Sqrt(Math.Pow(x - xc, 2) + Math.Pow(y - yc, 2));
            }

            double actualRoundness = trueShape.Max() - trueShape.Min();
            // 最大最小位移
            double maxDisp = compensated.Max();
            double minDisp = compensated.Min();

            // 直径
            double diameter = maxDisp + minDisp;
            return new MeasurementResult
            {
                Roundness = actualRoundness,
                Runout = actualRunout,
                MaxDisplacement = maxDisp,
                MinDisplacement = minDisp,
                Diameter = baseDiameter + diameter,
                BaseDiameter = baseDiameter,
                //IsQualified = (actualRoundness <= tolRoundness && actualRunout <= tolRunout),
                Eccentricity = Math.Sqrt(xc * xc + yc * yc)
            };
        }
    }
    public struct MeasurementResult
    {
        public double Roundness;
        public double Runout;
        public double MaxDisplacement;   // 最大位移
        public double MinDisplacement;   // 最小位移
        public double Diameter;          // 计算直径
        public double BaseDiameter;
        public double Eccentricity;
        public bool IsQualified;
        public override string ToString()
        {
            return $"Roundness={Roundness:F4}, " +
                   $"Runout={Runout:F4}, " +
                   $"Eccentricity={Eccentricity:F4}, " +
                   $"MaxDisp={MaxDisplacement:F4}, " +
                   $"MinDisp={MinDisplacement:F4}, " +
                   $"Diameter={Diameter:F4}, " +
                   $"Qualified={IsQualified}";
        }
    }
}

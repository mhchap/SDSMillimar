using SDSMillimar.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDSMillimar.Utils
{
    /// <summary>
    /// 2 探头（180°对称）测量算法
    /// </summary>
    public static class TwoProbeMeasurementAlgorithms
    {
        #region 外圆直径（一个数）

        /// <summary>
        /// 外圆直径
        /// = 基础直径 + 一圈位移平均值
        /// </summary>
        public static double CalcDiameter(
            SampleFrame frame,
            double baseDiameter)
        {
            CheckFrame(frame);

            int count = frame.APoints.Count;
            double sum = 0.0;

            for (int i = 0; i < count; i++)
            {
                sum += frame.APoints[i].Value
                     + frame.BPoints[i].Value;
            }

            double deltaMean = sum / (2.0 * count);

            return baseDiameter + deltaMean;
        }

        public static double CalcDiameterJC(
    SampleFrame frame,
    double baseDiameter)
        {
            CheckFrame(frame);

            int count = frame.APoints.Count;

            double min = double.MaxValue;
            double max = double.MinValue;

            for (int i = 0; i < count; i++)
            {
                double d = frame.APoints[i].Value
                         + frame.BPoints[i].Value;

                if (d < min) min = d;
                if (d > max) max = d;
            }

            double range = max - min;

            return baseDiameter + range;
        }


        #endregion


        /// <summary>
        /// 圆度（极差法）
        /// E(θ)= (A(θ)−B(θ))/2 Roundness=max⁡(E)−min⁡(E)
        /// </summary>
        public static double CalcRoundness(SampleFrame frame)
        {
            CheckFrame(frame);

            double max = double.MinValue;
            double min = double.MaxValue;

            for (int i = 0; i < frame.APoints.Count; i++)
            {
                double r = (frame.APoints[i].Value
                          + frame.BPoints[i].Value) / 2.0;

                if (r > max) max = r;
                if (r < min) min = r;
            }

            return max - min;
        }

        /// <summary>
        /// 外圆跳动（极差）
        /// </summary>
        public static double CalcRunout(SampleFrame frame)
        {
            CheckFrame(frame);

            double max = double.MinValue;
            double min = double.MaxValue;

            for (int i = 0; i < frame.APoints.Count; i++)
            {
                double r = (frame.APoints[i].Value
                          + frame.BPoints[i].Value) / 2.0;

                if (r > max) max = r;
                if (r < min) min = r;
            }

            return max - min;
        }

        /// <summary>
        /// 外圆跳动（多截面极差法）
        /// </summary>
        public static double CalcRunout(List<SampleFrame> frames)
        {
            if (frames == null || frames.Count == 0)
                throw new ArgumentException("没有截面数据");

            double globalMax = double.MinValue;
            double globalMin = double.MaxValue;

            foreach (var frame in frames)
            {
                CheckFrame(frame);

                for (int i = 0; i < frame.APoints.Count; i++)
                {
                    double r = (frame.APoints[i].Value + frame.BPoints[i].Value) / 2.0;

                    if (r > globalMax) globalMax = r;
                    if (r < globalMin) globalMin = r;
                }
            }

            return globalMax - globalMin;
        }



        /// <summary>
        /// 圆柱度
        /// </summary>
        public static double CalcCylindricity(List<SampleFrame> frames)
        {
            if (frames == null || frames.Count == 0)
                throw new ArgumentException("没有截面数据");

            double globalMax = double.MinValue;
            double globalMin = double.MaxValue;

            foreach (var frame in frames)
            {
                CheckFrame(frame);

                for (int i = 0; i < frame.APoints.Count; i++)
                {
                    double r = (frame.APoints[i].Value
                              - frame.BPoints[i].Value) / 2.0;  // ←← 关键是减号

                    if (r > globalMax) globalMax = r;
                    if (r < globalMin) globalMin = r;
                }
            }

            return globalMax - globalMin;
        }

        /// <summary>
        /// 极差法,目前sds跳动只用一组数据、所以跳动和圆度算法完全一致
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static double CalcRoundness1(SampleFrame frame)
        {
            CheckFrame(frame);

            double max = double.MinValue;
            double min = double.MaxValue;

            for (int i = 0; i < frame.APoints.Count; i++)
            {
                double r = (frame.APoints[i].Value + frame.BPoints[i].Value) / 2.0;

                if (r > max) max = r;
                if (r < min) min = r;
            }

            return max - min;
        }


        #region 校验

        private static void CheckFrame(SampleFrame frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            if (frame.APoints.Count == 0 ||
                frame.BPoints.Count == 0)
                throw new ArgumentException("采样点为空");

            if (frame.APoints.Count != frame.BPoints.Count)
                throw new ArgumentException("A/B 点数量不一致");
        }

        #endregion

        #region 辅助方法
        /// <summary>
        /// 计算中位数（用于鲁棒统计，抗异常点）
        /// </summary>
        static double Median(List<double> data)
        {
            if (data == null || data.Count == 0)
                return 0;

            var sorted = data.OrderBy(x => x).ToList();
            int n = sorted.Count;

            // 奇数个取中间，偶数个取中间两点平均
            return n % 2 == 1
                ? sorted[n / 2]
                : (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
        }

        /// <summary>
        /// 计算标准差（用于判断整体波动水平）
        /// </summary>
        static double StdDev(List<double> data)
        {
            if (data == null || data.Count == 0)
                return 0;

            double mean = data.Average();
            double sum = 0;

            foreach (var v in data)
                sum += (v - mean) * (v - mean);

            return Math.Sqrt(sum / data.Count);
        }

        /// <summary>
        /// 根据相邻点差分，自动计算“不可能的跳变阈值”
        /// </summary>
        static double AutoJumpThreshold(List<double> rList)
        {
            var diffs = new List<double>();

            // 相邻点位移差
            for (int i = 1; i < rList.Count; i++)
            {
                diffs.Add(Math.Abs(rList[i] - rList[i - 1]));
            }

            // 使用中位数而不是均值，避免异常点污染
            double median = Median(diffs);

            // 6 倍中位数：工程上常用，基本可认为是“物理不可能跳变”
            // 同时给一个下限，防止阈值过小
            return Math.Max(6 * median, 0.003);
        }

        /// <summary>
        /// 计算工件直径
        /// 直径 = 基础直径 + max(A) + min(B)
        /// 自动剔除异常点
        /// </summary>
        public static double CalcDiameter_Auto(SampleFrame frame, double baseDiameter)
        {
            CheckFrame(frame);

            var aList = new List<double>();
            var bList = new List<double>();

            int count = frame.APoints.Count;
            var centerList = new List<double>();

            for (int i = 0; i < count; i++)
            {
                double a = frame.APoints[i].Value;
                double b = frame.BPoints[i].Value;
                aList.Add(a);
                bList.Add(b);
                centerList.Add((a + b) / 2.0);
            }

            // 异常点剔除
            double jumpThreshold = AutoJumpThreshold(centerList);
            var filteredA = new List<double>();
            var filteredB = new List<double>();

            filteredA.Add(aList[0]);
            filteredB.Add(bList[0]);

            for (int i = 1; i < count; i++)
            {
                if (Math.Abs(centerList[i] - centerList[i - 1]) <= jumpThreshold)
                {
                    filteredA.Add(aList[i]);
                    filteredB.Add(bList[i]);
                }
            }

            if (filteredA.Count < count * 0.8)
                throw new InvalidOperationException("异常点过多，无法计算直径");

            double maxA = filteredA.Max();
            double minB = filteredB.Min();

            return baseDiameter + maxA + minB;
        }

        /// <summary>
        /// 计算跳动（极差法）
        /// 跳动 = max((A+B)/2) - min((A+B)/2)
        /// 自动剔除异常点
        /// </summary>
        public static double CalcRunout_Auto(SampleFrame frame)
        {
            CheckFrame(frame);

            int count = frame.APoints.Count;
            var centerList = new List<double>();

            for (int i = 0; i < count; i++)
                centerList.Add((frame.APoints[i].Value + frame.BPoints[i].Value) / 2.0);

            // 异常点剔除
            double jumpThreshold = AutoJumpThreshold(centerList);
            var filtered = new List<double> { centerList[0] };

            for (int i = 1; i < count; i++)
            {
                if (Math.Abs(centerList[i] - centerList[i - 1]) <= jumpThreshold)
                    filtered.Add(centerList[i]);
            }

            if (filtered.Count < count * 0.8)
                throw new InvalidOperationException("异常点过多，跳动计算无效");

            double max = filtered.Max();
            double min = filtered.Min();

            return max - min;
        }

        /// <summary>
        /// 基于双探头数据的自动圆度计算（极差法）
        /// 具备：
        /// 1. 异常点剔除
        /// 2. 倾斜自动判定
        /// 3. 去一阶趋势（去倾斜）
        /// </summary>
        public static double CalcRoundness_Auto(SampleFrame frame)
        {
            CheckFrame(frame);

            int n = frame.APoints.Count;
            if (n < 20)
                throw new InvalidOperationException("采样点数量不足，无法计算圆度");

            // =========================
            // ① 计算 r = (A + B) / 2
            //    表示工件轴线相对探头的径向位移
            // =========================
            var rList = new List<double>(n);
            for (int i = 0; i < n; i++)
            {
                double r = (frame.APoints[i].Value + frame.BPoints[i].Value) / 2.0;
                rList.Add(r);
            }

            // =========================
            // ② 自动计算异常跳变阈值
            // =========================
            double jumpThreshold = AutoJumpThreshold(rList);

            // =========================
            // ③ 剔除异常点（探头失去接触 / 数据错位）
            // =========================
            var filtered = new List<double>();
            filtered.Add(rList[0]);

            for (int i = 1; i < rList.Count; i++)
            {
                if (Math.Abs(rList[i] - rList[i - 1]) <= jumpThreshold)
                {
                    filtered.Add(rList[i]);
                }
            }

            // 如果异常点太多，说明测量过程本身有问题
            if (filtered.Count < n * 0.8)
                throw new InvalidOperationException("异常点过多，测量结果无效");

            // =========================
            // ④ 倾斜判定（非常关键）
            //    A/B 探头平均值长期偏置 → 工件倾斜
            // =========================
            double meanA = frame.APoints.Average(p => p.Value);
            double meanB = frame.BPoints.Average(p => p.Value);

            // 倾斜指标（几何意义明确）
            double tiltIndex = Math.Abs(meanA - meanB) / 2.0;

            // 倾斜阈值：使用本圈数据自身的 3σ
            double tiltThreshold = 3 * StdDev(rList);

            if (tiltIndex > tiltThreshold)
                throw new InvalidOperationException("工件存在明显倾斜，圆度计算无效");

            // =========================
            // ⑤ 去一阶趋势（最小二乘直线）
            //    去除由倾斜 / 偏心引起的慢变化
            // =========================
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

            for (int i = 0; i < filtered.Count; i++)
            {
                double x = i;              // 角度序号
                double y = filtered[i];    // 位移

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            double k = (filtered.Count * sumXY - sumX * sumY)
                     / (filtered.Count * sumX2 - sumX * sumX);
            double b = (sumY - k * sumX) / filtered.Count;

            // =========================
            // ⑥ 去趋势后的极差 = 圆度
            // =========================
            double max = double.MinValue;
            double min = double.MaxValue;

            for (int i = 0; i < filtered.Count; i++)
            {
                double corrected = filtered[i] - (k * i + b);

                if (corrected > max) max = corrected;
                if (corrected < min) min = corrected;
            }

            return max - min;
        }

        #endregion
    }
}

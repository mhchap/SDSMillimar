using SDSMillimar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDSMillimar.Utils
{
    /// <summary>
    /// 2 探头测量算法（基于 SampleFrame）
    /// </summary>
    public static class MeasurementAlgorithm
    {
        #region 基础工具

        /// <summary>
        /// 按 PointIndex 合并 A/B 探头
        /// </summary>
        private static List<(int Index, double A, double B)> MergePoints(SampleFrame frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            var aDict = frame.APoints.ToDictionary(p => p.PointIndex, p => p.Value);
            var bDict = frame.BPoints.ToDictionary(p => p.PointIndex, p => p.Value);

            var indexes = aDict.Keys.Intersect(bDict.Keys).OrderBy(i => i);

            var result = new List<(int, double, double)>();
            foreach (var i in indexes)
            {
                result.Add((i, aDict[i], bDict[i]));
            }

            return result;
        }

        #endregion

        // =========================================================
        // 外圆直径
        // =========================================================

        /// <summary>
        /// 外圆直径（逐点，不平均）
        /// D = A + B
        /// </summary>
        public static Dictionary<int, double> CalcDiameterByPoint(SampleFrame frame)
        {
            var points = MergePoints(frame);
            return points.ToDictionary(
                p => p.Index,
                p => p.A + p.B);
        }

        // =========================================================
        // 圆度
        // =========================================================

        /// <summary>
        /// 圆度（极差法，不平均）
        /// R = (A + B) / 2
        /// </summary>
        public static double CalcRoundness(SampleFrame frame)
        {
            var points = MergePoints(frame);

            if (points.Count == 0)
                return 0;

            double rMax = double.MinValue;
            double rMin = double.MaxValue;

            foreach (var p in points)
            {
                double r = (p.A + p.B) / 2.0;

                if (r > rMax) rMax = r;
                if (r < rMin) rMin = r;
            }

            return rMax - rMin;
        }

        // =========================================================
        // 外圆跳动
        // =========================================================

        /// <summary>
        /// 外圆径向跳动（极差法）
        /// 基于等效半径
        /// </summary>
        public static double CalcRunout(SampleFrame frame)
        {
            // 跳动算法与圆度在数学上相同
            return CalcRoundness(frame);
        }

        // =========================================================
        // 圆柱度
        // =========================================================

        /// <summary>
        /// 圆柱度（多截面包络，不平均）
        /// </summary>
        public static double CalcCylindricity(List<SampleFrame> frames)
        {
            if (frames == null || frames.Count == 0)
                return 0;

            double globalMax = double.MinValue;
            double globalMin = double.MaxValue;

            foreach (var frame in frames)
            {
                var points = MergePoints(frame);

                foreach (var p in points)
                {
                    double r = (p.A + p.B) / 2.0;

                    if (r > globalMax) globalMax = r;
                    if (r < globalMin) globalMin = r;
                }
            }

            return globalMax - globalMin;
        }

        // =========================================================
        // 辅助判定
        // =========================================================

        /// <summary>
        /// 外圆直径合格判定（逐点）
        /// </summary>
        public static bool CheckDiameterTolerance(
            SampleFrame frame,
            double lower,
            double upper)
        {
            var diameters = CalcDiameterByPoint(frame);

            return diameters.Values.All(d =>
                d >= lower && d <= upper);
        }
    }
}

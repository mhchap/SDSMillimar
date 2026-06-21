using System;
using System.Data;
using System.Linq;

namespace SDSMillimar.Utils
{
    /// <summary>
    /// X̄-R 控制图结果
    /// X 图：均值控制图
    /// R 图：极差控制图
    /// </summary>
    public class SPCResult
    {
        // ===== 每个子组的点 =====
        public double[] MeanValues;     // X̄i
        public double[] RangeValues;    // Ri
        public double[] StdDevValues;   // σi

        // ===== X̄ 图控制线 =====
        public double XBarCL;           // X̄̄
        public double XBarUCL;
        public double XBarLCL;

        // ===== R 图控制线 =====
        public double R_CL;             // R̄
        public double R_UCL;
        public double R_LCL;

        // ===== 过程能力（每子组一个）=====
        public double[] Cp;
        public double[] Cpk;
        public double[] Pp;
        public double[] Ppk;
    }

    public static class SPC
    {
        #region AIAG / ISO 常数表（n = 2 ~ 10）

        private static readonly double[] D2_TABLE =
        {
            1.128, 1.693, 2.059, 2.326, 2.534,
            2.704, 2.847, 2.970, 3.078
        };

        private static readonly double[] A2_TABLE =
        {
            1.880, 1.023, 0.729, 0.577, 0.483,
            0.419, 0.373, 0.337, 0.308
        };

        private static readonly double[] D3_TABLE =
        {
            0, 0, 0, 0.076, 0.136,
            0.184, 0.223, 0.256, 0.284
        };

        private static readonly double[] D4_TABLE =
        {
            3.267, 2.574, 2.282, 2.114, 2.004,
            1.924, 1.864, 1.816, 1.777
        };

        private static double D2(int n) => D2_TABLE[n - 2];
        private static double A2(int n) => A2_TABLE[n - 2];
        private static double D3(int n) => D3_TABLE[n - 2];
        private static double D4(int n) => D4_TABLE[n - 2];

        #endregion

        private static double StdDev(double[] values)
        {
            if (values.Length <= 1) return 0;
            double avg = values.Average();
            return Math.Sqrt(values.Sum(v => Math.Pow(v - avg, 2)) / (values.Length - 1));
        }

        /// <summary>
        /// X̄-R 控制图（按行作为子组）
        /// </summary>
        /// <param name="dt">
        /// DataTable：
        /// 第 0 列 = 时间 / 序号（忽略）
        /// 第 1~N 列 = 子组内样本
        /// 每一行 = 一个子组
        /// </param>
        public static SPCResult CalculateByRow(
            DataTable dt,
            double USL,
            double LSL)
        {
            if (dt == null || dt.Rows.Count < 1 || dt.Columns.Count < 3)
                throw new ArgumentException("数据不足，无法进行 SPC 计算");

            int subgroupCount = dt.Rows.Count;         // 行 = 子组数
            int subgroupSize = dt.Columns.Count - 1;  // 列 = 子组内样本数 n

            if (subgroupSize < 2 || subgroupSize > 10)
                throw new ArgumentOutOfRangeException("X̄-R 控制图要求子组容量 n = 2 ~ 10");

            double[] means = new double[subgroupCount];
            double[] ranges = new double[subgroupCount];
            double[] stdDevs = new double[subgroupCount];

            double[] Cp = new double[subgroupCount];
            double[] Cpk = new double[subgroupCount];
            double[] Pp = new double[subgroupCount];
            double[] Ppk = new double[subgroupCount];

            // ===== 每一行 = 一个子组 =====
            for (int row = 0; row < subgroupCount; row++)
            {

                double[] values = Enumerable
                      .Range(1, subgroupSize)
                      .Select(col =>
                      {
                          object v = dt.Rows[row][col];
                          return (v == null || v == DBNull.Value)
                              ? 0.0
                              : Convert.ToDouble(v);
                      })
                      .ToArray();

                double mean = values.Average();
                double range = values.Max() - values.Min();
                double std = StdDev(values);

                means[row] = mean;
                ranges[row] = range;
                stdDevs[row] = std;

                // ---- 短期能力 Cp / Cpk（组内）----
                double sigmaShort = range / D2(subgroupSize);

                Cp[row] = (USL - LSL) / (6 * sigmaShort);
                Cpk[row] = Math.Min(
                    (USL - mean) / (3 * sigmaShort),
                    (mean - LSL) / (3 * sigmaShort)
                );

                // ---- 长期能力 Pp / Ppk ----
                if (std > 1e-9)
                {
                    Pp[row] = (USL - LSL) / (6 * std);
                    Ppk[row] = Math.Min(
                        (USL - mean) / (3 * std),
                        (mean - LSL) / (3 * std)
                    );
                }
                else
                {
                    Pp[row] = double.PositiveInfinity;
                    Ppk[row] = double.PositiveInfinity;
                }
            }

            // ===== 控制线（全局）=====
            double XBarBar = means.Average();    // X̄̄
            double RBar = ranges.Average();  // R̄

            double xUcl = XBarBar + A2(subgroupSize) * RBar;
            double xLcl = XBarBar - A2(subgroupSize) * RBar;

            double rUcl = D4(subgroupSize) * RBar;
            double rLcl = D3(subgroupSize) * RBar;

            return new SPCResult
            {
                MeanValues = means,
                RangeValues = ranges,
                StdDevValues = stdDevs,

                XBarCL = XBarBar,
                XBarUCL = xUcl,
                XBarLCL = xLcl,

                R_CL = RBar,
                R_UCL = rUcl,
                R_LCL = rLcl,

                Cp = Cp,
                Cpk = Cpk,
                Pp = Pp,
                Ppk = Ppk
            };
        }
    }
}

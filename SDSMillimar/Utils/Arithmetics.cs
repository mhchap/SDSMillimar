using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDSMillimar.Utils
{
    public class Arithmetics
    {
        /// <summary>
        /// 直径算法
        /// 对称二探头
        /// </summary>
        /// <param name="probeA">探头1数据组</param>
        /// <param name="probeB">探头2数据组</param>
        /// <returns></returns>
        public static double CalcDiameterWithMean(List<double> probeA, List<double> probeB, double offset)
        {
            double sum = 0;
            int n = probeA.Count;

            for (int i = 0; i < n; i++)
            {
                sum += (probeA[i] + probeB[i]) / 2.0;
            }

            return 2.0 * sum / n + offset;
        }


        /// <summary>
        /// 圆度计算
        /// 极差圆度（Max - Min）
        /// 1.每个角度点算等效半径
        /// 2.求最大半径和最小半径
        /// 3.圆度 = Max - Min
        /// </summary>
        /// <param name="probeA"></param>
        /// <param name="probeB"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double CalcRoundness(List<double> probeA, List<double> probeB)
        {
            if (probeA.Count != probeB.Count)
                throw new ArgumentException("数据长度不一致");

            double rMax = double.MinValue;
            double rMin = double.MaxValue;

            for (int i = 0; i < probeA.Count; i++)
            {
                double r = (probeA[i] + probeB[i]) / 2.0;

                rMax = Math.Max(rMax, r);
                rMin = Math.Min(rMin, r);
            }

            return rMax - rMin;
        }


    }
}

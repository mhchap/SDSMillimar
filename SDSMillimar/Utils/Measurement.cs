using DocumentFormat.OpenXml.Spreadsheet;
using SDSMillimar.Common;
using SDSMillimar.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDSMillimar.Utils
{
    public static class Measurement
    {
        /// <summary>
        /// 中位数
        /// </summary>
        private static double Median(List<double> data)
        {
            var sorted = data.OrderBy(x => x).ToList();
            int n = sorted.Count;
            return n % 2 == 1 ? sorted[n / 2] : (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
        }

        /// <summary>
        /// 标准差
        /// </summary>
        private static double StdDev(List<double> data)
        {
            if (data.Count == 0) return 0;
            double mean = data.Average();
            double sum = data.Sum(x => (x - mean) * (x - mean));
            // 开平方，计算标准差
            return Math.Sqrt(sum / data.Count);
        }

        /// <summary>
        /// 自动跳变阈值
        /// </summary>
        private static double AutoJumpThreshold(List<double> data)
        {
            var diffs = new List<double>();

            //每一步测量变化了多少
            for (int i = 1; i < data.Count; i++)
                diffs.Add(Math.Abs(data[i] - data[i - 1]));

            //取中位数，而不是平均值
            double median = Median(diffs);
            return Math.Max(6 * median, 0.003); // 工业常用倍数
        }

        /// <summary>
        /// 异常点剔除
        /// </summary>
        //private static List<double> FilterData(List<double> data)
        //{
        //    if (data.Count == 0) return new List<double>();
        //    double threshold = AutoJumpThreshold(data);
        //    var filtered = new List<double> { data[0] };
        //    for (int i = 1; i < data.Count; i++)
        //    {
        //        if (Math.Abs(data[i] - data[i - 1]) <= threshold)
        //            filtered.Add(data[i]);
        //        else
        //            filtered.Add(filtered.Last()); // 或插值
        //    }
        //    return filtered;
        //}

        //private static List<double> FilterData(List<double> data)
        //{
        //    if (data.Count == 0) return new List<double>();

        //    double threshold = AutoJumpThreshold(data);
        //    var filtered = new List<double> { data[0] };

        //    for (int i = 1; i < data.Count; i++)
        //    {
        //        double prev = filtered.Last();
        //        double cur = data[i];

        //        if (Math.Abs(cur - prev) <= threshold)
        //            filtered.Add(cur);
        //        else
        //            filtered.Add(prev); // 或插值
        //    }

        //    return filtered;
        //}


        /// <summary>
        /// 保持点数不变的异常修复滤波（适合油槽 / 探头失效）
        /// </summary>
        private static List<double> FilterData(List<double> data)
        {
            if (data == null || data.Count == 0)
                return new List<double>();

            double threshold = AutoJumpThreshold(data);

            var result = new List<double>(data.Count);

            double lastValid = data[0];
            result.Add(lastValid);

            for (int i = 1; i < data.Count; i++)
            {
                double cur = data[i];

                // 正常点
                if (Math.Abs(cur - lastValid) <= threshold)
                {
                    result.Add(cur);
                    lastValid = cur;
                }
                else
                {
                    // 异常点（油槽、跳变）
                    // 用最近有效值修复
                    result.Add(lastValid);
                }
            }

            return result;
        }



        public static double CalcDiameter(
            string ParamValue,
            SampleFrame frame,
            double baseDiameter,
            bool enableFilter = true,
            double filterValue = 0,
            double compensationValue = 0)
        {
            try
            {
                CheckFrame(frame);
                int count = frame.APoints.Count;
                var centerList = new List<double>(count);

                // ===== 1. 先计算 A+B =====
                for (int i = 0; i < count; i++)
                {
                    centerList.Add(frame.APoints[i].Value + frame.BPoints[i].Value);
                }

                // ===== 2. 是否过滤 =====
                if (enableFilter)
                {
                    centerList = FilterOilGrooves(ParamValue, centerList).filteredData;
                }

                if (filterValue > 0)
                {
                    centerList = centerList
                        .Where(r => Math.Abs(r) <= filterValue)
                        .ToList();
                }

                if (!centerList.Any())
                {
                    AppLog.Production.Warn("CalcDiameter-> 过滤后无有效数据");
                    return 0;
                }

                //double max = centerList.Max();
                //double min = centerList.Min();

                var filtered = centerList.OrderBy(x => x).ToList();

                if (filtered.Count > 2)
                {
                    filtered.RemoveAt(0);                     // 去最小
                    filtered.RemoveAt(filtered.Count - 1);    // 去最大
                }

                double min = filtered.Min();
                double max = filtered.Max();

                AppLog.Production.Info(
                    $"计算直径-> 最大值:{max:F5}, 最小值:{min:F5}, 基础值:{baseDiameter},补偿值：{compensationValue}");

                return baseDiameter + max + min + compensationValue;
            }
            catch (Exception ex)
            {
                AppLog.Production.Error($"CalcDiameter->{ex.Message}");
                return 0;
            }
        }



        public static double CalcRunoutFor5MaxMin(
       string paramValue,
       SampleFrame frame,
       bool enableFilter = true, double filterValue = 0,
            double compensationValue = 0)
        {
            try
            {
                CheckFrame(frame);

                // ===== 1. 读取配置 =====
                double trimRatio = 0;
                if (!double.TryParse(
                        GlobalSession.Instance.FilterExtremumPre.ToString(),
                        out trimRatio))
                {
                    trimRatio = 0;
                }

                IEnumerable<double> aValues;
                //IEnumerable<double> bValues;

                // ===== 2. 过滤 =====
                if (enableFilter)
                {
                    aValues = FilterOilGrooves(paramValue + 'A', frame.APoints.Select(x => x.Value).ToList()).filteredData;
                    //bValues = FilterOilGrooves(paramValue + 'B', frame.BPoints.Select(x => x.Value).ToList()).filteredData;
                }
                else
                {
                    aValues = frame.APoints.Select(x => x.Value);
                    //bValues = frame.BPoints.Select(x => x.Value);
                }

                if (filterValue > 0)
                {
                    aValues = aValues
                        .Where(r => Math.Abs(r) <= filterValue)
                        .ToList();
                    //bValues = bValues
                    //   .Where(r => Math.Abs(r) <= filterValue)
                    //   .ToList();
                }

                AppLog.Production.Info($"{paramValue} A->{string.Join(",", aValues.Select(p => p.ToString("F5")))}");
                //AppLog.Production.Info($"{paramValue} B->{string.Join(",", bValues.Select(p => p.ToString("F5")))}");
                // ===== 3. 防止过滤后为空 =====
                if (!aValues.Any())//|| !bValues.Any()
                {
                    AppLog.Production.Warn("CalcRunoutFor5MaxMin -> 过滤后数据为空");
                    return 0;
                }

                var filtered = aValues.ToList();

                filtered.Remove(filtered.Min()); // 删除一个最小值
                filtered.Remove(filtered.Max()); // 删除一个最大值

                double newMin = filtered.Min();
                double newMax = filtered.Max();

                //double bMin = bValues.Min();
                //double bMax = bValues.Max();


                // A跳动值
                double aRunout = newMax - newMin;
                //double bRunout = bMax - bMin;
                //double abAvg = (aRunout + bRunout) / 2;
                double abAvg = aRunout / 2;

                // ===== 6. 日志 =====
                AppLog.Production.Info(
                    $"{paramValue} 计算跳动 -> " +
                    $"A[Min- {newMin:F4},Max- {newMax:F4}, Runout- {aRunout}] "

                //$"B[Min- {bMin:F4},B- {bMax:F4}, Runout- {bRunout}] " +
                //$"Trim:{trimRatio:P0} " +
                //$"最终跳动 :{abAvg:F4}"
                );

                return abAvg;
            }
            catch (Exception ex)
            {
                AppLog.Production.Error($"CalcRunoutFor5MaxMin -> {ex}");
                return 0;
            }
        }


        private static (double Min, double Max) GetTrimmedMinMax(
    List<double> values,
    double trimRatio)
        {
            if (values == null || values.Count == 0)
                return (0, 0);

            var sorted = values.OrderBy(x => x).ToList();
            int n = sorted.Count;

            int remove = (int)Math.Floor(n * trimRatio);
            remove = Math.Min(remove, (n - 1) / 2);

            return (
                sorted[remove],
                sorted[n - remove - 1]
            );
        }


        public static double CalcRoundnessFor5(
            string ParamValue,
            SampleFrame frame,
            bool enableFilter = true,
            double filterValue = 0,
            double compensationValue = 0)
        {
            try
            {
                CheckFrame(frame);

                var rList = new List<double>();

                // ===== 1. 原始 A / B =====
                var aValues = frame.APoints.Select(x => x.Value).ToList();
                var bValues = frame.BPoints.Select(x => x.Value).ToList();

                int count = Math.Min(aValues.Count, bValues.Count);

                // ===== 2. 先算半径（角度严格一一对应）=====
                for (int i = 0; i < count; i++)
                {
                    double r = (aValues[i] + bValues[i]) / 2.0;
                    rList.Add(r);
                }



                // ===== 3. 在“半径域”过滤油槽 =====
                if (enableFilter)
                {
                    rList = FilterOilGrooves(ParamValue, rList).filteredData;
                }

                // ===== 4. 额外绝对值过滤（可选）=====
                if (filterValue > 0)
                {
                    rList = rList
                        .Where(r => Math.Abs(r) <= filterValue)
                        .ToList();
                }

                if (rList.Count == 0)
                    return 0;
                AppLog.Production.Info(
                   $"{ParamValue} R(raw)->{string.Join(",", rList.Select(p => p.ToString("F5")))}");
                double min = rList.Min();
                double max = rList.Max();

                var filtered = rList.ToList();

                if (filtered.Count > 2)
                {
                    filtered.Remove(min);
                    filtered.Remove(max);
                }

                double newMin = filtered.Min();
                double newMax = filtered.Max();



                AppLog.Production.Info(
                    $"计算圆度-> 最大:{newMax}, 最小:{newMin}, 极差:{newMax - newMin}");

                return newMax - newMin;
            }
            catch (Exception ex)
            {
                AppLog.Production.Error($"CalcRoundnessFor5->{ex.Message}");
                return 0;
            }
        }



        public static List<double> FilterAuto(IEnumerable<double> values)
        {
            var (normal, outliers) = FilterByIQR(values);
            string aCsv = string.Join(",", outliers);
            AppLog.FilterData.Info(aCsv);
            return normal;
        }

        /// <summary>
        /// 使用 IQR（四分位距）法筛选异常值
        /// </summary>
        /// <param name="values">原始数据</param>
        /// <param name="factor">倍数，默认 1.5，越大越宽松</param>
        /// <returns>正常值 + 异常值</returns>
        public static (List<double> Normal, List<double> Outliers) FilterByIQR(
            IEnumerable<double> values,
            double factor = 1.5)
        {
            var data = values.ToList();
            if (data.Count < 4)
                return (data, new List<double>());

            // 排序副本（不影响原始顺序）
            var sorted = data.OrderBy(x => x).ToList();

            double q1 = Percentile(sorted, 0.25);
            double q3 = Percentile(sorted, 0.75);
            double iqr = q3 - q1;

            double lower = q1 - factor * iqr;
            double upper = q3 + factor * iqr;

            var normal = new List<double>();
            var outliers = new List<double>();

            foreach (var v in data)
            {
                if (v < lower || v > upper)
                    outliers.Add(v);
                else
                    normal.Add(v);
            }

            return (normal, outliers);
        }

        /// <summary>
        /// 百分位计算（线性插值）
        /// </summary>
        private static double Percentile(List<double> sorted, double p)
        {
            double pos = (sorted.Count - 1) * p;
            int left = (int)Math.Floor(pos);
            int right = (int)Math.Ceiling(pos);

            if (left == right)
                return sorted[left];

            return sorted[left] + (sorted[right] - sorted[left]) * (pos - left);
        }


        /// <summary>
        /// 纯净版，在之前过滤掉异常值
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        //public static double CalcRoundness(SampleFrame frame)
        //{
        //    CheckFrame(frame);

        //    var rList = frame.APoints
        //        .Zip(frame.BPoints, (a, b) => (a.Value - b.Value) / 2.0)
        //        .ToList();

        //    double rAvg = rList.Average();

        //    // 减平均值是 最大最小减平均值，是在算工件相对于‘最小二乘圆心’的最大径向起伏
        //    double max = rList.Max(r => r - rAvg);
        //    double min = rList.Min(r => r - rAvg);
        //    AppLog.Production.Info($"计算圆度-> 最大值:{max}, 最小值:{min}, 极差:{max - min}");
        //    return max - min;
        //}

        public static (
     double max,
     double min,
            double coreThreshold,
     List<double> cleanData,
     List<double> removedData
 )
 RemoveOilGrooveAndGetExtremes(
     List<double> data,
     double k = 6.0,
     int minSegmentLength = 3,
     double recoverRatio = 0.6,
     double slopeEps = 1e-6
 )
        {
            if (data == null || data.Count < 10)
                throw new ArgumentException("数据量不足");

            int n = data.Count;

            // ===== 1. Median =====
            var sorted = data.OrderBy(x => x).ToList();
            double median = sorted[n / 2];

            // ===== 2. MAD =====
            var absDevs = data
                .Select(x => Math.Abs(x - median))
                .OrderBy(x => x)
                .ToList();
            double mad = absDevs[n / 2];
            if (mad < 1e-9)
                mad = 1e-9;

            // ===== 3. 核心油槽判定阈值 =====
            double coreThreshold = median - k * mad;
            bool[] isCore = data.Select(x => x < coreThreshold).ToArray();
            bool[] removeMask = new bool[n];

            int i = 0;
            while (i < n)
            {
                if (!isCore[i])
                {
                    i++;
                    continue;
                }

                // ===== 4. 找核心油槽区间 =====
                int coreStart = i;
                while (i < n && isCore[i])
                    i++;
                int coreEnd = i - 1;

                if (coreEnd - coreStart + 1 < minSegmentLength)
                    continue;

                // ===== 5. 油槽深度 & 回升高度 =====
                double grooveMin = data
                    .Skip(coreStart)
                    .Take(coreEnd - coreStart + 1)
                    .Min();

                double grooveDepth = median - grooveMin;
                double recoverLevel = grooveMin + grooveDepth * recoverRatio;

                // ===== 6. 向左扩展（只能继续下降或轻微回升）=====
                int left = coreStart;
                while (left > 0)
                {
                    double prev = data[left - 1];
                    double curr = data[left];

                    // 进入明显回升或稳定区 → 停
                    if (prev > curr + slopeEps || prev >= recoverLevel)
                        break;

                    left--;
                }

                // ===== 7. 向右扩展（必须持续回升）=====
                int right = coreEnd;
                while (right < n - 1)
                {
                    double next = data[right + 1];
                    double curr = data[right];

                    // 回升结束 / 进入稳定区 → 停
                    if (next < curr - slopeEps || next >= recoverLevel)
                        break;

                    right++;
                }

                // ===== 8. 标记删除区间 =====
                for (int j = left; j <= right; j++)
                    removeMask[j] = true;
            }

            // ===== 9. 输出 clean / removed =====
            var cleanData = new List<double>();
            var removedData = new List<double>();

            for (int idx = 0; idx < n; idx++)
            {
                if (removeMask[idx])
                    removedData.Add(data[idx]);
                else
                    cleanData.Add(data[idx]);
            }

            if (cleanData.Count == 0)
                throw new InvalidOperationException("全部数据被剔除");

            return (
                cleanData.Max(),
                cleanData.Min(),
                coreThreshold,
                cleanData,
                removedData
            );
        }

        public static (
    double max,
    double min,
    List<double> cleanData,
    List<double> removedData)
RemoveOilGrooveByShape(
    List<double> data,
    double minDepth = 0.014,      // 最小油槽深度（核心参数）
    int minWidth = 3             // 最小油槽宽度
)
        {
            if (data == null || data.Count < 5)
                throw new ArgumentException("数据量不足");

            int n = data.Count;
            bool[] removeMask = new bool[n];

            // ===== 1. 找所有“谷底” =====
            for (int i = 1; i < n - 1; i++)
            {
                // 局部最小
                if (!(data[i] < data[i - 1] && data[i] < data[i + 1]))
                    continue;

                int valley = i;

                // ===== 2. 向左爬坡找峰 =====
                int left = valley;
                while (left > 0 && data[left - 1] >= data[left])
                    left--;

                // ===== 3. 向右爬坡找峰 =====
                int right = valley;
                while (right < n - 1 && data[right + 1] >= data[right])
                    right++;

                int width = right - left + 1;
                if (width < minWidth)
                    continue;

                double leftPeak = data[left];
                double rightPeak = data[right];
                double peak = Math.Max(leftPeak, rightPeak);

                double depth = peak - data[valley];

                // ===== 4. 深度判定 =====
                if (depth < minDepth)
                    continue;

                // ===== 5. 标记整段为油槽 =====
                for (int j = left; j <= right; j++)
                    removeMask[j] = true;
            }

            // ===== 6. 输出结果 =====
            var cleanData = new List<double>();
            var removedData = new List<double>();

            for (int i = 0; i < n; i++)
            {
                if (removeMask[i])
                    removedData.Add(data[i]);
                else
                    cleanData.Add(data[i]);
            }

            if (cleanData.Count == 0)
                throw new InvalidOperationException("全部数据被剔除");

            return (
                cleanData.Max(),
                cleanData.Min(),
                cleanData,
                removedData
            );
        }

        /// <summary>
        /// 剔除油槽值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="minDepth">最小油槽深度</param>
        /// <returns>返回剔除油槽值后的集合</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static (double max, double min, List<double> cleanData, List<double> removedData) RemoveOilGrooveByDepth(List<double> data, double minDepth = 0.014)
        {
            if (data == null || data.Count < 2)
                throw new ArgumentException("数据量不足");

            int n = data.Count;
            bool[] removeMask = new bool[n];

            int i = 0;
            while (i < n - 1)
            {
                // 判断下降幅度
                if (data[i] - data[i + 1] >= minDepth)
                {
                    // 进入油槽
                    int start = i;
                    int end = i + 1;

                    // 向后找油槽结束点
                    while (end < n - 1 && data[end] - data[end + 1] >= 0)
                        end++;

                    // 标记为油槽
                    for (int j = start; j <= end; j++)
                        removeMask[j] = true;

                    i = end + 1;
                }
                else
                {
                    i++;
                }
            }

            // ===== 输出结果 =====
            var cleanData = new List<double>();
            var removedData = new List<double>();

            for (int j = 0; j < n; j++)
            {
                if (removeMask[j])
                    removedData.Add(data[j]);
                else
                    cleanData.Add(data[j]);
            }

            if (cleanData.Count == 0)
                throw new InvalidOperationException("全部数据被剔除");

            return (
                cleanData.Max(),
                cleanData.Min(),
                cleanData,
                removedData
            );
        }

        /// <summary>
        /// 线性替换油槽值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="minDepth">最小油槽深度</param>
        /// <returns>返回线性替换油槽值后的集合</returns>
        /// <exception cref="ArgumentException"></exception>
        public static (
      double max,
      double min,
      List<double> replacedData,
      List<double> removedData
  ) ReplaceOilGroovesByDepth(
      List<double> data,
      double minDepth = 0.014
  )
        {
            if (data == null || data.Count < 2)
                throw new ArgumentException("数据量不足");

            int n = data.Count;
            var result = new List<double>(data);
            var removedData = new List<double>();

            int i = 0;

            while (i < n - 1)
            {
                // 判断是否可能进入油槽
                if (Math.Abs(result[i] - result[i + 1]) >= minDepth)
                {
                    int start = i;
                    int end = i + 1;

                    // ========= ① 找油槽最低点（下降段） =========
                    while (end < n - 1 && result[end] - result[end + 1] >= 0)
                        end++;

                    // 左侧正常面
                    double leftValue = start > 0 ? result[start - 1] : result[start];

                    // ========= ② 找真正的右侧正常面 =========
                    int rightIndex = end + 1;

                    // 回升到接近左侧正常面，才认为出油槽
                    double recoverThreshold = minDepth * 0.3; // 可调，经验值

                    while (rightIndex < n &&
                           Math.Abs(leftValue - result[rightIndex]) >= recoverThreshold)
                    {
                        rightIndex++;
                    }

                    if (rightIndex >= n)
                        rightIndex = n - 1;

                    double rightValue = result[rightIndex];

                    // ========= ③ 记录被替换的原始数据 =========
                    for (int j = start; j < rightIndex; j++)
                        removedData.Add(data[j]);

                    // ========= ④ 线性桥接替换 =========
                    int length = rightIndex - start + 1;

                    if (length == 1)
                    {
                        result[start] = (leftValue + rightValue) / 2.0;
                    }
                    else
                    {
                        for (int j = 0; j < length; j++)
                        {
                            result[start + j] =
                                leftValue + (rightValue - leftValue) * j / (length - 1);
                        }
                    }

                    // 跳过整个油槽区
                    i = rightIndex + 1;
                }
                else
                {
                    i++;
                }
            }

            // ========= ② 再在替换后的结果上，用绝对值清异常 =========
            //i = 0;
            //while (i < n)
            //{
            //    if (Math.Abs(result[i]) >= absThreshold)
            //    {
            //        int start = i;
            //        int end = i;

            //        while (end < n && Math.Abs(result[end]) >= absThreshold)
            //            end++;

            //        for (int j = start; j < end; j++)
            //            removedData.Add(data[j]);

            //        double leftValue = start > 0 ? result[start - 1] : result[start];
            //        double rightValue = end < n ? result[end] : result[start];

            //        int length = end - start;
            //        for (int j = 0; j < length; j++)
            //        {
            //            result[start + j] =
            //                leftValue + (rightValue - leftValue) * (j + 1) / (length + 1);
            //        }

            //        i = end;
            //    }
            //    else
            //    {
            //        i++;
            //    }
            //}

            return (
                result.Max(),
                result.Min(),
                result,
                removedData
            );
        }

        public static List<(int start, int end, double valley)> FindOilValleyValues(
     List<double> data,
     double minDepth = 0.014)
        {
            if (data == null || data.Count < 2)
                return new List<(int, int, double)>();

            int n = data.Count;
            var result = new List<double>(data); // 原始数据副本
            var valleys = new List<(int start, int end, double valley)>();

            int i = 0;
            while (i < n - 1)
            {
                // 判断是否进入油槽
                if (Math.Abs(result[i] - result[i + 1]) >= minDepth)
                {
                    int start = i;
                    int end = i + 1;

                    // 找下降段最低点
                    while (end < n - 1 && result[end] - result[end + 1] >= 0)
                        end++;

                    // 左侧正常面
                    double leftValue = start > 0 ? result[start - 1] : result[start];

                    // 找回升段，直到接近左侧正常面
                    int rightIndex = end + 1;
                    double recoverThreshold = minDepth * 0.3; // 可调经验值
                    while (rightIndex < n && Math.Abs(leftValue - result[rightIndex]) >= recoverThreshold)
                        rightIndex++;

                    if (rightIndex >= n)
                        rightIndex = n - 1;

                    // 记录波谷
                    double valley = result.GetRange(start, rightIndex - start + 1).Min();
                    valleys.Add((start, rightIndex, valley));

                    // 跳过当前油槽
                    i = rightIndex + 1;
                }
                else
                {
                    i++;
                }
            }

            return valleys;
        }


        /// <summary>
        /// 找波谷
        /// </summary>
        /// <param name="data"></param>
        /// <param name="depthThreshold"></param>
        /// <returns>波谷组</returns>
        public static List<(int index, double valley)> FindOilValleys(
    List<double> data,
    double depthThreshold = 0.01 // 波谷深度阈值，可调
)
        {
            var valleys = new List<(int, double)>();
            if (data == null || data.Count < 2)
                return valleys;

            int n = data.Count;

            for (int i = 0; i < n; i++)
            {
                double left = i == 0 ? data[i] + depthThreshold * 2 : data[i - 1];
                double right = i == n - 1 ? data[i] + depthThreshold * 2 : data[i + 1];

                // 局部最小值
                if (data[i] < left && data[i] < right)
                {
                    // 找左侧正常面
                    int l = i;
                    while (l > 0 && data[l] < data[l - 1])
                        l--;
                    double leftNormal = data[l];

                    // 找右侧正常面
                    int r = i;
                    while (r < n - 1 && data[r] < data[r + 1])
                        r++;
                    double rightNormal = data[r];

                    // 波谷深度
                    double valleyDepth = Math.Min(leftNormal - data[i], rightNormal - data[i]);

                    if (valleyDepth >= depthThreshold)
                    {
                        valleys.Add((i, data[i]));
                    }
                }
            }

            return valleys;
        }


        public static (List<double> filteredData, List<double> removedData, List<(int index, double value)> valleys)
RemoveAroundValleys(string p, List<double> data, int margin = 2, double depthThreshold = 0.01)
        {
            var filteredData = new List<double>(data);
            var removedData = new List<double>();
            var valleys = new List<(int index, double value)>();

            if (data == null || data.Count < 2)
                return (filteredData, removedData, valleys);

            int n = data.Count;
            var toRemove = new bool[n];

            // =====================================================
            // 1️⃣ 中间对称油槽
            // =====================================================
            for (int i = 1; i < n - 1; i++)
            {
                if (data[i] < data[i - 1] && data[i] < data[i + 1])
                {
                    int l = i;
                    while (l > 0 && data[l] < data[l - 1]) l--;

                    int r = i;
                    while (r < n - 1 && data[r] < data[r + 1]) r++;

                    double leftNormal = data[l];
                    double rightNormal = data[r];
                    double depth = Math.Min(leftNormal - data[i], rightNormal - data[i]);

                    if (depth >= depthThreshold)
                    {
                        valleys.Add((i, data[i]));

                        for (int j = Math.Max(0, i - margin); j <= Math.Min(n - 1, i + margin); j++)
                            toRemove[j] = true;
                    }
                }
            }

            // =====================================================
            // 2️⃣ 前端单边油槽（起点一路下坡）
            // =====================================================
            int headMinIndex = 0;
            double headMinValue = data[0];

            for (int i = 1; i < n; i++)
            {
                if (data[i] <= headMinValue)
                {
                    headMinValue = data[i];
                    headMinIndex = i;
                }
                else
                {
                    // 出现回升
                    if (data[i] - headMinValue >= depthThreshold)
                        break;
                }
            }

            if (headMinIndex > 0)
            {
                double rightNormal = data[Math.Min(n - 1, headMinIndex + 1)];
                double depth = rightNormal - headMinValue;

                if (depth >= depthThreshold)
                {
                    valleys.Add((headMinIndex, headMinValue));

                    for (int j = 0; j <= Math.Min(n - 1, headMinIndex + margin); j++)
                        toRemove[j] = true;
                }
            }

            // =====================================================
            // 3️⃣ 末端单边油槽（一路下坠无回升）
            // =====================================================
            int tailMinIndex = n - 1;
            double tailMinValue = data[n - 1];

            for (int i = n - 2; i >= 0; i--)
            {
                if (data[i] <= tailMinValue)
                {
                    tailMinValue = data[i];
                    tailMinIndex = i;
                }
                else
                {
                    if (data[i] - tailMinValue >= depthThreshold)
                        break;
                }
            }

            if (tailMinIndex < n - 1)
            {
                double leftNormal = data[Math.Max(0, tailMinIndex - 1)];
                double depth = leftNormal - tailMinValue;

                if (depth >= depthThreshold)
                {
                    valleys.Add((tailMinIndex, tailMinValue));

                    for (int j = Math.Max(0, tailMinIndex - margin); j < n; j++)
                        toRemove[j] = true;
                }
            }

            // =====================================================
            // 4️⃣ 剔除
            // =====================================================
            for (int i = 0; i < n; i++)
                if (toRemove[i])
                    removedData.Add(data[i]);

            filteredData = data
                .Where((v, idx) => !toRemove[idx])
                .ToList();

            // =====================================================
            // 5️⃣ 日志
            // =====================================================
            AppLog.FilterData.Info($"{p}->原始数据->{string.Join(",", data.Select(x => x.ToString("F6")))}");
            AppLog.FilterData.Info($"{p}->油槽剔除->{string.Join(",", removedData.Select(x => x.ToString("F6")))}");
            AppLog.FilterData.Info($"{p}->油槽波谷组->" + string.Join(",", valleys.Select(v => $"({v.Item1},{v.Item2.ToString("F6")})")));
            AppLog.FilterData.Info($"{p}->过滤后数据->{string.Join(",", filteredData.Select(x => x.ToString("F6")))}");

            return (filteredData, removedData, valleys);
        }



        public static (
    List<double> filteredData,
    List<double> removedData,
    List<(int index, double value)> valleys)
FilterOilGrooves(
    string p,
    List<double> data,
    double depthThreshold = 0.01,
    int stableWindow = 5)
        {
            var filteredData = new List<double>();
            var removedData = new List<double>();
            var valleys = new List<(int index, double value)>();

            if (data == null || data.Count < 3)
                return (data?.ToList() ?? new List<double>(), removedData, valleys);

            int n = data.Count;
            var toRemove = new bool[n];

            // ===============================
            // 1️⃣ 计算全局稳定参考高度（中位数更稳）
            // ===============================
            var sorted = data.OrderBy(x => x).ToList();
            double globalNormal = sorted[n / 2];

            // ===============================
            // 2️⃣ 找所有局部极小值（允许波动）
            // ===============================
            for (int i = 0; i < n; i++)
            {
                double left = i > 0 ? data[i - 1] : data[i];
                double right = i < n - 1 ? data[i + 1] : data[i];

                bool isValley =
                    data[i] <= left &&
                    data[i] <= right &&
                    (globalNormal - data[i]) >= depthThreshold;

                if (!isValley)
                    continue;

                valleys.Add((i, data[i]));

                // ===============================
                // 3️⃣ 向左右扩展，直到回到正常区
                // ===============================
                int l = i;
                while (l > 0 && globalNormal - data[l] >= depthThreshold * 0.5)
                    l--;

                int r = i;
                while (r < n - 1 && globalNormal - data[r] >= depthThreshold * 0.5)
                    r++;

                for (int j = l; j <= r; j++)
                    toRemove[j] = true;
            }

            // ===============================
            // 4️⃣ 输出结果
            // ===============================
            for (int i = 0; i < n; i++)
            {
                if (toRemove[i])
                    removedData.Add(data[i]);
                else
                    filteredData.Add(data[i]);
            }

            // ===============================
            // 5️⃣ 日志
            // ===============================
            AppLog.FilterData.Info($"{p}->原始数据->{string.Join(",", data.Select(x => x.ToString("F6")))}");
            AppLog.FilterData.Info($"{p}->油槽剔除->{string.Join(",", removedData.Select(x => x.ToString("F6")))}");
            AppLog.FilterData.Info($"{p}->油槽波谷组->" +
                string.Join(",", valleys.Select(v => $"({v.index},{v.value:F6})")));
            AppLog.FilterData.Info($"{p}->过滤后数据->{string.Join(",", filteredData.Select(x => x.ToString("F6")))}");

            return (filteredData, removedData, valleys);
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
    }

}

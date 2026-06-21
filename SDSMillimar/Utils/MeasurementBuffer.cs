//using SDSMillimar.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace SDSMillimar.Utils
//{
//    public class MeasurementBuffer
//    {
//        private readonly List<SampleCycle> _cycles = new List<SampleCycle>();
//        private readonly object _lock = new object();

//        private List<SampleFrame> _currentFrames;
//        private int _pointIndex;

//        /// <summary>
//        /// 开始一次采集周期（N 组设备）
//        /// </summary>
//        public void BeginCycle(int frameCount)
//        {
//            if (frameCount <= 0)
//                throw new ArgumentException("frameCount 必须大于 0");

//            lock (_lock)
//            {
//                _pointIndex = 1;
//                _currentFrames = new List<SampleFrame>();

//                for (int i = 0; i < frameCount; i++)
//                {
//                    _currentFrames.Add(new SampleFrame
//                    {
//                        FrameIndex = i   // 👈 建议在 SampleFrame 里加
//                    });
//                }
//            }
//        }

//        /// <summary>
//        /// 给指定 Frame 添加一个点
//        /// </summary>
//        public void AddPoint(int frameIndex, double probeA, double probeB)
//        {
//            lock (_lock)
//            {
//                if (_currentFrames == null)
//                    throw new InvalidOperationException("请先调用 BeginCycle");

//                if (frameIndex < 0 || frameIndex >= _currentFrames.Count)
//                    throw new ArgumentOutOfRangeException(nameof(frameIndex));

//                var frame = _currentFrames[frameIndex];

//                frame.APoints.Add(new ProbePoint
//                {
//                    PointIndex = _pointIndex,
//                    Value = probeA
//                });

//                frame.BPoints.Add(new ProbePoint
//                {
//                    PointIndex = _pointIndex,
//                    Value = probeB
//                });
//            }
//        }

//        /// <summary>
//        /// 当前点采集完成（所有 Frame 都加完后调用一次）
//        /// </summary>
//        public void NextPoint()
//        {
//            lock (_lock)
//            {
//                _pointIndex++;
//            }
//        }

//        /// <summary>
//        /// 结束本次采集周期
//        /// </summary>
//        public void EndCycle()
//        {
//            lock (_lock)
//            {
//                if (_currentFrames == null)
//                    return;

//                var cycle = new SampleCycle();
//                cycle.Frames.AddRange(_currentFrames);

//                _cycles.Add(cycle);

//                _currentFrames = null;
//            }
//        }

//        /// <summary>
//        /// 获取最近一次采集周期
//        /// </summary>
//        public SampleCycle GetLatestCycle()
//        {
//            lock (_lock)
//            {
//                return _cycles.LastOrDefault();
//            }
//        }

//        /// <summary>
//        /// 获取所有采集周期
//        /// </summary>
//        public List<SampleCycle> GetAllCycles()
//        {
//            lock (_lock)
//            {
//                return _cycles.ToList();
//            }
//        }

//        public void Clear()
//        {
//            lock (_lock)
//            {
//                _cycles.Clear();
//                _currentFrames = null;
//                _pointIndex = 1;
//            }
//        }
//    }
//}

using SDSMillimar.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDSMillimar.Utils
{
    public class MeasurementBuffer
    {
        private readonly List<SampleFrame> _frames = new List<SampleFrame>();
        private readonly object _lock = new object();

        private SampleFrame _currentFrame;
        private int _pointIndex = 1;

        /// <summary>
        /// 开始一帧新的采样（PointIndex 从 1 开始）
        /// </summary>
        public void BeginFrame()
        {
            lock (_lock)
            {
                _currentFrame = new SampleFrame();
                _pointIndex = 1;
            }
        }

        /// <summary>
        /// 添加一个测量点（A/B 共用同一个 PointIndex）
        /// </summary>
        public void AddPoint(double probeA, double probeB)
        {
            lock (_lock)
            {
                if (_currentFrame == null)
                    _currentFrame = new SampleFrame();

                int index = _pointIndex++;

                _currentFrame.APoints.Add(new ProbePoint
                {
                    PointIndex = index,
                    Value = probeA
                });

                _currentFrame.BPoints.Add(new ProbePoint
                {
                    PointIndex = index,
                    Value = probeB
                });
            }
        }

        /// <summary>
        /// 结束当前帧并保存
        /// </summary>
        public void EndFrame()
        {
            lock (_lock)
            {
                if (_currentFrame == null)
                    return;

                if (_currentFrame.APoints.Count > 0 &&
                    _currentFrame.BPoints.Count > 0)
                {
                    _frames.Add(_currentFrame);
                }

                _currentFrame = null;
            }
        }
        /// <summary>
        /// 获取指定 SampleFrame 的 APoints 和 BPoints 值，逗号分隔
        /// </summary>
        public static string GetFramePointsCsv(SampleFrame frame)
        {
            if (frame == null)
                return string.Empty;

            var aValues = frame.APoints.Select(p => p.Value.ToString("F5")); // 可保留5位小数
            var bValues = frame.BPoints.Select(p => p.Value.ToString("F5"));

            string aCsv = string.Join(",", aValues);
            string bCsv = string.Join(",", bValues);

            return $"A: {aCsv}\nB: {bCsv}";
        }


        /// <summary>
        /// 获取最近一帧
        /// </summary>
        public SampleFrame GetLatestFrame()
        {
            lock (_lock)
            {
                return _frames.Count == 0 ? null : _frames[_frames.Count - 1];
            }
        }

        /// <summary>
        /// 获取最近 N 帧
        /// </summary>
        public List<SampleFrame> GetLastFrames(int count)
        {
            lock (_lock)
            {
                return _frames
                    .Skip(Math.Max(0, _frames.Count - count))
                    .ToList();
            }
        }

        /// <summary>
        /// 获取所有帧
        /// </summary>
        public List<SampleFrame> GetAllFrames()
        {
            lock (_lock)
            {
                return _frames.ToList();
            }
        }

        /// <summary>
        /// 清空（换工件 / 重测）
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _frames.Clear();
                _currentFrame = null;
                _pointIndex = 1;
            }
        }
    }
}

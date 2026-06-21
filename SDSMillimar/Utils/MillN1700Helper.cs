using Mysqlx;
using SDSMillimar.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace SDSMillimar.Utils
{
    public class MillN1700Helper
    {
        private static readonly Lazy<MillN1700Helper> _instance =
            new Lazy<MillN1700Helper>(() => new MillN1700Helper());

        public static MillN1700Helper Instance => _instance.Value;

        int iResult;
        uint NumModules;
        uint NumChannels;
        N1700Lib.N1700version ver = new N1700Lib.N1700version();

        public N1700Lib.N1700MsgCallback MCallback;
        public N1700Lib.N1700ExtDataCallback DCallback;

        public event Action<ObservableCollection<ChannelModel>> DataBatchReceived;

        static int MaxModuleCount = 100;
        static int MaxChannelCount = 400;
        static int iContext = 0x33;

        N1700Lib.sN1700_Module[] aN1700_Module = new N1700Lib.sN1700_Module[MaxModuleCount];
        N1700Lib.sN1700_Channel[] aN1700_Channel = new N1700Lib.sN1700_Channel[MaxChannelCount];

        bool DataCallbackRegistered = false;
        int LastTick;

        /// <summary>
        /// 最新值缓存（用于清零）
        /// </summary>
        private readonly Dictionary<uint, double> _latestValues =
            new Dictionary<uint, double>();

        private readonly object _valueLock = new object();

        private MillN1700Helper() { }

        #region 初始化

        public void Init()
        {
            iResult = N1700Lib.N1700InitializeLibrary(false, out NumModules, out NumChannels, 0);

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(ver));
            N1700Lib.N1700GetVersion(ptr);
            ver = (N1700Lib.N1700version)Marshal.PtrToStructure(ptr, typeof(N1700Lib.N1700version));
            Marshal.FreeHGlobal(ptr);

            MCallback = new N1700Lib.N1700MsgCallback(MsgCallback);
            N1700Lib.N1700RegisterMsgCallback(MCallback);

            GetModulesData();
            GetChannelsData();
        }

        #endregion

        #region ⭐ 开始 / 停止连续读取（加回）

        public void N1700StartContinuousRequestAllData()
        {
            var result = N1700Lib.N1700StartContinuousRequestAllData(0, 0);
            Console.WriteLine($"N1700StartContinuousRequestAllData - {result}");
        }

        public void N1700StopContinuousRequestAllData()
        {
            var result = N1700Lib.N1700StopContinuousRequestAllData();
            Console.WriteLine($"N1700StopContinuousRequestAllData - {result}");
        }

        #endregion

        #region 模块 / 通道信息

        public void GetModulesData()
        {
            for (uint i = 0; i < NumModules && i < MaxModuleCount; i++)
            {
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(aN1700_Module[i]));
                N1700Lib.N1700GetModule(i, ptr);
                aN1700_Module[i] =
                    (N1700Lib.sN1700_Module)Marshal.PtrToStructure(ptr, typeof(N1700Lib.sN1700_Module));
                Marshal.FreeHGlobal(ptr);
            }
        }

        public void GetChannelsData()
        {
            for (uint i = 0; i < NumChannels && i < MaxChannelCount; i++)
            {
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(aN1700_Channel[i]));
                N1700Lib.N1700GetChannel(i, ptr);
                aN1700_Channel[i] =
                    (N1700Lib.sN1700_Channel)Marshal.PtrToStructure(ptr, typeof(N1700Lib.sN1700_Channel));
                Marshal.FreeHGlobal(ptr);
            }

            if (DataCallbackRegistered)
                N1700Lib.N1700UnregisterExtDataCallback(DCallback);

            RegisterDataCallback();
            DataCallbackRegistered = true;

            N1700Lib.N1700RequestAllData(0);
        }

        #endregion

        #region 回调注册

        public void RegisterDataCallback()
        {
            uint[] channelIdxs = new uint[NumChannels];
            for (int i = 0; i < NumChannels; i++)
                channelIdxs[i] = aN1700_Channel[i].ChannelIdx;

            IntPtr pContext = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(pContext, iContext);

            DCallback = new N1700Lib.N1700ExtDataCallback(DataCallback);
            N1700Lib.N1700RegisterExtDataCallback(
                DCallback,
                (int)NumChannels,
                channelIdxs,
                pContext
            );
        }

        #endregion

        #region 消息回调
        public event Action<bool> CommunicationChanged;
        public event Action<uint> ModulesChanged;
        public event Action<uint> ChannelsChanged;

        public void MsgCallback(int msg, uint channel, int param)
        {
            switch (msg)
            {
                case N1700Lib.WM_N1700_ModuleCountChanged:
                    NumModules = (uint)param;
                    GetModulesData();
                    ModulesChanged?.Invoke(NumModules);
                    break;

                case N1700Lib.WM_N1700_ChannelCountChanged:
                    NumChannels = (uint)param;
                    GetChannelsData();
                    ChannelsChanged?.Invoke(NumChannels);
                    break;
                case N1700Lib.WM_N1700_Communication:
                    _communicationOk = param != 0;
                    CommunicationChanged?.Invoke(_communicationOk);
                    break;
            }
        }

        #endregion

        #region 数据回调（核心）

        public void DataCallback(int numData, IntPtr pData, IntPtr context)
        {
            lock (_dataLock)
            {
                if (Environment.TickCount <= LastTick + 10)
                    return;

                int size = Marshal.SizeOf(typeof(N1700Lib.sN1700_ChannelExtData));
                var list = new ObservableCollection<ChannelModel>();

                for (int i = 0; i < numData; i++)
                {
                    var data = (N1700Lib.sN1700_ChannelExtData)
                        Marshal.PtrToStructure(pData, typeof(N1700Lib.sN1700_ChannelExtData));

                    pData += size;

                    // ⭐⭐ 核心：更新最新值缓存
                    lock (_valueLock)
                    {
                        _latestValues[data.ChannelIdx] = data.dValue;
                    }

                    list.Add(new ChannelModel
                    {
                        ChannelIdx = data.ChannelIdx,
                        Value = data.dValue,
                        ValueType = data.ValueType,
                        Referenced = data.Referenced > 0,
                        Timestamp = DateTime.Now
                    });
                }

                DataBatchReceived?.Invoke(list);
                LastTick = Environment.TickCount;
                _lastDataTick = Environment.TickCount;
            }
        }

        #endregion

        #region ⭐ 清零（最终正确）
        private readonly object _dataLock = new object();
        public bool N1700SetOffsetMM(uint channelIdx, float value)
        {
            lock (_dataLock)
            {
                int ret;


                // 4️⃣ Poll 真实值
                double rawValue;
                //ret = N1700Lib.N1700PollData(Convert.ToUInt32(channelIdx), out rawValue);
                //if (ret != N1700Lib.N1700_SUCCESS)
                //    return false;


                // 5️⃣ 设置 Offset 设置0就是恢复偏移
                ret = N1700Lib.N1700SetOffsetMM(Convert.ToUInt32(channelIdx), value);
                if (ret != N1700Lib.N1700_SUCCESS)
                    return false;

                //ret = N1700Lib.N1700PollData(Convert.ToUInt32(channelIdx), out rawValue);
                return true;
            }
        }

        #endregion

        private volatile bool _communicationOk = false;
        private volatile int _lastDataTick = 0;
        /// <summary>
        /// 判断当前 N1700 是否具备开始测量的条件
        /// </summary>
        /// <param name="reason">不满足原因（可用于 UI 提示）</param>
        /// <returns></returns>
        public bool CanStartMeasurement(out string reason)
        {
            reason = null;

            // 1️⃣ 是否初始化
            if (NumModules <= 0 || NumChannels <= 0)
            {
                reason = "未检测到 N1700 设备";
                return false;
            }

            // 2️⃣ USB 通信是否正常
            if (!_communicationOk)
            {
                reason = "N1700 通信中断（USB 断开）";
                return false;
            }

            // 3️⃣ 数据回调是否注册
            if (!DataCallbackRegistered || DCallback == null)
            {
                reason = "数据回调未注册";
                return false;
            }

            // 4️⃣ 通道信息是否合法
            for (int i = 0; i < NumChannels; i++)
            {
                if (aN1700_Channel[i].ChannelIdx == 0 && NumChannels > 1)
                {
                    reason = $"通道 {i} 信息异常";
                    return false;
                }
            }

            // 5️⃣ 是否在最近 1 秒内收到过数据
            int now = Environment.TickCount;
            if (_lastDataTick == 0 || now - _lastDataTick > 1000)
            {
                reason = "未收到实时测量数据";
                return false;
            }

            // 6️⃣ 最新值缓存是否完整
            lock (_valueLock)
            {
                if (_latestValues.Count < NumChannels)
                {
                    reason = "测量数据尚未全部就绪";
                    return false;
                }
            }

            return true;
        }


        public bool ZeroAllChannelsSafe(bool restartAfterZero = true)
        {
            // 1️⃣ 停止采集
            lock (_dataLock)
            {
                N1700Lib.N1700StopContinuousRequestAllData();
            }

            Thread.Sleep(300);

            // 2️⃣ 恢复所有通道 Offset
            lock (_dataLock)
            {
                for (uint i = 0; i < NumChannels; i++)
                {
                    int ret = N1700Lib.N1700SetOffsetMM(i, 0);
                    if (ret != N1700Lib.N1700_SUCCESS)
                        return false;

                    Thread.Sleep(10);
                }
            }

            // 3️⃣ 等设备内部生效
            Thread.Sleep(300);

            // 4️⃣ 取当前值快照
            Dictionary<uint, double> snapshot;
            lock (_valueLock)
            {
                snapshot = new Dictionary<uint, double>(_latestValues);
            }

            // 5️⃣ 正式回零
            lock (_dataLock)
            {
                foreach (var kv in snapshot)
                {
                    int ret = N1700Lib.N1700SetOffsetMM(kv.Key, (float)kv.Value);
                    if (ret != N1700Lib.N1700_SUCCESS)
                        return false;

                    Thread.Sleep(20);
                }
            }

            // 6️⃣ 恢复采集
            if (restartAfterZero)
            {
                Thread.Sleep(200);
                N1700Lib.N1700StartContinuousRequestAllData(0, 0);
            }

            return true;
        }



        public void UnInit()
        {
            lock (_dataLock)
            {
                try
                {
                    // 1️⃣ 停止连续采集
                    N1700Lib.N1700StopContinuousRequestAllData();

                    Thread.Sleep(200);

                    // 2️⃣ 注销数据回调
                    if (DataCallbackRegistered)
                    {
                        N1700Lib.N1700UnregisterExtDataCallback(DCallback);
                        DataCallbackRegistered = false;
                    }

                    // 3️⃣ 注销消息回调（如果 SDK 支持）
                    if (MCallback != null)
                    {
                        N1700Lib.N1700RegisterMsgCallback(null);
                        MCallback = null;
                    }

                    // 4️⃣ 清空本地缓存
                    lock (_valueLock)
                    {
                        _latestValues.Clear();
                    }

                    LastTick = 0;
                    NumModules = 0;
                    NumChannels = 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("UnInit error: " + ex.Message);
                }
            }
        }



    }
}

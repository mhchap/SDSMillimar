using S7.Net;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace SDSMillimar.Common
{
    public class S7NetController
    {
        private static readonly Lazy<S7NetController> _instance = new Lazy<S7NetController>(() => new S7NetController());
        public static S7NetController Instance => _instance.Value;

        private Plc plc;
        private bool heartbeat = false;
        private System.Timers.Timer heartbeatTimer;


        private CancellationTokenSource listenerCts;

        // 点位订阅表
        private readonly Dictionary<string, object> lastValues = new Dictionary<string, object>();
        private readonly List<string> subscribedAddresses = new List<string>();

        public event EventHandler<(string Address, object Value)> OnValueChanged;

        // 私有构造，防止外部实例化
        private S7NetController() { }

        /// <summary>
        /// 初始化PLC客户端
        /// </summary>
        public bool Connect(string ip, int port, short rack = 0, short slot = 1)
        {
            try
            {
                if (plc != null && plc.IsConnected)
                    return false; // 已连接则不重复连接
                plc = new Plc(CpuType.S71200, ip, rack, slot);
                plc.Open();
                AppLog.Plc.Info("PLC连接成功");
                StartHeartbeat();
                return true;
            }
            catch (Exception ex)
            {
                AppLog.Plc.Error($"PLC连接失败,{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 启动心跳信号，每秒反转一次
        /// </summary>
        private void StartHeartbeat()
        {
            heartbeatTimer = new System.Timers.Timer(1000);
            heartbeatTimer.Elapsed += (s, e) =>
            {
                try
                {
                    if (plc != null && plc.IsConnected)
                    {
                        heartbeat = !heartbeat;
                        plc.Write("DB203.DBX66.0", heartbeat);
                        AppLog.PlcHeartbeat.Info($"心跳报文->{heartbeat}");
                    }
                }
                catch (Exception ex)
                {
                    AppLog.PlcHeartbeat.Error($"心跳发送异常: {ex.Message}");
                    TryReconnect();
                }
            };
            heartbeatTimer.Start();
        }


        /// <summary>
        /// 封装PLC写点位操作，自动处理异常和日志
        /// </summary>
        /// <param name="address">PLC点位地址</param>
        /// <param name="value">要写入的值</param>
        public bool WritePlcPoint(string address, object value)
        {
            try
            {
                if (plc != null && plc.IsConnected)
                {
                    plc.Write(address, value);
                    AppLog.Plc.Info($"PLC写点位成功: {address} -> {value}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                AppLog.Plc.Error($"PLC写点位失败: {address} -> {value}, Error: {ex.Message}");
                return false;
            }
            return false;
        }

        /// <summary>
        /// PLC断线重连
        /// </summary>
        private void TryReconnect()
        {
            try
            {
                if (plc != null && !plc.IsConnected)
                {
                    AppLog.Plc.Info("尝试重新连接PLC...");
                    plc.Close();
                    Thread.Sleep(1000);
                    plc.Open();
                    AppLog.Plc.Info("PLC重新连接成功");
                }
            }
            catch (Exception ex)
            {
                AppLog.Plc.Error($"PLC重连失败: {ex.Message}");
            }
        }


        /// <summary>
        /// 根据PLC地址读取数据
        /// </summary>
        /// <param name="address">PLC地址，例如 "DB800.DBX0.0" 或 "DB800.DBD36"</param>
        /// <returns>返回 object，需要调用处自行转换类型</returns>
        public object ReadByAddress(string address)
        {
            try
            {
                if (plc != null && plc.IsConnected)
                {
                    return plc.Read(address);
                }
            }
            catch (Exception ex)
            {
                AppLog.Plc.Error($"PLC读取地址[{address}]异常: {ex.Message}");
                TryReconnect();
            }

            return null;
        }

        public T ReadByAddress<T>(string address)
        {
            try
            {
                if (plc != null && plc.IsConnected)
                {
                    return (T)plc.Read(address);
                }
            }
            catch (Exception ex)
            {
                AppLog.Plc.Error($"PLC读取地址[{address}]异常: {ex.Message}");
                TryReconnect();
            }

            return default(T);
        }

        /// <summary>
        /// 按字节读取 PLC DB 数据块
        /// </summary>
        /// <param name="dbNumber">DB 块号，例如 800</param>
        /// <param name="startByte">起始偏移（字节）</param>
        /// <param name="length">读取长度（字节）</param>
        /// <returns>byte 数组，如果读取失败返回 null</returns>
        public byte[] ReadBytes(int dbNumber, int startByte, int length)
        {
            try
            {
                if (plc != null && plc.IsConnected)
                {

                    // 使用 S7.Net 的 ReadBytes 方法
                    return plc.ReadBytes(DataType.DataBlock, dbNumber, startByte, length);
                }
            }
            catch (PlcException ex)
            {
                AppLog.Plc.Error($"PLC按字节读取失败 DB{dbNumber}, offset={startByte}, length={length}: {ex.Message}");
                TryReconnect();
            }
            catch (Exception ex)
            {
                AppLog.Plc.Error($"PLC按字节读取异常 DB{dbNumber}, offset={startByte}, length={length}: {ex.Message}");
                TryReconnect();
            }

            return null;
        }

        /// <summary>
        /// 读取 Siemens STRING 类型
        /// </summary>
        /// <param name="dbNumber">DB 号，如 203</param>
        /// <param name="startByte">起始偏移，如 2</param>
        /// <param name="maxLen">STRING 最大长度，如 STRING[20] 就填 20</param>
        public string ReadString(int dbNumber, int startByte, int maxLen)
        {
            try
            {
                if (plc != null && plc.IsConnected)
                {
                    // 读取：最大长度 + 当前长度 + 内容
                    int totalLen = 2 + maxLen;

                    var bytes = plc.ReadBytes(DataType.DataBlock, dbNumber, startByte, totalLen);

                    if (bytes == null || bytes.Length < 2)
                        return string.Empty;

                    int currLen = bytes[1]; // 当前长度

                    if (currLen <= 0)
                        return string.Empty;

                    if (currLen > maxLen)
                        currLen = maxLen;

                    return System.Text.Encoding.ASCII.GetString(bytes, 2, currLen);
                }
            }
            catch (Exception ex)
            {
                AppLog.Plc.Error($"读取 STRING 失败 DB{dbNumber}, offset={startByte}: {ex.Message}");
                TryReconnect();
            }

            return string.Empty;
        }



        #region 数据采集
        /// <summary>
        /// 订阅指定点位，开启后台监听
        /// </summary>
        public void Subscribe(string address)
        {
            if (!subscribedAddresses.Contains(address))
                subscribedAddresses.Add(address);
        }

        /// <summary>
        /// 开始监听PLC点位变化
        /// </summary>
        public void StartListening(int intervalMs = 50)
        {
            listenerCts = new CancellationTokenSource();
            var token = listenerCts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (plc == null || !plc.IsConnected)
                        {
                            TryReconnect();
                        }
                        else
                        {
                            foreach (var address in subscribedAddresses)
                            {
                                object value = plc.Read(address);
                                AppLog.Plc.Info($"Subscribed Addresses {address} Success -> {value}");
                                if (!lastValues.ContainsKey(address) || !Equals(lastValues[address], value))
                                {
                                    lastValues[address] = value;
                                    OnValueChanged?.Invoke(this, (address, value));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TryReconnect();

                        AppLog.Plc.Error($"StartListening->{ex.Message}");
                    }

                    await Task.Delay(intervalMs, token);
                }
            }, token);
        }

        public void StopListening() => listenerCts?.Cancel();
        #endregion

        /// <summary>
        /// 停止心跳和关闭PLC连接
        /// </summary>
        public void Stop()
        {
            heartbeatTimer?.Stop();
            plc?.Close();
            AppLog.Plc.Info("关闭PLC通信");
        }

      
    }
}

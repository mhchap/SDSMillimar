
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MySqlX.XDevAPI.Common;
using NLog;
using SDSMillimar.Common;
using SDSMillimar.Dtos;
using SDSMillimar.Models;
using SDSMillimar.Services;
using SDSMillimar.Utils;
using SDSMillimar.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SDSMillimar.ViewModel
{
    public partial class TestItemViewModel : BaseViewModel
    {


        private ProcessDataRepository dataRepository;

        private ObservableCollection<ChannelModel> channelModels;

        public ObservableCollection<ChannelModel> ChannelModels
        {
            get { return channelModels; }
            set { channelModels = value; OnPropertyChanged(); }
        }


        private uint numModules;

        public uint NumModules
        {
            get { return numModules; }
            set { numModules = value; OnPropertyChanged(); }
        }

        private uint numChannels;

        public uint NumChannels
        {
            get { return numChannels; }
            set { numChannels = value; OnPropertyChanged(); }
        }


        #region 测量过程状态
        // 状态机
        private enum ProcessState
        {
            [Description("空闲")]
            Idle = 0,
            [Description("开始测量")]
            Started = 1,
            [Description("测量完成")]
            Completed = 2
        }
        private int currentProcessState = (int)ProcessState.Idle;

        public int CurrentProcessState
        {
            get { return currentProcessState; }
            set
            {
                currentProcessState = value;
                OnPropertyChanged();
                ProcessStateDesc = ((ProcessState)currentProcessState).GetDescription();
            }
        }


        private string processStateDesc;

        public string ProcessStateDesc
        {
            get { return processStateDesc; }
            set
            {
                processStateDesc = value;
                OnPropertyChanged();
            }
        }

        private enum PlcConnectType
        {
            [Description("未连接")]
            Unconnect = 0,
            [Description("已连接")]
            Connected = 1,
            [Description("连接失败")]
            ConnectionFail = 2
        }

        private int currentPlcConnectType = (int)PlcConnectType.Unconnect;

        public int CurrentPlcConnectType
        {
            get { return currentPlcConnectType; }
            set
            {
                currentPlcConnectType = value;
                OnPropertyChanged();
                PlcConnectTypeDesc = ((PlcConnectType)currentPlcConnectType).GetDescription();
            }
        }

        private Visibility toleranceColumnVisibility = Visibility.Visible;

        public Visibility ToleranceColumnVisibility
        {
            get { return toleranceColumnVisibility; }
            set { toleranceColumnVisibility = value; OnPropertyChanged(); }
        }


        private string plcConnectTypeDesc;

        public string PlcConnectTypeDesc
        {
            get { return plcConnectTypeDesc; }
            set
            {
                plcConnectTypeDesc = value;
                OnPropertyChanged();
            }
        }

        private string millimarStateDesc = "未连接";

        public string MillimarStateDesc
        {
            get { return millimarStateDesc; }
            set
            {
                millimarStateDesc = value;
                //if ("未连接".Equals(value))
                //{
                //    var result = S7NetController.Instance.WritePlcPoint("DB203.DBX66.4", true);
                //    if (result)
                //        AppLog.Production.Info("[PC Send] 发送测量设备离线信号(DB203.DBX66.4) 成功,1");
                //    else
                //        AppLog.Production.Info("[PC Send] 发送测量设备离线信号(DB203.DBX66.4) 失败");
                //}
                OnPropertyChanged();
            }
        }



        private enum ProcessType
        {
            [Description("测量")]
            Measure = 0,
            [Description("校准")]
            Calibration = 1,
            [Description("初始化")]
            Init = 2
        }

        private int currentProcessType = (int)ProcessType.Init;

        public int CurrentProcessType
        {
            get { return currentProcessType; }
            set
            {
                currentProcessType = value;
                OnPropertyChanged();
                ProcessTypeDesc = ((ProcessType)currentProcessType).GetDescription();
            }
        }

        private ObservableCollection<ProbeGroup> probeDatas = new ObservableCollection<ProbeGroup>
            {
                new ProbeGroup() { TitleA = "探针1", TitleB = "探针2", Ids = "1,2",MeasuredValueA=0,MeasuredValueB=0 },
                new ProbeGroup() {  TitleA = "探针3", TitleB = "探针4", Ids = "3,4",MeasuredValueA=0,MeasuredValueB=0},
                new ProbeGroup() { TitleA = "探针5", TitleB = "探针6", Ids = "5,6",MeasuredValueA=0,MeasuredValueB=0},
                new ProbeGroup() {  TitleA = "探针7", TitleB = "探针8", Ids ="7,8",MeasuredValueA=0,MeasuredValueB=0},
                new ProbeGroup() { TitleA = "探针9", TitleB = "探针10", Ids = "9,10",MeasuredValueA=0,MeasuredValueB=0},
            };

        public ObservableCollection<ProbeGroup> ProbeDatas
        {
            get { return probeDatas; }
            set { probeDatas = value; OnPropertyChanged(); }
        }


        private bool isAuto = false;

        public bool IsAuto
        {
            get { return isAuto; }
            set { isAuto = value; OnPropertyChanged(); }
        }



        private string processTypeDesc;

        public string ProcessTypeDesc
        {
            get { return processTypeDesc; }
            set
            {
                processTypeDesc = value;
                OnPropertyChanged();
            }
        }


        private enum ResultState
        {
            Idle = -2,
            Processing = -1,
            OK = 0,
            NG = 1
        }
        #endregion
        public bool N1700Init { get; set; } = true;
        public bool N1700InitS { get; set; } = false;
        private bool allOk = false;

        public bool AllOk
        {
            get { return allOk; }
            set { allOk = value; OnPropertyChanged(); }
        }

        private string plcSendBarcode;

        public string PlcSendBarcode
        {
            get { return plcSendBarcode; }
            set { plcSendBarcode = value; OnPropertyChanged(); }
        }

        private readonly WorkpieceAnalyzer workpieceAnalyzer = new WorkpieceAnalyzer();

        #region Command
        public ICommand StartTestCommand { get; set; }
        public ICommand StopTestCommand { get; set; }

        public ICommand ZeroDrivceCommand { get; set; }
        public ICommand ResetOffsetCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand JZCommand { get; set; }
        public ICommand CLCommand { get; set; }
        #endregion

        /// <summary>
        /// 测量点数据集合
        /// </summary>
        private List<MeasurementBuffer> measurementBuffers = new List<MeasurementBuffer>();

        private ObservableCollection<TechnologyParam> processDatas;

        public ObservableCollection<TechnologyParam> ProcessDatas
        {
            get { return processDatas; }
            set { processDatas = value; OnPropertyChanged(); }
        }

        public string PlcSendProducted { get; set; }
        public bool IsCalibration { get; set; } = false;
        public bool IsMeasure { get; set; } = false;

        private TechnologyDto _currentJZtDto;
        private readonly Random _pullRandom = new Random();
        private readonly object _pullRandomLock = new object();

        public TestItemViewModel()
        {
            try
            {
                MillN1700Helper.Instance.Init();
                MillN1700Helper.Instance.CommunicationChanged += Instance_CommunicationChanged;
                MillN1700Helper.Instance.ChannelsChanged += Instance_ChannelsChanged;
                MillN1700Helper.Instance.ModulesChanged += Instance_ModulesChanged;
                // ⚠️ 一定要在 UI 线程 new
                //_uiContext = SynchronizationContext.Current;
                StartTestCommand = new RelayCommand(() =>
                {
                    SetBeginFrame();
                    MillN1700Helper.Instance.N1700StartContinuousRequestAllData();
                });
                StopTestCommand = new RelayCommand(() =>
                {
                    MillN1700Helper.Instance.N1700StopContinuousRequestAllData();
                });

                ZeroDrivceCommand = new RelayCommand(async () =>
                {
                    await ZeroDrivce();
                }, IsCan);
                LoadedCommand = new RelayCommand(Loaded);
                JZCommand = new RelayCommand(JZ);
                CLCommand = new RelayCommand(CL);
                //ResetOffsetCommand = new RelayCommand(ResetOffset);
                dataRepository = new ProcessDataRepository();
                Messenger.Register("ModelSelection", ModelSelection);
                Messenger.Register<bool>("ModelSelectionShow", ModelSelectionShow);
                Messenger.Register<bool>("DataBatchReceived", DataBatchReceived);
                S7NetController.Instance.OnValueChanged += async (s, e) =>
                {
                    switch (e.Address)
                    {
                        case "DB203.DBX0.0":
                            if ((bool)e.Value) MeasurementBegins();
                            else await MeasurementCompleted();
                            break;
                        case "DB203.DBX0.1": // 校准模式
                            if ((bool)e.Value)
                            {
                                AppLog.Production.Info("[PLC Send] 校准模式(DB203.DBX0.1)");
                                CurrentProcessType = (int)ProcessType.Calibration;
                                Refresh(2);
                            }
                            break;
                        case "DB203.DBX0.2": // 测量模式
                            if ((bool)e.Value)
                            {
                                AppLog.Production.Info("[PLC Send] 测量模式(DB203.DBX0.2)");
                                CurrentProcessType = (int)ProcessType.Measure;
                                Refresh(1);
                            }
                            break;
                        case "DB203.DBX0.3":
                            IsAuto = (bool)e.Value;
                            if (!(bool)e.Value)
                            {
                                AppLog.Production.Info("[PLC Send] 手动模式(DB203.DBX0.3)");
                            }
                            else
                            {
                                AppLog.Production.Info("[PLC Send] 自动模式(DB203.DBX0.3)");
                                if (IsAuto)
                                {
                                    if (CurrentProcessType == (int)ProcessType.Measure)
                                        CL();
                                    if (CurrentProcessType == (int)ProcessType.Calibration)
                                        JZ();
                                }
                            }
                            break;
                        case "DB203.DBX0.4":
                            var restart = (bool)e.Value;
                            AppLog.Production.Info($"[PLC Send] 复位(DB203.DBX0.4) {restart}");
                            //if (!(bool)e.Value)
                            //{

                            //}
                            //else
                            //{

                            //}
                            break;
                        case "DB203.DBD2.0":
                            PlcSendBarcode = S7NetController.Instance.ReadString(203, 2, 40);
                            AppLog.Production.Info($"[PLC Send] 工件条码(DB203.DBD2.0) {PlcSendBarcode}");
                            break; //工件条码
                        case "DB203.DBD44.0":
                            PlcSendProducted = S7NetController.Instance.ReadString(203, 44, 20);
                            AppLog.Production.Info($"[PLC Send] 产品号->{PlcSendProducted}");
                            break; //零件类型
                    }
                };
                S7NetController.Instance.StopListening();
                S7NetController.Instance.StartListening();
                GlobalSession.Instance.IsTestItem = true;
                //InitLineSeries();
            }
            catch (Exception ex)
            {
                AppLog.Production.Error($"error->{ex.Message}");
            }

        }

        private void ModelSelectionShow(bool t)
        {
            var modelSelectionView = new ModelSelectionView();
            modelSelectionView.Owner = Application.Current.MainWindow;
            modelSelectionView.ShowDialog();
        }

        public void DataBatchReceived(bool t)
        {
            if (t)
            {

                MillN1700Helper.Instance.DataBatchReceived += Instance_DataBatchReceived;
            }
            else
            {
                MillN1700Helper.Instance.DataBatchReceived -= Instance_DataBatchReceived;
            }
        }

        private void Instance_ModulesChanged(uint obj)
        {
            NumModules = obj;
            AppLog.Production.Info($"Instance_ModulesChanged->{NumModules}");
            if (NumModules == 0)
            {
                var result = S7NetController.Instance.WritePlcPoint("DB203.DBX66.4", true);
                if (result)
                    AppLog.Production.Info("[PC Send] 发送测量设备离线信号(DB203.DBX66.4) 成功,1");
                else
                    AppLog.Production.Info("[PC Send] 发送测量设备离线信号(DB203.DBX66.4) 失败");
            }
            else
            {
                MillimarStateDesc = "已连接";
                var result = S7NetController.Instance.WritePlcPoint("DB203.DBX66.4", false);
                if (result)
                    AppLog.Production.Info("[PC Send] 发送测量设备在线信号(DB203.DBX66.4) 成功,1");
                else
                    AppLog.Production.Info("[PC Send] 发送测量设备在线信号(DB203.DBX66.4) 失败");
            }
        }

        private void Instance_ChannelsChanged(uint obj)
        {
            NumChannels = obj;
            AppLog.Production.Info($"Instance_ChannelsChanged->{NumChannels}");
            if (NumChannels == 0)
            {
                var result = S7NetController.Instance.WritePlcPoint("DB203.DBX66.4", true);
                if (result)
                    AppLog.Production.Info("[PC Send] 发送测量设备离线信号(DB203.DBX66.4) 成功,1");
                else
                    AppLog.Production.Info("[PC Send] 发送测量设备离线信号(DB203.DBX66.4) 失败");
            }
            else
            {
                MillimarStateDesc = "已连接";
                var result = S7NetController.Instance.WritePlcPoint("DB203.DBX66.4", false);
                if (result)
                    AppLog.Production.Info("[PC Send] 发送测量设备在线信号(DB203.DBX66.4) 成功,1");
                else
                    AppLog.Production.Info("[PC Send] 发送测量设备在线信号(DB203.DBX66.4) 失败");
            }
        }

        private void Instance_CommunicationChanged(bool obj)
        {
            AppLog.Production.Info($"Instance_CommunicationChanged->{obj}");
            //throw new NotImplementedException();
            MillimarStateDesc = "已连接";
            var result = S7NetController.Instance.WritePlcPoint("DB203.DBX66.4", false);
            if (result)
                AppLog.Production.Info("[PC Send] 发送测量设备在线信号(DB203.DBX66.4) 成功,1");
            else
                AppLog.Production.Info("[PC Send] 发送测量设备在线信号(DB203.DBX66.4) 失败");
        }

        /// <summary>
        /// 选型
        /// </summary>
        /// <param name="t"></param>
        private void ModelSelection(object t)
        {
            var _currentTechnologyParamDtos = GlobalSession.Instance.CurrentTechnologyParamDtos;
            if (_currentTechnologyParamDtos != null && _currentTechnologyParamDtos.Count > 0)
            {
                var distinctTechs = _currentTechnologyParamDtos
                                 .GroupBy(x => x.DeviceIds)
                                 .Select(g => g.First())
                                 .ToList();
                //var usedProbeIds = distinctTechs
                //                 .SelectMany(t1 => t1.DeviceIds)
                //                 .Select(ids => ids)
                //                 .ToHashSet();
                var usedProbeIds = distinctTechs
                              .Select(t1 => t1.DeviceIds)
                              .ToHashSet();
                ProbeDatas = new ObservableCollection<ProbeGroup>
            {
                new ProbeGroup() { TitleA = "探针1", TitleB = "探针2", Ids = "1,2",MeasuredValueA=0,MeasuredValueB=0 },
                new ProbeGroup() {  TitleA = "探针3", TitleB = "探针4", Ids = "3,4",MeasuredValueA=0,MeasuredValueB=0},
                new ProbeGroup() { TitleA = "探针5", TitleB = "探针6", Ids = "5,6",MeasuredValueA=0,MeasuredValueB=0},
                new ProbeGroup() {  TitleA = "探针7", TitleB = "探针8", Ids ="7,8",MeasuredValueA=0,MeasuredValueB=0},
                new ProbeGroup() { TitleA = "探针9", TitleB = "探针10", Ids = "9,10",MeasuredValueA=0,MeasuredValueB=0},
            };
                var filteredProbeDatas = ProbeDatas
                                .Where(p => usedProbeIds.Contains(p.Ids))
                                .ToList();
                if (filteredProbeDatas.Count == 0)
                {
                    MessageBox.Show(
                   $"未选择探针",
                   "工艺配方有误",
                   MessageBoxButton.OK,
                   MessageBoxImage.Error);
                }
                ProbeDatas.Clear();
                foreach (var item in filteredProbeDatas)
                {
                    ProbeDatas.Add(item);
                }
            }
            if (IsAuto)
            {
                if (CurrentProcessType == (int)ProcessType.Measure)
                    CL();
                if (CurrentProcessType == (int)ProcessType.Calibration)
                    JZ();
            }
            ToleranceColumnVisibility = GlobalSession.Instance.CurrentSelectedTechnologyDto.TechnologyType == 2 ? Visibility.Collapsed : Visibility.Visible;
            AppLog.Production.Info($"ProbeDatas Count -> {ProbeDatas.Count}");
            _currentJZtDto = GlobalSession.Instance.CurrentSelectedTechnologyDtos.Where(x => x.TechnologyType == 2).FirstOrDefault();
        }

        /// <summary>
        /// 校准
        /// </summary>
        private void JZ()
        {
            CurrentProcessType = (int)ProcessType.Calibration;
            Refresh(2);

        }

        /// <summary>
        /// 测量
        /// </summary>
        private void CL()
        {
            CurrentProcessType = (int)ProcessType.Measure;
            Refresh(1);
        }


        public bool IsCan()
        {
            if (CurrentProcessState == (int)ProcessState.Idle)
                return true;
            return false;
        }


        private void Loaded()
        {
            var vm = new ModelSelectionView();
            vm.Owner = Application.Current.MainWindow;
            vm.ShowDialog();
            GetMeasurements();
        }

        private void Refresh(int type)
        {
            if (GlobalSession.Instance.CurrentSelectedTechnologyDtos != null)
            {
                var pDto = GlobalSession.Instance.CurrentSelectedTechnologyDtos?.Where(x => x.TechnologyType == type).First();
                GlobalSession.Instance.CurrentSelectedTechnologyDto = pDto;
                GlobalSession.Instance.CurrentTechnologyParamDtos = pDto.Params;
                GetMeasurements();
            }
            ToleranceColumnVisibility = type == 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        #region Millimar
        /// <summary>
        /// 恢复偏移量
        /// </summary>
        private async Task ResetOffset()
        {
            for (int i = 1; i <= ChannelModels?.Count; i++)
            {
                //MillN1700Helper.Instance.ZeroChannel(ChannelModels[i].ChannelIdx);
                MillN1700Helper.Instance.N1700SetOffsetMM((uint)i, 0);
                await Task.Delay(100);
            }
        }

        /// <summary>
        /// 根据当前产品工艺的测量创建数据点集合
        /// </summary>
        private void GetMeasurements()
        {
            measurementBuffers?.Clear();
            for (int i = 0; i < GlobalSession.Instance.CurrentTechnologyParamDtos?.Count; i++)
            {
                MeasurementBuffer measurementBuffer = new MeasurementBuffer();
                measurementBuffers.Add(measurementBuffer);
            }
        }

        /// <summary>
        /// 回零流程
        /// </summary>
        /// <returns></returns>
        private async Task ZeroDrivce()
        {
            //await Task.Run(async () =>
            //{
            MillN1700Helper.Instance.N1700StopContinuousRequestAllData();
            SetBeginFrame();
            AppLog.Production.Info("开始回零");
            await Task.Delay(300);
            await ResetOffset();
            //await Task.Delay(300);
            //N1700Lib.N1700RequestAllData(0);
            //await Task.Delay(300);
            //ZeroAllChannels();
            //await Task.Delay(300);
            //N1700Lib.N1700RequestAllData(0);
            await Task.Delay(300);
            N1700Lib.N1700RequestAllData(0);
            await Task.Delay(300);
            await ZeroAllChannels1();
            await Task.Delay(300);
            N1700Lib.N1700RequestAllData(0);
            AppLog.Production.Info("结束回零");
            //}).ConfigureAwait(false);
        }

        private readonly object _dataLock = new object();
        public bool ZeroAllChannels()
        {
            lock (_dataLock)
            {
                if (ChannelModels == null)
                    return false;
                int channelCount = ChannelModels.Count;
                // 2️⃣ 冻结所有通道的值（非常关键）
                double[] snapshot = new double[channelCount];
                for (int i = 0; i < channelCount; i++)
                {
                    snapshot[i] = ChannelModels[i].Value;
                }

                // 3️⃣ 统一设置 Offset
                for (int i = 0; i < channelCount; i++)
                {
                    int ret = N1700Lib.N1700SetOffsetMM((uint)i, (float)snapshot[i]);
                    if (ret != N1700Lib.N1700_SUCCESS)
                    {
                        return false;
                    }

                }
                return true;
            }
        }

        public void RefreshAllChannelsData()
        {
            N1700Lib.N1700RequestAllData(0);
        }

        public async Task<bool> ZeroAllChannels1()
        {
            //lock (_dataLock)
            //{
            if (ChannelModels == null)
                return false;
            int channelCount = ChannelModels.Count;
            //// 2️⃣ 冻结所有通道的值（非常关键）
            //double[] snapshot = new double[channelCount];
            //for (int i = 0; i < channelCount; i++)
            //{
            //    snapshot[i] = ChannelModels[i].Value;
            //}
            // 3️⃣ 统一设置 Offset
            for (int i = 1; i < channelCount; i++)
            {
                int ret = N1700Lib.N1700SetOffsetMM((uint)i, (float)ChannelModels[i].Value);
                if (ret != N1700Lib.N1700_SUCCESS)
                {
                    return false;
                }
                await Task.Delay(100);
            }
            return true;
            //}
        }

        /// <summary>
        /// 可以开始新的采集
        /// </summary>
        private void SetBeginFrame()
        {

            for (int i = 0; i < measurementBuffers.Count; i++)
            {
                //GlobalSession.Instance.CurrentTechnologyParamDtos[i].DeviceIds.Split(',').Length
                measurementBuffers[i].BeginFrame();
                GlobalSession.Instance.CurrentTechnologyParamDtos[i].MeasuredValue = 0;
                GlobalSession.Instance.CurrentTechnologyParamDtos[i].IsMeasured = false;
            }
        }

        /// <summary>
        /// 结束测量
        /// </summary>
        private void SetEndFrame()
        {
            for (int i = 0; i < measurementBuffers.Count; i++)
            {
                measurementBuffers[i].EndFrame();
            }
        }
        // 门禁：是否正在处理
        private int _isProcessing = 0;


        private ObservableCollection<ChannelModel> _latestData;
        // 采样周期（毫秒）——你可以放到 app.config
        private int _sampleIntervalMs = GlobalSession.Instance.SampleIntervalMs;

        // 上一次进入计算的时间戳
        private long _lastSampleTick = 0;

        // Stopwatch 更精确（强烈推荐）
        private static readonly Stopwatch _sw = Stopwatch.StartNew();

        private void Instance_DataBatchReceived(ObservableCollection<ChannelModel> obj)
        {
            // ===== 采样频率控制 =====
            long now = _sw.ElapsedMilliseconds;
            long last = Interlocked.Read(ref _lastSampleTick);

            if (now - last < _sampleIntervalMs)
            {
                // 不到采样周期，直接丢弃
                return;
            }

            // 更新时间戳（CAS，防止并发）
            if (Interlocked.CompareExchange(ref _lastSampleTick, now, last) != last)
                return;

            ChannelModels = obj;
            _latestData = obj;
            if (N1700Init)
            {
                ObservableCollection<ProbeItem> probeItems = new ObservableCollection<ProbeItem>();
                for (int i = 1; i < obj.Count; i++)
                {

                    probeItems.Add(new ProbeItem() { Direction = i % 2 == 0 ? -1 : 1, Title = $"通道-{channelModels[i].ChannelIdx}", Id = channelModels[i].ChannelIdx, GridColumn = 0, GridRow = 0, IsSelected = false });
                }
                GlobalSession.Instance.Probes = probeItems;
            }

            // 如果已经在处理，就不再启动新一轮
            //if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0)
            //    return;

            //Task.Run(ProcessLoop);
            ProcessMeasurement(obj);
        }

        private void ProcessLoop()
        {
            try
            {
                while (true)
                {
                    // 取出最新数据
                    var data = Interlocked.Exchange(ref _latestData, null);

                    // 没新数据，退出
                    if (data == null)
                        break;

                    // 真正处理
                    SafeProcessMeasurement(data);
                }
            }
            catch (Exception ex)
            {
                AppLog.Production.Error($"ProcessLoop -> {ex}");
            }
            finally
            {
                // 释放门禁
                Interlocked.Exchange(ref _isProcessing, 0);

                // 防止刚释放就又来了数据
                if (_latestData != null &&
                    Interlocked.CompareExchange(ref _isProcessing, 1, 0) == 0)
                {
                    Task.Run(ProcessLoop);
                }
            }
        }

        private void SafeProcessMeasurement(ObservableCollection<ChannelModel> obj)
        {
            try
            {
                if (obj.Count < ProbeDatas.Count)
                    return;

                // 快照，防止中途变化
                var channelSnapshot = obj.ToArray();
                var techSnapshot = GlobalSession.Instance.CurrentTechnologyParamDtos.ToList();
                var bdSnapshot = GlobalSession.Instance.BdDatas.ToArray();

                for (int i = 0; i < measurementBuffers.Count; i++)
                {
                    var buffer = measurementBuffers[i];
                    var tech = techSnapshot[i];

                    var ids = tech.DeviceIds
                        .Split(',')
                        .Select(x => int.Parse(x.Trim()))
                        .ToArray();

                    int aIndex = ids[0];
                    int bIndex = ids[1];

                    double av = channelSnapshot[aIndex].Value - bdSnapshot[aIndex - 1];
                    double bv = channelSnapshot[bIndex].Value - bdSnapshot[bIndex - 1];

                    // 阈值过滤（第一道防抖）
                    //if (tech.FilterValue > 0 &&
                    //    (Math.Abs(av) > tech.FilterValue || Math.Abs(bv) > tech.FilterValue))
                    //{
                    //    AppLog.FilterData.Info(
                    //        $"过滤 {tech.ParamValue} A:{av:F4} B:{bv:F4}"
                    //    );
                    //    continue;
                    //}

                    buffer.AddPoint(av, bv);

                    // UI 更新
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        tech.RealValueA = av;
                        tech.RealValueB = bv;

                        if (i < ProbeDatas.Count)
                        {
                            ProbeDatas[i].MeasuredValueA = av;
                            ProbeDatas[i].MeasuredValueB = bv;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                AppLog.Production.Error($"SafeProcessMeasurement -> {ex}");
            }
        }

        private readonly Dictionary<int, int> _overLimitCount = new Dictionary<int, int>();

        //private void CheckOverLimit(int channelIndex, double value)
        //{
        //    if (!_overLimitCount.ContainsKey(channelIndex))
        //        _overLimitCount[channelIndex] = 0;

        //    if (Math.Abs(value) > 1.5)
        //    {
        //        _overLimitCount[channelIndex]++;

        //        // 连续10次超限直接抛异常
        //        if (_overLimitCount[channelIndex] >= 10)
        //        {
        //            _overLimitCount[channelIndex] = 0;
        //            MessageBox.Show($"探针 【{channelIndex}】调整异常，当前值：{value}", "采集异常", MessageBoxButton.OK, MessageBoxImage.Error);
        //            MeasurementResult(false);
        //            AllOk = false;
        //            throw new Exception(
        //                $"探针 【{channelIndex}】调整异常，当前值：{value}");
        //        }
        //    }
        //    else
        //    {
        //        // 恢复正常后清零
        //        _overLimitCount[channelIndex] = 0;
        //    }
        //}

        /// <summary>
        /// 是否已经触发超限异常
        /// 0 = 正常
        /// 1 = 已触发
        /// </summary>
        private int _overLimitTriggered = 0;

        /// <summary>
        /// 外部读取（PLC业务逻辑用）
        /// </summary>
        public bool IsOverLimitTriggered =>
            Volatile.Read(ref _overLimitTriggered) == 1;

        private void CheckOverLimit(int channelIndex, double value)
        {
            // 已经触发异常
            if (Volatile.Read(ref _overLimitTriggered) == 1)
                return;

            if (!_overLimitCount.ContainsKey(channelIndex))
                _overLimitCount[channelIndex] = 0;

            if (Math.Abs(value) > GlobalSession.Instance.OverNum)
            {
                _overLimitCount[channelIndex]++;

                if (_overLimitCount[channelIndex] >= 10)
                {
                    _overLimitCount[channelIndex] = 0;

                    // 只允许一个线程进入异常流程
                    if (Interlocked.CompareExchange(
                            ref _overLimitTriggered,
                            1,
                            0) != 0)
                    {
                        return;
                    }

                    //AllOk = false;

                    //MeasurementResult(false);


                    var result = S7NetController.Instance.WritePlcPoint("DB203.DBX66.4", true);
                    if (result)
                        AppLog.Production.Info("[PC Send 采集异常] 发送测量设备离线信号(DB203.DBX66.4) 成功,1");
                    else
                        AppLog.Production.Info("[PC Send 采集异常] 发送测量设备离线信号(DB203.DBX66.4) 失败");

                    MessageBox.Show(
                        $"探针 【{channelIndex}】调整异常，当前值：{value}",
                        "采集异常",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    throw new Exception(
                        $"探针 【{channelIndex}】调整异常，当前值：{value}");
                }
            }
            else
            {
                _overLimitCount[channelIndex] = 0;
            }
        }

        private void ProcessMeasurement(ObservableCollection<ChannelModel> obj)
        {
            try
            {
                //if (obj.Count <= ProbeDatas.Count)
                //{
                //    MessageBox.Show("当前采集设备数量与工艺配置的采集设备数量不一致", "采集异常", MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}
                AppLog.FilterData.Info($"ObservableCollection<ChannelModel> -> {string.Join(Environment.NewLine, obj)}");
                //if (obj.Count <= ProbeDatas.Count) throw new Exception("当前采集设备数量与工艺配置的采集设备数量不一致");

                for (int i = 0; i < measurementBuffers.Count; i++)
                {

                    var buffer = measurementBuffers[i];
                    var tech = GlobalSession.Instance.CurrentTechnologyParamDtos[i];
                    var ids = tech.DeviceIds
                        .Split(',')
                        .Select(x => int.Parse(x.Trim()))
                        .ToArray();
                    int aIndex = ids[0];
                    int bIndex = ids[1];


                    // 原始值
                    double rawA = obj[aIndex].Value;
                    double rawB = obj[bIndex].Value;

                    // 持续超限检测
                    CheckOverLimit(aIndex, rawA);
                    CheckOverLimit(bIndex, rawB);



                    double av = obj[aIndex].Value - GlobalSession.Instance.BdDatas[aIndex - 1];
                    double bv = obj[bIndex].Value - GlobalSession.Instance.BdDatas[bIndex - 1];
                    //if (tech.FilterValue != 0 && (Math.Abs(av) > tech.FilterValue || Math.Abs(bv) > tech.FilterValue))
                    //{
                    //    if (Math.Abs(av) > tech.FilterValue)
                    //        AppLog.FilterData.Info($"过滤数据{tech.ParamValue}-{av}");
                    //    if (Math.Abs(bv) > tech.FilterValue)
                    //        AppLog.FilterData.Info($"过滤数据{tech.ParamValue}-{bv}");
                    //}
                    //else
                    buffer.AddPoint(av, bv);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        tech.RealValueA = av;
                        tech.RealValueB = bv;
                        if (i < ProbeDatas.Count)
                        {
                            ProbeDatas[i].MeasuredValueA = av;
                            ProbeDatas[i].MeasuredValueB = bv;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                AppLog.Production.Error($"ProcessMeasurement-> {ex.Message}");

                return;
            }
        }
        #endregion



        #region 测量流程方法

        private void StartTest()
        {
            SetBeginFrame();
            N1700Init = false;

            // 重置测量状态
            foreach (var item in GlobalSession.Instance.CurrentTechnologyParamDtos)
            {
                item.Status = null;
                item.RealValueA = 0;
                item.RealValueB = 0;
            }
            ;
            MillN1700Helper.Instance.N1700StartContinuousRequestAllData();
        }

        public int CurrentMeasureCount { get; set; } = 0;
        public bool HasSaved { get; set; } = false;   // 防止重复保存
        private double _CompensationValue = 0.0;
        // 每个测量项保存第一次合格跳动结果，后续拉值固定以该基准为中心，避免逐次漂移。
        private readonly Dictionary<int, double> _firstRunoutResults = new Dictionary<int, double>();
        private async Task StopTest()
        {
            try
            {
                // 1️⃣ 停止采集
                MillN1700Helper.Instance.N1700StopContinuousRequestAllData();
                int bufferCount = measurementBuffers.Count;
                int dtoCount = GlobalSession.Instance.CurrentTechnologyParamDtos.Count;
                int count = Math.Min(bufferCount, dtoCount);
                // 2️⃣ 结束帧
                //for (int i = 0; i < count; i++)
                //{
                //    measurementBuffers[i].EndFrame();
                //}
                SetEndFrame();
                // 3️⃣ 先算结果（后台线程）
                var results = new List<(int index, double value)>();
                List<ProcessData> processDatas = new List<ProcessData>();
                for (int i = 0; i < count; i++)
                {
                    var buffer = measurementBuffers[i];
                    var dto = GlobalSession.Instance.CurrentTechnologyParamDtos[i];
                    var points = buffer.GetAllFrames();
                    ProcessData processData = new ProcessData();
                    double result = 0;
                    processData.Barcode = S7NetController.Instance.ReadString(203, 2, 40);
                    processData.TargetValue = dto.TargetValue;
                    processData.LowerTolerance = dto.LowerTolerance;
                    processData.UpperTolerance = dto.UpperTolerance;
                    processData.MeasureType = dto.MeasureType;
                    processData.ParamValue = dto.ParamValue;

                    processData.ParamName = dto.ParamName;
                    processData.ProductName = GlobalSession.Instance.SelectedProduct.ProductName;
                    processData.ProductId = GlobalSession.Instance.SelectedProduct.Id;
                    processData.TechnologyId = GlobalSession.Instance.CurrentSelectedTechnologyDto.TechnologyID;
                    processData.TechnologyName = GlobalSession.Instance.CurrentSelectedTechnologyDto.TechnologyName;

                    try
                    {
                        if (points[0].APoints.Count > 0)
                        {
                            //AppLog.ProcessData.Info($"{dto.ParamValue} 原始数据A -> {string.Join(",", points[0].APoints.Select(p => p.Value.ToString("F5")))}");
                            //AppLog.ProcessData.Info($"{dto.ParamValue} 原始数据B -> {string.Join(",", points[0].BPoints.Select(p => p.Value.ToString("F5")))}");
                            switch (dto.MeasureType)
                            {
                                case 1:
                                    GlobalSession.Instance.CurrentTechnologyParamDtos[i].CUValue = dto.TargetValue + dto.UpperTolerance;
                                    GlobalSession.Instance.CurrentTechnologyParamDtos[i].CLValue = dto.TargetValue + dto.LowerTolerance;
                                    if (CurrentProcessType == (int)ProcessType.Calibration)
                                    {
                                        dto.MeasureCount++;

                                        // 公差中值
                                        var baseValue = ((dto.TargetValue + dto.LowerTolerance) +
                                                         (dto.TargetValue + dto.UpperTolerance)) / 2;

                                        result = workpieceAnalyzer.CalculateDiameter(points[0].APoints.Select(p => p.Value).ToArray(), points[0].BPoints.Select(p => p.Value).ToArray(), baseValue, dto.CompensationValue, dto.ParamValue);

                                        if (CheckValue(dto, result))
                                        {
                                            AppLog.Production.Info("测量合格");
                                            dto.MeasureCount = 0;
                                            break;
                                        }
                                        // 35.05   35.06 -0.01
                                        double diff = baseValue - result;
                                        double absDiff = Math.Abs(diff);
                                        // 自适应补偿系数
                                        double k = 0.0;
                                        double compensation = 0.0;
                                        if (GlobalSession.Instance.LearningRate == 1)
                                        {
                                            if (absDiff > 0.01)
                                                k = 0.99;
                                            else if (absDiff > 0.005)
                                                k = 0.95;
                                            else if (absDiff > 0.002)
                                                k = 0.90;
                                            else
                                                k = 0.85;

                                            compensation = diff * k;

                                        }
                                        else
                                        {
                                            compensation = diff;
                                        }
                                        // 单次补偿限幅 ±0.02
                                        compensation = Math.Max(-0.02, Math.Min(0.02, compensation));
                                        // 总补偿限幅 ±0.05
                                        dto.CompensationValue = Math.Max(-0.05, Math.Min(0.05, dto.CompensationValue + compensation));

                                        GlobalSession.Instance.CurrentTechnologyParamDtos[i].TestValue = result - baseValue;
                                        AppLog.Production.Info($"{dto.ParamName}->偏差:{diff} 系数:{k} 本次补偿:{compensation} 当前补偿:{dto.CompensationValue}");
                                    }
                                    else
                                    {
                                        var jzDto = _currentJZtDto.Params[i];
                                        // 中位值
                                        var _mDiameter = ((dto.TargetValue + dto.LowerTolerance) + (dto.TargetValue + dto.UpperTolerance)) / 2;
                                        var _jzmDiameter = ((jzDto.TargetValue + jzDto.LowerTolerance) + (jzDto.TargetValue + jzDto.UpperTolerance)) / 2;
                                        //// 中位值 - (中位值 - 校准目标值中位值 + 补偿值)
                                        var _compensationValue = (_mDiameter - _jzmDiameter + jzDto.CompensationValue);
                                        var MDiameter = 0.0;
                                        if (GlobalSession.Instance.CalibrationDiameter == 1)
                                        {
                                            MDiameter = _jzmDiameter;
                                        }
                                        else
                                        {
                                            MDiameter = _mDiameter;
                                        }
                                        AppLog.Production.Info($"{dto.ParamName}目标值->{MDiameter}->{jzDto.CompensationValue}");
                                        result = workpieceAnalyzer.CalculateDiameter(points[0].APoints.Select(p => p.Value).ToArray(), points[0].BPoints.Select(p => p.Value).ToArray(), MDiameter, jzDto.CompensationValue, dto.ParamValue);
                                        GlobalSession.Instance.CurrentTechnologyParamDtos[i].TestValue = result - _mDiameter;
                                        // 当合格件时，强制把测量件拉到校准件1μ内
                                        if (jzDto.MeasuredValue != 0 && GlobalSession.Instance.IsPull == 1 && CheckValue(dto, result))
                                        {

                                            AppLog.Production.Info($"{dto.ParamName} JZ->{jzDto.MeasuredValue}->{result}");
                                            result = PullBToWithin1Um(jzDto.MeasuredValue, result);
                                            AppLog.Production.Info($"{dto.ParamName} JZ->{jzDto.MeasuredValue}->{result}");
                                        }
                                    }
                                    break;
                                case 2:
                                    result = workpieceAnalyzer.CalculateRoundness(points[0].APoints.Select(p => p.Value).ToArray(), points[0].BPoints.Select(p => p.Value).ToArray(), dto.ParamValue);
                                    GlobalSession.Instance.CurrentTechnologyParamDtos[i].TestValue = result;
                                    break;
                                case 3:
                                    result = TwoProbeMeasurementAlgorithms
                                    .CalcCylindricity(points);
                                    break;
                                case 4:
                                    result = workpieceAnalyzer.CalculateRunout(points[0].APoints.Select(p => p.Value).ToArray(), dto.ParamValue);

                                    if (GlobalSession.Instance.IsPull == 1 && CheckValue(dto, result))
                                    {
                                        // 第一次进入：直接使用并记录当前 result 作为固定基准。
                                        if (!_firstRunoutResults.TryGetValue(i, out double firstResult))
                                        {
                                            _firstRunoutResults[i] = result;
                                        }
                                        else
                                        {
                                            // 后续进入：确保新 result 在第一次结果 ± Pull4Tolerance 内。
                                            result = Pull4BToWithin1Um(firstResult, result);
                                        }
                                    }

                                    GlobalSession.Instance.CurrentTechnologyParamDtos[i].TestValue = result;
                                    
                                    break;
                            }


                        }

                        AppLog.Production.Info($"{dto.ParamValue}->{result}");
                        processData.MeasureValue = Math.Round(result, 5, MidpointRounding.AwayFromZero);
                        //AppLog.Production.Info($"{dto.ParamValue}->{MeasurementBuffer.GetFramePointsCsv(points[0])}");
                        // ⭐⭐⭐ 关键：单项判定 ⭐⭐⭐
                        bool itemOk = points[0].APoints.Count == 0 ? false : CheckValue(dto, result);



                        if (IsOverLimitTriggered)
                        {
                            var _index = _overLimitCount.Keys.First();
                            if (dto.DeviceIds.Contains(_index.ToString()))
                            {
                                itemOk = false;
                                AppLog.Production.Error($"StopTest -> 采集异常 _index={_index} -> 直接判定 itemOk = false");
                            }
                        }

                        processData.Status = itemOk;
                        GlobalSession.Instance.CurrentTechnologyParamDtos[i].Status = itemOk;
                        GlobalSession.Instance.CurrentTechnologyParamDtos[i].IsMeasured = true;
                        results.Add((i, Math.Round(result, 4)));
                        processDatas.Add(processData);
                        //MeasurementResult measurementResult = workpieceAnalyzer.Process(points[0].APoints.Select(p => p.Value).ToArray(), points[0].BPoints.Select(p => p.Value).ToArray(), _currentJZtDto.Params[i].TargetValue);
                        //AppLog.Production.Info(measurementResult.ToString());
                    }
                    catch (Exception ex)
                    {
                        AppLog.Production.Error($"StopTest -> 测量计算异常 index={i} -> {ex.Message}");
                    }
                }

                // 4️⃣ 一次性 UI 更新（关键）
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var r in results)
                    {
                        if (r.index >= GlobalSession.Instance.CurrentTechnologyParamDtos.Count)
                            continue;

                        GlobalSession.Instance
                            .CurrentTechnologyParamDtos[r.index]
                            .MeasuredValue = r.value;

                    }
                    //UpdateLineAll(results);
                });


                CurrentMeasureCount++;
                if (IsOverLimitTriggered)
                {


                    AllOk = false;
                }
                else
                    AllOk = processDatas.Count > 0 && processDatas.All(x => x.Status);
                AppLog.Production.Info($"工件判定:{AllOk}");


                bool shouldSave = false;


                if (AllOk || (CurrentProcessType == (int)ProcessType.Calibration && CurrentMeasureCount >= GlobalSession.Instance.MaxMeasuredCount) || (CurrentProcessType == (int)ProcessType.Measure && CurrentMeasureCount >= GlobalSession.Instance.MaxTestMeasuredCount))
                {
                    shouldSave = true;
                }

                try
                {
                    if (shouldSave && !HasSaved)
                    {
                        HasSaved = true;

                        try
                        {
                            AppLog.Production.Info("保存数据中");
                            await dataRepository.AddBatchAsync(processDatas);
                            AppLog.Production.Info("保存数据成功");

                        }
                        catch (Exception ex)
                        {
                            AppLog.Production.Error($"保存数据失败->{ex.Message}");
                        }

                        CurrentMeasureCount = 0;
                        HasSaved = false;
                    }
                }
                catch (Exception ex)
                {
                    AppLog.Production.Info("保存数据失败");
                    AppLog.Production.Error($"保存数据失败->{ex.Message}");
                }


                // 5️⃣ UI 更新完成后，再 Clear
                for (int i = 0; i < count; i++)
                {
                    measurementBuffers[i].Clear();
                }

            }
            catch (Exception ex)
            {
                AllOk = false; // 算异常，直接判 NG
                AppLog.Production.Error($"StopTest 异常", ex);
            }
            finally
            {
                MeasurementResult(AllOk);
                // 清除报警状态
                Interlocked.Exchange(ref _overLimitTriggered, 0);

                // 清空连续超限计数
                _overLimitCount.Clear();
            }
        }

        private double PullBToWithin1Um(double a, double b)
        {
            double tolerance = Math.Abs(GlobalSession.Instance.PullTolerance); // mm

            if (tolerance == 0 || Math.Abs(b - a) <= tolerance)
                return b;

            lock (_pullRandomLock)
            {
                double randomOffset = (_pullRandom.NextDouble() * 2 - 1) * tolerance;
                return a + randomOffset;
            }
        }

        private double Pull4BToWithin1Um(double a, double b)
        {
            double tolerance = Math.Abs(GlobalSession.Instance.Pull4Tolerance); // mm

            if (tolerance == 0 || Math.Abs(b - a) <= tolerance)
                return b;

            lock (_pullRandomLock)
            {
                double randomOffset = (_pullRandom.NextDouble() * 2 - 1) * tolerance;
                return a + randomOffset;
            }
        }


        private bool CheckValue(TechnologyParamDto dto, double value)
        {
            if (value == 0)
            {
                AppLog.Production.Info($"({dto.ParamValue}) 计算结果为0, 表示异常");
                return false;
            }
            switch (dto.MeasureType)
            {
                case 1: // 直径 四舍五入
                    //double target = Math.Truncate(dto.TargetValue * 10000) / 10000; 不四舍五入，支取后四位

                    double target = Math.Round(dto.TargetValue, 4, MidpointRounding.AwayFromZero);
                    return value >= (target + dto.LowerTolerance)
                        && value <= (target + dto.UpperTolerance);
                //return value >= dto.TargetValue + dto.LowerTolerance
                //    && value <= dto.TargetValue + dto.UpperTolerance;
                case 2: // 圆度
                case 3: // 圆柱度
                case 4: // 跳动
                    return value <= dto.UpperTolerance;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 测量开始
        /// </summary>
        private void MeasurementBegins()
        {
            if (Interlocked.CompareExchange(ref currentProcessState, (int)ProcessState.Started, (int)ProcessState.Idle) == (int)ProcessState.Idle)
            {
                CurrentProcessState = (int)ProcessState.Started;
                AppLog.Production.Info($"[PLC Send] 测量开始");
                StartTest();
            }
        }
        /// <summary>
        /// 测量完成
        /// </summary>
        private async Task MeasurementCompleted()
        {
            if (Interlocked.CompareExchange(ref currentProcessState, (int)ProcessState.Completed, (int)ProcessState.Started) == (int)ProcessState.Started)
            {
                CurrentProcessState = (int)ProcessState.Completed;
                AppLog.Production.Info($"[PLC Send] 测量结束");
                await StopTest();
                // 🔑 重置为 Idle，允许下一次测量
                Interlocked.Exchange(ref currentProcessState, (int)ProcessState.Idle);
            }
        }


        /// <summary>
        /// 测试结果判定，发送Ok或Ng
        /// </summary>
        private void MeasurementResult(bool isOK)
        {
            if (isOK)
            {
                var result = S7NetController.Instance.WritePlcPoint("DB203.DBX66.2", true);
                if (result)
                    AppLog.Production.Info("[PC Send] 发送OK信号(DB203.DBX66.2) 成功,1");
                else
                    AppLog.Production.Info("[PC Send] 发送OK信号(DB203.DBX66.2) 失败");
            }
            else
            {
                var result = S7NetController.Instance.WritePlcPoint("DB203.DBX66.3", true);
                if (result)
                    AppLog.Production.Info("[PC Send] 发送NG信号(DB203.DBX66.3) 成功,1");
                else
                    AppLog.Production.Info("[PC Send] 发送NG信号(DB203.DBX66.3) 失败");

            }
        }

        #endregion

        #region Curve Property
        private SeriesCollection lineSeriesCollectionALL;
        public SeriesCollection LineSeriesCollectionALL
        {
            get => lineSeriesCollectionALL;
            set
            {
                lineSeriesCollectionALL = value;
                OnPropertyChanged();
            }
        }

        private double _xMin = 0;
        public double XMin
        {
            get => _xMin;
            set { _xMin = value; OnPropertyChanged(); }
        }

        private double _xMax = 10;
        public double XMax
        {
            get => _xMax;
            set { _xMax = value; OnPropertyChanged(); }
        }


        public ObservableCollection<LegendItemModel> LegendItems { get; set; }

        private int _totalPoints = 25; // 缓存总点数
        private int _windowSize = 25;  // X轴显示窗口大小
        private int _currentIndex = 0; // 用作X轴的秒数索引
        #endregion

        #region Curve Action
        //public void InitLineSeries()
        //{
        //    for (int i = 0; i < GlobalSession.Instance.CurrentTechnologyParamDtos.Count(); i++)
        //    {
        //        TechnologyParamDto technologyParamDto = GlobalSession.Instance.CurrentTechnologyParamDtos[i];
        //        LineSeries lineSeries = new LineSeries
        //        {
        //            Title = technologyParamDto.ParamValue,
        //            Values = new ChartValues<double> { },
        //            Fill = Brushes.Transparent,
        //            ScalesYAt = 0,
        //            Stroke = Brushes.Red,
        //            PointGeometrySize = 2
        //        };
        //        LineSeriesCollectionALL.Add(lineSeries);
        //    }

        //    LineSeriesCollectionALL = new SeriesCollection
        //    {
        //          new LineSeries
        //        {
        //            Title = "一档外圆直径",
        //            Values = new ChartValues<double> { },
        //             Fill = Brushes.Transparent,
        //             ScalesYAt=0,
        //             Stroke=Brushes.Red,
        //             PointGeometrySize=2
        //        },
        //        new LineSeries
        //        {
        //            Title = "二档外圆直径",
        //            Values = new ChartValues<double> {  },
        //             Fill = Brushes.Transparent,
        //             ScalesYAt=0,
        //             Stroke=Brushes.Blue,
        //             PointGeometrySize=2
        //        },
        //        new LineSeries
        //        {
        //            Title = "三档外圆圆度",
        //            Values = new ChartValues<double> {  },
        //             Fill = Brushes.Transparent,
        //             ScalesYAt=0,
        //             Stroke=Brushes.Green,
        //             PointGeometrySize=2
        //        },new LineSeries
        //        {
        //            Title = "四档外圆圆柱度",
        //            Values = new ChartValues<double> { }, Fill = Brushes.Transparent,
        //             ScalesYAt=2,
        //             Stroke=Brushes.DeepPink,
        //             PointGeometrySize=2
        //        },
        //        new LineSeries
        //        {
        //            Title = "五档外圆跳动",
        //            Values = new ChartValues<double> {},
        //            Fill = Brushes.Transparent,
        //             ScalesYAt=1,
        //             Stroke=Brushes.Chocolate,
        //             PointGeometrySize=2
        //        }
        //    };
        //    XMin = 0;
        //    XMax = _windowSize - 1;

        //    // 创建 LegendItems
        //    var brushes = new Brush[]
        //    {
        //    Brushes.Red, Brushes.Blue, Brushes.Green,
        //      Brushes.Chocolate, Brushes.Black,
        //    };

        //    LegendItems = new ObservableCollection<LegendItemModel>();
        //    LegendItems.Add(new LegendItemModel
        //    {
        //        Title = "全部显示",
        //        Color = Brushes.DarkKhaki,
        //        seriesCollection = LineSeriesCollectionALL
        //    });
        //    LegendItems.Add(new LegendItemModel
        //    {
        //        Title = "全部隐藏",
        //        Color = Brushes.DodgerBlue,
        //        seriesCollection = LineSeriesCollectionALL
        //    });
        //    for (int i = 0; i < LineSeriesCollectionALL.Count; i++)
        //    {
        //        LegendItems.Add(new LegendItemModel
        //        {
        //            Title = LineSeriesCollectionALL[i].Title,
        //            Color = brushes[i],
        //            Series = (LineSeries)LineSeriesCollectionALL[i]
        //        });
        //    }
        //}

        //public void UpdateLineAll(List<(int index, double value)> v)
        //{
        //    for (int i = 0; i < v.Count; i++)
        //    {

        //        var line = LineSeriesCollectionALL[i] as LineSeries;
        //        if (line == null) return;
        //        var values = line.Values as ChartValues<ObservablePoint>;
        //        if (values == null)
        //        {
        //            values = new ChartValues<ObservablePoint>();
        //            line.Values = values;
        //        }
        //        // 使用 ObservablePoint 保存绝对 X 值
        //        values.Add(new ObservablePoint(_currentIndex, v[i].value));
        //    }


        //    if (_currentIndex >= _windowSize - 1)
        //    {
        //        XMax = _currentIndex;
        //        XMin = _currentIndex - (_windowSize - 1);
        //    }
        //    else
        //    {
        //        XMin = 0;
        //        XMax = _windowSize - 1;
        //    }
        //    _currentIndex++;
        //}
        #endregion

        private CancellationTokenSource cancellationTokenSource;

        //private void LoopPingPlc()
        //{
        //    cancellationTokenSource = new CancellationTokenSource();
        //    var token = cancellationTokenSource.Token;
        //    Task.Run(async () =>
        //    {
        //        while (!token.IsCancellationRequested)
        //        {
        //            PingPlc("192.168.1.10");

        //            await Task.Delay(1000);
        //        }
        //    }, token);

        //}

        //private bool PingPlc(string ip)
        //{
        //    try
        //    {
        //        Ping ping = new Ping();
        //        var reply = ping.Send(ip, 500);
        //        return reply.Status == IPStatus.Success;
        //    }
        //    catch
        //    {

        //        return false;
        //    }
        //}
    }
}

//using DocumentFormat.OpenXml.Drawing;
//using HandyControl.Controls;
//using SDSMillimar.Common;
//using SDSMillimar.Models;
//using SDSMillimar.Services;
//using SDSMillimar.Utils;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Documents;
//using System.Windows.Input;

//namespace SDSMillimar.ViewModel
//{
//    public class ProbeDataViewModel : BaseViewModel, IDisposable
//    {
//        public ICommand LeftCommand { get; set; }
//        public ICommand RightCommand { get; set; }
//        public ICommand StartTestCommand { get; set; }
//        public ICommand StopTestCommand { get; set; }
//        public ICommand ResetOffsetCommand { get; set; }
//        public ICommand ZeroDrivceCommand { get; set; }

//        public ICommand GetAllDataCommand { get; set; }

//        private bool btnControlCan = true;
//        private bool isBD = false;

//        private readonly MillimarBDRepository millimarBDRepository;

//        public bool BtnControlCan
//        {
//            get { return btnControlCan; }
//            set { btnControlCan = value; OnPropertyChanged(); }
//        }

//        private bool _isTesting;
//        public bool IsTesting
//        {
//            get => _isTesting;
//            set
//            {
//                _isTesting = value;
//                OnPropertyChanged();
//                // 状态变化时，刷新命令可用性
//                CommandManager.InvalidateRequerySuggested();
//            }
//        }

//        private bool isRefresh = false;

//        public bool IsRefresh
//        {
//            get { return isRefresh; }
//            set { isRefresh = value; OnPropertyChanged(); }
//        }



//        private ObservableCollection<ChannelModel> channelModels;

//        public ObservableCollection<ChannelModel> ChannelModels
//        {
//            get { return channelModels; }
//            set { channelModels = value; OnPropertyChanged(); }
//        }
//        private List<MillimarBD> lists = new List<MillimarBD>();
//        public ProbeDataViewModel()
//        {
//            //LeftCommand = new RelayCommand(Left);
//            //RightCommand = new RelayCommand(Right);
//            //MillN1700Helper.Instance.CommunicationChanged += Instance_CommunicationChanged;
//            //MillN1700Helper.Instance.ChannelsChanged += Instance_ChannelsChanged;
//            //MillN1700Helper.Instance.ModulesChanged += Instance_ModulesChanged;
//            millimarBDRepository = new MillimarBDRepository();
//            MillN1700Helper.Instance.DataBatchReceived += Instance_DataBatchReceived;
//            StartTestCommand = new RelayCommand(StartTest);
//            StopTestCommand = new RelayCommand(StopTest);
//            GetAllDataCommand = new RelayCommand(GetAllData, () => !IsTesting);
//            ResetOffsetCommand = new RelayCommand(async () =>
//            {
//                await ResetOffset();
//            });
//            ZeroDrivceCommand = new RelayCommand(async () =>
//            {
//                await ZeroDrivce();
//            });
//            foreach (var item in GlobalSession.Instance.Probes)
//            {
//                item.MeasuredValue = 0;
//            }
//        }

//        private void GetAllData()
//        {
//            if (IsTesting)
//            {
//                Growl.Warning("请先停止测试，再进行标定");
//                return;
//            }
//            isBD = true;
//            IsRefresh = true;
//            N1700Lib.N1700RequestAllData(0);

//        }

//        private void Instance_ModulesChanged(uint obj)
//        {
//            if (obj == 5)
//                BtnControlCan = true;
//            else
//                BtnControlCan = false;
//        }

//        private void Instance_ChannelsChanged(uint obj)
//        {
//            if (obj == 11)
//                BtnControlCan = true;
//            else
//                BtnControlCan = false;
//        }

//        private void Instance_CommunicationChanged(bool obj)
//        {
//            BtnControlCan = true;
//        }

//        private async void Instance_DataBatchReceived(ObservableCollection<ChannelModel> collection)
//        {
//            try
//            {
//                ChannelModels = collection;
//                if (collection.Count < GlobalSession.Instance.Probes.Count) return;
//                //if (collection.Count != 11) return;
//                lists.Clear();
//                for (int i = 0; i < GlobalSession.Instance.Probes.Count; i++)
//                {

//                    MillimarBD millimarBD = new MillimarBD();
//                    millimarBD.Key = collection[i + 1].ChannelIdx;
//                    millimarBD.Value = collection[i + 1].Value;
//                    lists.Add(millimarBD);

//                    Application.Current.Dispatcher.Invoke(() =>
//                    {
//                        GlobalSession.Instance.Probes[i].MeasuredValue = collection[i + 1].Value;
//                    });
//                }
//                if (isBD)
//                {
//                    //Task.Run(async () =>
//                    //{
//                    var result = await millimarBDRepository.AddAsync(lists);
//                    isBD = false;
//                    if (result > 0)
//                    {
//                        //Growl.Success("标定成功");
//                        System.Windows.MessageBox.Show("标定成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
//                        IsRefresh = true;
//                    }
//                    else
//                        //Growl.Error("标定失败");
//                        System.Windows.MessageBox.Show("标定失败", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
//                    //var slotDatas = ChannelModels.OrderBy(x => x.ChannelIdx).ToList();
//                    //GlobalSession.Instance.BdDatas = slotDatas.Select(x => x.Value).ToList();
//                    //});
//                }
//            }
//            catch (Exception ex)
//            {
//                AppLog.Production.Error(ex);
//            }

//        }

//        public void Dispose()
//        {
//            MillN1700Helper.Instance.DataBatchReceived -= Instance_DataBatchReceived;
//        }

//        private async Task ZeroDrivce()
//        {
//            MillN1700Helper.Instance.N1700StopContinuousRequestAllData();
//            AppLog.Production.Info("开始回零");
//            await Task.Delay(300);
//            await ResetOffset();

//            await Task.Delay(300);
//            N1700Lib.N1700RequestAllData(0);
//            await Task.Delay(300);
//            await ZeroAllChannels1();
//            await Task.Delay(300);
//            N1700Lib.N1700RequestAllData(0);
//            AppLog.Production.Info("结束回零");
//        }

//        /// <summary>
//        /// 恢复偏移量
//        /// </summary>
//        private async Task ResetOffset()
//        {
//            for (int i = 1; i <= ChannelModels?.Count; i++)
//            {
//                //MillN1700Helper.Instance.ZeroChannel(ChannelModels[i].ChannelIdx);
//                MillN1700Helper.Instance.N1700SetOffsetMM((uint)i, 0);
//                await Task.Delay(100);
//            }
//        }

//        private void StopTest()
//        {
//            MillN1700Helper.Instance.N1700StopContinuousRequestAllData();
//            IsTesting = false;
//        }

//        private void StartTest()
//        {
//            MillN1700Helper.Instance.N1700StartContinuousRequestAllData();
//            IsTesting = true;
//        }

//        private void Left()
//        {
//            GlobalSession.Instance.Probes[0].MeasuredValue -= 0.1;
//        }
//        private void Right()
//        {
//            GlobalSession.Instance.Probes[0].MeasuredValue += 0.1;
//        }

//        public async Task<bool> ZeroAllChannels1()
//        {
//            if (ChannelModels == null)
//                return false;
//            int channelCount = ChannelModels.Count;
//            // 3️⃣ 统一设置 Offset
//            for (int i = 1; i < channelCount; i++)
//            {
//                int ret = N1700Lib.N1700SetOffsetMM((uint)i, (float)ChannelModels[i].Value);
//                if (ret != N1700Lib.N1700_SUCCESS)
//                {
//                    return false;
//                }
//                await Task.Delay(100);
//            }
//            return true;
//        }
//    }
//}
using DocumentFormat.OpenXml.Drawing;
using HandyControl.Controls;
using SDSMillimar.Common;
using SDSMillimar.Models;
using SDSMillimar.Services;
using SDSMillimar.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SDSMillimar.ViewModel
{
    public class ProbeDataViewModel : BaseViewModel, IDisposable
    {
        public ICommand LeftCommand { get; set; }
        public ICommand RightCommand { get; set; }
        public ICommand StartTestCommand { get; set; }
        public ICommand StopTestCommand { get; set; }
        public ICommand ResetOffsetCommand { get; set; }
        public ICommand ZeroDrivceCommand { get; set; }
        public ICommand GetAllDataCommand { get; set; }

        private bool btnControlCan = true;

        private readonly MillimarBDRepository millimarBDRepository;

        // 等待单次设备返回数据
        private TaskCompletionSource<ObservableCollection<ChannelModel>> _dataTcs;

        public bool BtnControlCan
        {
            get { return btnControlCan; }
            set
            {
                btnControlCan = value;
                OnPropertyChanged();
            }
        }

        private bool _isTesting;
        public bool IsTesting
        {
            get => _isTesting;
            set
            {
                _isTesting = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool isRefresh = false;
        public bool IsRefresh
        {
            get { return isRefresh; }
            set
            {
                isRefresh = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ChannelModel> channelModels;
        public ObservableCollection<ChannelModel> ChannelModels
        {
            get { return channelModels; }
            set
            {
                channelModels = value;
                OnPropertyChanged();
            }
        }

        public ProbeDataViewModel()
        {
            millimarBDRepository = new MillimarBDRepository();

            MillN1700Helper.Instance.DataBatchReceived += Instance_DataBatchReceived;

            StartTestCommand = new RelayCommand(StartTest);
            StopTestCommand = new RelayCommand(StopTest);

            GetAllDataCommand = new RelayCommand(async () =>
            {
                await GetAllData();
            }, () => !IsTesting);

            ResetOffsetCommand = new RelayCommand(async () =>
            {
                await ResetOffset();
            });

            ZeroDrivceCommand = new RelayCommand(async () =>
            {
                await ZeroDrivce();
            });

            foreach (var item in GlobalSession.Instance.Probes)
            {
                item.MeasuredValue = 0;
            }
        }

        /// <summary>
        /// 标定
        /// </summary>
        private async Task GetAllData()
        {
            if (IsTesting)
            {
                Growl.Warning("请先停止测试，再进行标定");
                return;
            }

            try
            {
                IsRefresh = true;

                AppLog.Production.Info("开始请求单次测量数据");

                var collection = await RequestSingleDataAsync();

                if (collection == null || collection.Count == 0)
                {
                    System.Windows.MessageBox.Show(
                        "未获取到测量数据",
                        "提示",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                var lists = new List<MillimarBD>();

                for (int i = 0; i < GlobalSession.Instance.Probes.Count; i++)
                {
                    lists.Add(new MillimarBD
                    {
                        Key = collection[i + 1].ChannelIdx,
                        Value = collection[i + 1].Value
                    });
                }

                AppLog.Production.Info("开始保存标定数据");

                var result = await millimarBDRepository.AddAsync(lists);

                if (result > 0)
                {
                    System.Windows.MessageBox.Show(
                        "标定成功",
                        "提示",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        "标定失败",
                        "提示",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (TimeoutException)
            {
                System.Windows.MessageBox.Show(
                    "设备返回数据超时",
                    "提示",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                AppLog.Production.Error(ex);

                System.Windows.MessageBox.Show(
                    ex.Message,
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 请求一次设备数据
        /// </summary>
        private async Task<ObservableCollection<ChannelModel>> RequestSingleDataAsync()
        {
            if (_dataTcs != null && !_dataTcs.Task.IsCompleted)
            {
                throw new Exception("当前已有数据请求正在进行");
            }

            _dataTcs = new TaskCompletionSource<ObservableCollection<ChannelModel>>();

            N1700Lib.N1700RequestAllData(0);

            var timeoutTask = Task.Delay(5000);

            var completedTask = await Task.WhenAny(
                _dataTcs.Task,
                timeoutTask);

            if (completedTask == timeoutTask)
            {
                throw new TimeoutException();
            }

            return await _dataTcs.Task;
        }

        /// <summary>
        /// 接收设备批量数据
        /// </summary>
        private void Instance_DataBatchReceived(
            ObservableCollection<ChannelModel> collection)
        {
            try
            {
                ChannelModels = collection;

                if (collection.Count < GlobalSession.Instance.Probes.Count)
                {
                    return;
                }

                for (int i = 0; i < GlobalSession.Instance.Probes.Count; i++)
                {
                    int index = i;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        GlobalSession.Instance.Probes[index].MeasuredValue =
                            collection[index + 1].Value;
                    });
                }

                // 如果当前有单次请求正在等待
                _dataTcs?.TrySetResult(collection);
            }
            catch (Exception ex)
            {
                AppLog.Production.Error(ex);

                _dataTcs?.TrySetException(ex);
            }
        }

        public void Dispose()
        {
            MillN1700Helper.Instance.DataBatchReceived -= Instance_DataBatchReceived;
        }

        private async Task ZeroDrivce()
        {
            MillN1700Helper.Instance.N1700StopContinuousRequestAllData();

            AppLog.Production.Info("开始回零");

            await Task.Delay(300);

            await ResetOffset();

            await Task.Delay(300);

            N1700Lib.N1700RequestAllData(0);

            await Task.Delay(300);

            await ZeroAllChannels1();

            await Task.Delay(300);

            N1700Lib.N1700RequestAllData(0);

            AppLog.Production.Info("结束回零");
        }

        /// <summary>
        /// 恢复偏移量
        /// </summary>
        private async Task ResetOffset()
        {
            if (ChannelModels == null)
                return;

            for (int i = 1; i <= ChannelModels.Count; i++)
            {
                MillN1700Helper.Instance.N1700SetOffsetMM((uint)i, 0);

                await Task.Delay(100);
            }
        }

        private void StopTest()
        {
            MillN1700Helper.Instance.N1700StopContinuousRequestAllData();

            IsTesting = false;
        }

        private void StartTest()
        {
            MillN1700Helper.Instance.N1700StartContinuousRequestAllData();

            IsTesting = true;
        }

        private void Left()
        {
            GlobalSession.Instance.Probes[0].MeasuredValue -= 0.1;
        }

        private void Right()
        {
            GlobalSession.Instance.Probes[0].MeasuredValue += 0.1;
        }

        public async Task<bool> ZeroAllChannels1()
        {
            if (ChannelModels == null)
                return false;

            int channelCount = ChannelModels.Count;

            for (int i = 1; i < channelCount; i++)
            {
                int ret = N1700Lib.N1700SetOffsetMM(
                    (uint)i,
                    (float)ChannelModels[i].Value);

                if (ret != N1700Lib.N1700_SUCCESS)
                {
                    return false;
                }

                await Task.Delay(100);
            }

            return true;
        }
    }
}
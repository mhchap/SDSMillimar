using SDSMillimar.Dtos;
using SDSMillimar.Models;
using SDSMillimar.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using static Mysqlx.Crud.Order.Types;

namespace SDSMillimar.Common
{
    public class GlobalSession : INotifyPropertyChanged
    {

        private static readonly Lazy<GlobalSession> _instance =
   new Lazy<GlobalSession>(() => new GlobalSession());

        public static GlobalSession Instance => _instance.Value;
        public GlobalSession()
        {

        }


        public ObservableCollection<ChannelModel> ChannelModels = new ObservableCollection<ChannelModel>();

        public List<double> BdDatas = new List<double>();

        public double FilterExtremumPre
        {
            get
            {
                var value = ConfigurationManager.AppSettings["FilterExtremumPre"];
                if (double.TryParse(value, out double result))
                    return result;

                // 解析失败时的兜底值
                return 0.0;
            }
        }

        public bool IsFilter
        {
            get
            {
                var value = ConfigurationManager.AppSettings["IsFilter"];
                if (bool.TryParse(value, out bool result))
                    return result;

                // 解析失败时的兜底值
                return false;
            }
        }

        public int SampleIntervalMs
        {
            get
            {
                var value = ConfigurationManager.AppSettings["SampleIntervalMs"];
                if (int.TryParse(value, out int result))
                    return result;

                // 解析失败时的兜底值
                return 0;
            }
        }

        public int Span
        {
            get
            {
                var value = ConfigurationManager.AppSettings["Span"];
                if (int.TryParse(value, out int result))
                    return result;

                // 解析失败时的兜底值
                return 0;
            }
        }
        public int ExcludeCount
        {
            get
            {
                var value = ConfigurationManager.AppSettings["ExcludeCount"];
                if (int.TryParse(value, out int result))
                    return result;

                // 解析失败时的兜底值
                return 0;
            }
        }


        public int WindowSize
        {
            get
            {
                var value = ConfigurationManager.AppSettings["WindowSize"];
                if (int.TryParse(value, out int result))
                    return result;

                // 解析失败时的兜底值
                return 0;
            }
        }

        public int MaxMeasuredCount
        {
            get
            {
                var value = ConfigurationManager.AppSettings["MaxMeasuredCount"];
                if (int.TryParse(value, out int result))
                    return result;

                // 解析失败时的兜底值
                return 0;
            }
        }


        public int MaxTestMeasuredCount
        {
            get
            {
                var value = ConfigurationManager.AppSettings["MaxTestMeasuredCount"];
                if (int.TryParse(value, out int result))
                    return result;

                // 解析失败时的兜底值
                return 0;
            }
        }

        public int CalibrationDiameter
        {
            get
            {
                var value = ConfigurationManager.AppSettings["CalibrationDiameter"];
                if (int.TryParse(value, out int result))
                    return result;

                // 解析失败时的兜底值
                return 0;
            }
        }

        public int LearningRate
        {
            get
            {
                var value = ConfigurationManager.AppSettings["LearningRate"];
                if (int.TryParse(value, out int result))
                    return result;

                // 解析失败时的兜底值
                return 0;
            }
        }

        public double OverNum
        {
            get
            {
                var value = ConfigurationManager.AppSettings["OverNum"];
                if (double.TryParse(value, out double result))
                    return result;

                // 解析失败时的兜底值
                return 1.5;
            }
        }

        private int isPull;

        /// <summary>
        /// 是否从 CGK 测量入口进入测量页面：1 是，0 否。
        /// </summary>
        public int IsPull
        {
            get { return isPull; }
            set
            {
                if (isPull == value)
                    return;

                isPull = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPull)));
            }
        }

        public double PullTolerance
        {
            get
            {
                var value = ConfigurationManager.AppSettings["PullTolerance"];
                if (double.TryParse(value, out double result))
                    return result;

                // 解析失败时的兜底值
                return 0.001;
            }
        }

        public double Pull4Tolerance
        {
            get
            {
                var value = ConfigurationManager.AppSettings["Pull4Tolerance"];
                if (double.TryParse(value, out double result))
                    return result;

                // 解析失败时的兜底值
                return 0.005;
            }
        }

        private List<TechnologyParamDto> currentTechnologyParamDtos;
        /// <summary>
        /// 当前动态工艺参数，工件测量项目
        /// </summary>
        public List<TechnologyParamDto> CurrentTechnologyParamDtos
        {
            get { return currentTechnologyParamDtos; }
            set { currentTechnologyParamDtos = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTechnologyParamDtos))); }
        }

        private TechnologyDto currentSelectedTechnologyDto;

        public TechnologyDto CurrentSelectedTechnologyDto
        {
            get { return currentSelectedTechnologyDto; }
            set { currentSelectedTechnologyDto = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSelectedTechnologyDto))); }
        }

        private List<TechnologyDto> currentSelectedTechnologyDtos;

        public List<TechnologyDto> CurrentSelectedTechnologyDtos
        {
            get { return currentSelectedTechnologyDtos; }
            set { currentSelectedTechnologyDtos = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSelectedTechnologyDtos))); }
        }

        private List<OptionsModel> currentChannelOptionsModel = new List<OptionsModel>
        {
            new OptionsModel(){ Key=1,Value=1},
            new OptionsModel(){ Key=2,Value=2},
            new OptionsModel(){ Key=3,Value=3},
            new OptionsModel(){ Key=4,Value=4},
            new OptionsModel(){ Key=5,Value=5},
            new OptionsModel(){ Key=6,Value=6},
            new OptionsModel(){ Key=7,Value=7},
            new OptionsModel(){ Key=8,Value=8},
            new OptionsModel(){ Key=9,Value=9},
            new OptionsModel(){ Key=10,Value=10},
        };

        public List<OptionsModel> CurrentChannelOptionsModel
        {
            get { return currentChannelOptionsModel; }
            set
            {
                currentChannelOptionsModel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentChannelOptionsModel)));
            }
        }


        private Product selectedProduct;

        public Product SelectedProduct
        {
            get { return selectedProduct; }
            set { selectedProduct = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedProduct))); }
        }

        public ObservableCollection<ProbeItem> Probes { get; set; }
    = new ObservableCollection<ProbeItem>
            {
                new ProbeItem() { Direction = 1, Title = "探针1", Id = 1, MeasuredValue=0,GridColumn = 0, GridRow = 0, IsSelected = false},
                new ProbeItem() { Direction = -1, Title = "探针2", Id = 2, GridColumn = 1, MeasuredValue=0, GridRow = 0, IsSelected = false},
                new ProbeItem() { Direction = 1, Title = "探针3", Id = 3, GridColumn = 0, MeasuredValue=0, GridRow = 1, IsSelected = false},
                new ProbeItem() { Direction = -1, Title = "探针4", Id = 4, GridColumn = 1, MeasuredValue=0, GridRow = 1, IsSelected = false},
                new ProbeItem() { Direction = 1, Title = "探针5", Id = 5, GridColumn = 0,  MeasuredValue=0,GridRow = 2, IsSelected = false},
                new ProbeItem() { Direction = -1, Title = "探针6", Id = 6, GridColumn = 1, MeasuredValue=0, GridRow = 2, IsSelected = false},
                new ProbeItem() { Direction = 1, Title = "探针7", Id = 7, GridColumn = 0,  MeasuredValue=0,GridRow = 3, IsSelected = false},
                new ProbeItem() { Direction = -1, Title = "探针8", Id = 8, GridColumn = 1, MeasuredValue=0, GridRow = 3, IsSelected = false},
                new ProbeItem() { Direction = 1, Title = "探针9", Id = 9, GridColumn = 0,  MeasuredValue=0,GridRow = 4, IsSelected = false},
                new ProbeItem() { Direction = -1, Title = "探针10", Id = 10, GridColumn = 1, MeasuredValue=0, GridRow = 4, IsSelected = false},
            };

        public void SetProbeSelect(string DeviceIds)
        {
            if (!string.IsNullOrEmpty(DeviceIds))
            {
                var ids = DeviceIds.Split(',');
                for (int i = 0; i < ids.Length; i++)
                {
                    ProbeItem item = Probes.Where(x => x.Id == Convert.ToInt64(ids[i])).FirstOrDefault();
                    if (item == null)
                        continue;

                    Probes.Where(x => x.Id == Convert.ToInt64(ids[i])).First().IsSelected = true;
                }
            }
            else
            {
                foreach (var probe in Probes)
                {
                    probe.IsSelected = false;
                }
                ;
            }
        }

        private bool isTestItem = true;

        public bool IsTestItem
        {
            get { return isTestItem; }
            set
            {
                isTestItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTestItem)));
                Messenger.Send<bool>("DataBatchReceived", value);
            }
        }

        public List<DemoType> DemoTypes
        {
            get; set;
        } = new List<DemoType>() {
            new DemoType{ Key=false, Value="启用"},
            new DemoType{ Key=true, Value="不启用"},
        };
        public event PropertyChangedEventHandler PropertyChanged;
    }
}

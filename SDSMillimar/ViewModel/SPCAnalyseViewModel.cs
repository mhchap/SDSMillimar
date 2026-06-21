using HandyControl.Controls;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MySqlX.XDevAPI.Common;
using SDSMillimar.Common;
using SDSMillimar.Dtos;
using SDSMillimar.Models;
using SDSMillimar.Services;
using SDSMillimar.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SDSMillimar.ViewModel
{
    public class SPCAnalyseViewModel : BaseViewModel
    {
        private readonly ProcessDataRepository processDataRepository = null;
        private readonly TechnologyRepository technologyRepository = null;
        private readonly DynamicTechnologyRepository dynamicTechnologyRepository = null;


        /* ================= X̄ 图 ================= */

        private SeriesCollection _xBarSeries;
        public SeriesCollection XBarSeries
        {
            get => _xBarSeries;
            set { _xBarSeries = value; OnPropertyChanged(); }
        }

        private string[] _xBarLabels;
        public string[] XBarLabels
        {
            get => _xBarLabels;
            set { _xBarLabels = value; OnPropertyChanged(); }
        }


        /* ================= R 图 ================= */
        private SeriesCollection _rSeries;
        public SeriesCollection RSeries
        {
            get => _rSeries;
            set { _rSeries = value; OnPropertyChanged(); }
        }

        private string[] _rLabels;
        public string[] RLabels
        {
            get => _rLabels;
            set { _rLabels = value; OnPropertyChanged(); }
        }

        private Func<double, string> _yFormatter;
        public Func<double, string> YFormatter
        {
            get => _yFormatter;
            set { _yFormatter = value; OnPropertyChanged(); }
        }






        private int subgroupNum = 5;
        /// <summary>
        /// 子组大小：表示此次分析多少工件
        /// </summary>
        public int SubgroupNum
        {
            get { return subgroupNum; }
            set { subgroupNum = value; OnPropertyChanged(); UpdateColumns?.Invoke(); }
        }

        private int sampleNum = 5;
        /// <summary>
        /// 子组数：表示有多少样本数据
        /// </summary>
        public int SamplesNum
        {
            get { return sampleNum; }
            set { sampleNum = value; OnPropertyChanged(); UpdateColumns?.Invoke(); }
        }
        // 委托通知 View 生成列
        public Action UpdateColumns { get; set; }

        private List<SubgroupDto> subgroups = new List<SubgroupDto>();

        public List<SubgroupDto> Subgroups
        {
            get { return subgroups; }
            set { subgroups = value; OnPropertyChanged(); }
        }

        private List<Technology> technologys;

        public List<Technology> Technologys
        {
            get { return technologys; }
            set { technologys = value; OnPropertyChanged(); }
        }

        private long selectedTechnologyId;

        public long SelectedTechnologyId
        {
            get { return selectedTechnologyId; }
            set
            {
                selectedTechnologyId = value;
                OnPropertyChanged();
                _ = TechnologySelectedChanged();
            }
        }

        private DateTime startTime = DateTime.Today;

        public DateTime StartTime
        {
            get { return startTime; }
            set { startTime = value; OnPropertyChanged(); }
        }

        private DateTime endTime = DateTime.Today.AddDays(1);

        public DateTime EndTime
        {
            get { return endTime; }
            set
            {
                endTime = value;
                OnPropertyChanged();
            }
        }


        private List<TechnologyParam> technologyParams;

        public List<TechnologyParam> TechnologyParams
        {
            get { return technologyParams; }
            set { technologyParams = value; OnPropertyChanged(); }
        }

        private string selectedTechnologyParamsParamName;

        public string SelectedTechnologyParamsParamName
        {
            get { return selectedTechnologyParamsParamName; }
            set { selectedTechnologyParamsParamName = value; OnPropertyChanged(); }
        }

        private bool technologyParamsEnabled = false;

        public bool TechnologyParamsEnabled
        {
            get { return technologyParamsEnabled; }
            set { technologyParamsEnabled = value; OnPropertyChanged(); }
        }



        private async Task TechnologySelectedChanged()
        {
            TechnologyParams = await dynamicTechnologyRepository.GetByTechnologyIdAsync(SelectedTechnologyId);
            if (TechnologyParams != null && TechnologyParams.Count > 0)
                TechnologyParamsEnabled = true;
            else
                TechnologyParamsEnabled = false;
        }

        public DataTable MeasurementData { get; set; } = new DataTable();
        public Action ExportBmpRequested { get; set; }
        public ICommand BuildXBarRCommand { get; set; }
        public ICommand ExportBmpCommand { get; set; }
        public ICommand LoadedCommand { get; set; }

        public SPCAnalyseViewModel()
        {
            processDataRepository = new ProcessDataRepository();
            technologyRepository = new TechnologyRepository();
            dynamicTechnologyRepository = new DynamicTechnologyRepository();
            ExportBmpCommand = new RelayCommand(ExportBmp);
            LoadedCommand = new RelayCommand(async () => { await Loaded(); });
            BuildXBarRCommand = new RelayCommand(async () =>
            {
                await BuildXBarR();
            });



        }

        private async Task Loaded()
        {
            PageResult<Technology> technologies = await technologyRepository.GetPageAsync(1, 10000);
            Technologys = technologies.Items;
        }

        private void ExportBmp()
        {
            ExportBmpRequested?.Invoke();
        }

        private async Task BuildXBarR()
        {
            await LoadFromDatabase();
        }

        public async Task LoadFromDatabase()
        {
            await LoadData();
            // 清空表格
            MeasurementData.Columns.Clear();
            MeasurementData.Rows.Clear();

            // 1️⃣ 添加列
            MeasurementData.Columns.Add("子组"); // 第一列显示子组名
            if (Subgroups.Count == 0)
            {
                Growl.Warning("没有当前工艺对应的测量数据");
                return;
            }
            int maxSamples = Subgroups.Max(s => s.Samples.Count);
            for (int i = 0; i < maxSamples; i++)
            {
                MeasurementData.Columns.Add($"测量{i + 1}");
            }

            // 2️⃣ 添加行
            foreach (var subgroup in Subgroups)
            {
                var row = MeasurementData.NewRow();
                row["子组"] = subgroup.SubgroupName;

                for (int i = 0; i < subgroup.Samples.Count; i++)
                {
                    row[$"测量{i + 1}"] = subgroup.Samples[i];
                }

                MeasurementData.Rows.Add(row);
            }
            try
            {

                SPCResult result = SPC.CalculateByRow(MeasurementData, 2, 1);

                int count = result.MeanValues.Length;

                XBarLabels = Enumerable.Range(1, count)
                                       .Select(i => $"样本{i}")
                                       .ToArray();

                RLabels = XBarLabels;

                YFormatter = v => v.ToString("F3");

                /* ============ X̄ 图 ============ */
                XBarSeries = new SeriesCollection
            {
                // 均值点
                new LineSeries
                {
                    Title = "均值",
                    Values = new ChartValues<double>(result.MeanValues),
                    PointGeometrySize = 8
                },
                CreateConstLine("CL(中心线)", result.MeanValues.Average(), count),
                CreateConstLine("UCL(上控制线)", result.XBarUCL, count),
                CreateConstLine("LCL(下控制线)", result.XBarLCL, count),
                CreateLineLabel($"CL={result.MeanValues.Average():F3}", result.MeanValues.Average(), count),
                CreateLineLabel($"UCL={result.XBarUCL:F3}", result.XBarUCL, count),
                CreateLineLabel($"LCL={result.XBarLCL:F3}", result.XBarLCL, count),
            };

                /* ============ R 图 ============ */
                RSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "极差值",
                    Values = new ChartValues<double>(result.RangeValues),
                    PointGeometrySize = 8
                },
                CreateConstLine("CL(中心线)", result.RangeValues.Average(), count),
                CreateConstLine("UCL(上控制线)", result.R_UCL, count),
                CreateConstLine("LCL(下控制线)", result.R_LCL, count),
                CreateLineLabel($"CL={result.MeanValues.Average():F3}", result.RangeValues.Average(), count),
                CreateLineLabel($"UCL={result.XBarUCL:F3}", result.R_UCL, count),
                CreateLineLabel($"LCL={result.XBarLCL:F3}", result.R_LCL, count),
            };
            }
            catch (Exception ex)
            {
                Growl.Warning(ex.Message);
                return;
            }

        }

        public async Task LoadData()
        {
            Subgroups.Clear();
            Subgroups = await processDataRepository.GetSubgroupsAsync(SelectedTechnologyId, SelectedTechnologyParamsParamName, StartTime, EndTime, SubgroupNum, SubgroupNum * SamplesNum);

        }

        private LineSeries CreateConstLine(string title, double value, int count)
        {
            return new LineSeries
            {
                Title = title,
                Values = new ChartValues<double>(
                    Enumerable.Repeat(value, count)
                ),
                PointGeometry = null,
                StrokeDashArray = new System.Windows.Media.DoubleCollection { 4, 2 }
            };
        }

        private ScatterSeries CreateLineLabel(string text, double value, int index)
        {
            return new ScatterSeries
            {
                Values = new ChartValues<ObservablePoint>
                {
                    new ObservablePoint(index - 0.5, value)
                },
                DataLabels = true,
                LabelPoint = p => text,
                PointGeometry = null,
                Foreground = System.Windows.Media.Brushes.Gray
            };
        }

    }
}

using HandyControl.Controls;
using HandyControl.Data;
using SDSMillimar.Common;
using SDSMillimar.Dtos;
using SDSMillimar.Models;
using SDSMillimar.Services;
using SDSMillimar.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SDSMillimar.ViewModel
{
    public class QueryDataViewModel : BaseViewModel
    {
        private readonly ProcessDataRepository repo = null;
        private TechnologyRepository technologyRepository = null;
        private ProductRepository productRepository = null;

        public ObservableCollection<StatusOption> StatusList { get; set; } =
            new ObservableCollection<StatusOption>
        {
    new StatusOption { Name = "全部", Value = null },
    new StatusOption { Name = "合格", Value = true },
    new StatusOption { Name = "不合格", Value = false }
        };

        //public bool? SelectedStatus { get; set; }

        private bool? selectedStatus = false;

        public bool? SelectedStatus
        {
            get { return selectedStatus; }
            set { selectedStatus = value; OnPropertyChanged(); }
        }


        private bool isLoading = true;

        public bool IsLoading
        {
            get { return isLoading; }
            set { isLoading = value; OnPropertyChanged(); }
        }

        private string keyword;

        public string Keyword
        {
            get { return keyword; }
            set { keyword = value; OnPropertyChanged(); }
        }


        private int pageIndex = 1;
        public int PageIndex
        {
            get => pageIndex;
            set { pageIndex = value; OnPropertyChanged(); }
        }

        private int maxPageCount = 1;

        public int MaxPageCount
        {
            get { return maxPageCount; }
            set { maxPageCount = value; OnPropertyChanged(); }
        }

        private int totalCount = 0;

        public int TotalCount
        {
            get { return totalCount; }
            set { totalCount = value; OnPropertyChanged(); }
        }


        private List<ProcessDataListDto> datas = new List<ProcessDataListDto>();

        public List<ProcessDataListDto> Datas
        {
            get { return datas; }
            set { datas = value; OnPropertyChanged(); }
        }

        private List<Product> products;

        public List<Product> Products
        {
            get { return products; }
            set { products = value; OnPropertyChanged(); }
        }


        private List<Technology> technologys;

        public List<Technology> Technologys
        {
            get { return technologys; }
            set { technologys = value; OnPropertyChanged(); }
        }

        private long? technologyId;

        public long? TechnologyId
        {
            get { return technologyId; }
            set { technologyId = value; OnPropertyChanged(); }
        }

        private long? productId;

        public long? ProductId
        {
            get { return productId; }
            set
            {
                productId = value; OnPropertyChanged();
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



        public ICommand LoadedCommand { get; set; }
        public ICommand ExportCommand { get; }
        public ICommand PageUpdatedCommand { get; set; }
        public ICommand QueryCommand { get; set; }
        public ICommand ResetKeywordCommand { get; set; }
        public QueryDataViewModel()
        {
            repo = new ProcessDataRepository();
            productRepository = new ProductRepository();
            technologyRepository = new TechnologyRepository();

            ExportCommand = new RelayCommand(async () =>
            {
                await OnExport();
            }, CanExport);
            QueryCommand = new RelayCommand(Query);
            ResetKeywordCommand = new RelayCommand(ResetKeyword);
            LoadedCommand = new RelayCommand(Loaded);
            PageUpdatedCommand = new RelayCommand<object>((args) =>
            {
                if (args is FunctionEventArgs<int> e)
                {
                    PageIndex = e.Info;
                    Query();
                }
            });

        }


        private void Loaded()
        {
            Task.Run(async () =>
            {
                SelectedStatus = false;
                var result = await productRepository.GetPageAsync(1, 1000);
                Products = result.Items;
                var result1 = await technologyRepository.GetPageAsync(1, 1000);
                Technologys = result1.Items;

            });
            Query();
        }

        private void ResetKeyword()
        {
            ProductId = 0;
            TechnologyId = 0;
            StartTime = DateTime.Today;                 // 今天 00:00:00
            EndTime = DateTime.Today.AddDays(1);      // 明天 00:00:00
            Query();
        }

        private void Query()
        {
            _ = Task.Run(async () =>
            {
               
                IsLoading = true;
                var result = await repo.GetPageAsyncGroupUuid(PageIndex, 20, StartTime, EndTime, productId: ProductId, technologyId: TechnologyId, status: SelectedStatus);
                IsLoading = false;
                TotalCount = result.TotalCount;
                // 总页数 = 总条数 / 每页条数 向上取整
                int totalPages = (int)Math.Ceiling(result.TotalCount / 20.0);
                MaxPageCount = totalPages;
                Datas = result.Items;
            });
        }

        private bool CanExport()
        {
            return Datas != null && Datas.Count > 0;
        }

        private async Task OnExport()
        {
            IsLoading = true;

            var columns = new Dictionary<string, string>
            {
                { "CreateTime", "生产时间" },
                { "Barcode", "条码" },
                { "ProductId", "产品编号" },
                { "ProductName", "产品名称" },
                { "TechnologyId", "工艺编号" },
                { "TechnologyName", "工艺名称" },

                { "M1", "一档直径" },
                { "M2", "二档直径" },
                { "M3", "三档直径" },
                { "M4", "四档直径" },
                { "M5", "五档直径" },

                { "M6", "一档圆度" },
                { "M7", "二档圆度" },
                { "M8", "三档圆度" },
                { "M9", "四档圆度" },
                { "M10", "五档圆度" },

                { "M11", "一档跳动" },
                { "M12", "二档跳动" },
                { "M13", "三档跳动" },
                { "M14", "四档跳动" },
                { "M15", "五档跳动" },

                { "Status", "状态" },
                { "GroupUuid", "分组编码" }
            };
            try
            {
                var resultData = await repo.GetPageAsyncGroupUuid(PageIndex, TotalCount, StartTime, EndTime, productId: ProductId, technologyId: TechnologyId, status: SelectedStatus);
                var result = await ExcelExportHelper.ExportAsync(
                                    resultData.Items,
                                    "ProcessData",
                                    columns
                                );
                IsLoading = false;
                if (result)
                {
                    Growl.Success("导出成功");
                }
                else
                {
                    Growl.Warning("导出失败");
                }
            }
            catch (Exception ex)
            {
                AppLog.Production.Error($"导出失败->{ex.Message}");
                Growl.Error("导出失败");
            }

        }
    }
}

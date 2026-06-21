using HandyControl.Controls;
using HandyControl.Data;
using Mysqlx;
using SDSMillimar.Common;
using SDSMillimar.Dtos;
using SDSMillimar.Models;
using SDSMillimar.Services;
using SDSMillimar.UserControls;
using SDSMillimar.Utils;
using SDSMillimar.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SDSMillimar.ViewModel
{
    internal class TechnologyViewModel : BaseViewModel
    {

        private readonly TechnologyRepository repo = null;
        private readonly DynamicTechnologyRepository dynamicTechnologyRepository = null;

        private bool toleranceColumnReadOnly = false;

        public bool ToleranceColumnReadOnly
        {
            get { return toleranceColumnReadOnly; }
            set { toleranceColumnReadOnly = value; OnPropertyChanged(); }
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

        private List<Technology> datas = new List<Technology>();

        public List<Technology> Datas
        {
            get { return datas; }
            set { datas = value; OnPropertyChanged(); }
        }

        private List<TechnologyParam> saveTPs;

        public List<TechnologyParam> SaveTPs
        {
            get { return saveTPs; }
            set { saveTPs = value; }
        }

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; OnPropertyChanged(); }
        }


        private List<TechnologyParamDto> technologyParamDatas = new List<TechnologyParamDto>() {
            new TechnologyParamDto{  IsDelete=true, ParamName="M1",DeviceIds="", ParamValue="一档直径", MeasureType=1 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M2",DeviceIds="", ParamValue="二档直径", MeasureType=1 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M3",DeviceIds="", ParamValue="三档直径", MeasureType=1 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M4",DeviceIds="", ParamValue="四档直径", MeasureType=1 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M5",DeviceIds="", ParamValue="五档直径", MeasureType=1 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M6",DeviceIds="", ParamValue="一档圆度", MeasureType=2 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M7",DeviceIds="", ParamValue="二档圆度", MeasureType=2 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M8",DeviceIds="", ParamValue="三档圆度", MeasureType=2 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M9",DeviceIds="", ParamValue="四档圆度", MeasureType=2 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M10",DeviceIds="", ParamValue="五档圆度", MeasureType=2},
            new TechnologyParamDto{  IsDelete=true, ParamName="M11", DeviceIds="",ParamValue="一档跳动", MeasureType=4},
            new TechnologyParamDto{  IsDelete=true, ParamName="M12", DeviceIds="",ParamValue="二档跳动", MeasureType=4},
            new TechnologyParamDto{  IsDelete=true, ParamName="M13", DeviceIds="",ParamValue="三档跳动", MeasureType=4},
            new TechnologyParamDto{  IsDelete=true, ParamName="M14",DeviceIds="", ParamValue="四档跳动", MeasureType=4},
            new TechnologyParamDto{  IsDelete=true, ParamName="M15",DeviceIds="", ParamValue="五档跳动", MeasureType=4},
        };

        public List<TechnologyParamDto> TechnologyParamDatas
        {
            get { return technologyParamDatas; }
            set { technologyParamDatas = value; OnPropertyChanged(); }
        }



        private bool isDrawerOpen = false;

        public bool IsDrawerOpen
        {
            get { return isDrawerOpen; }
            set { isDrawerOpen = value; OnPropertyChanged(); }
        }

        private bool isRefresh = false;

        public bool IsRefresh
        {
            get { return isRefresh; }
            set { isRefresh = value; OnPropertyChanged(); }
        }


        public ICommand LoadedCommand { get; set; }
        public ICommand AddProductedCommand { get; set; }
        public ICommand EditProductedCommand { get; set; }
        public ICommand DeleteProductedCommand { get; set; }
        public ICommand PageUpdatedCommand { get; set; }
        public ICommand QueryCommand { get; set; }
        public ICommand ResetKeywordCommand { get; set; }
        public ICommand ClosedCommand { get; set; }
        public ICommand AddMItemCommand { get; set; }
        public ICommand SaveMItemCommand { get; set; }
        public ICommand SelectedProbeCommand { get; set; }

        private Technology selectedData;
        public Technology SelectedData
        {
            get => selectedData;
            set
            {
                selectedData = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BtnControlCan));
            }
        }


        public bool BtnControlCan
        {
            get
            {
                return SelectedData != null;
            }
        }


        public TechnologyViewModel()
        {
            LoadedCommand = new RelayCommand(Loaded);
            QueryCommand = new RelayCommand(Query);
            ResetKeywordCommand = new RelayCommand(ResetKeyword);
            AddProductedCommand = new RelayCommand(AddProducted);
            EditProductedCommand = new RelayCommand(EditProducted);
            DeleteProductedCommand = new RelayCommand(DeleteProducted);
            ClosedCommand = new RelayCommand(() =>
            {
                if (IsRefresh)
                {
                    Messenger.Send("ModelSelectionShow", true);
                    IsRefresh = false;
                }
            });
            AddMItemCommand = new RelayCommand(async () =>
            {
                await AddMItem();
            });
            SaveMItemCommand = new RelayCommand(async () =>
            {
                await SaveMItem();
            });
            SelectedProbeCommand = new RelayCommand<TechnologyParamDto>(SelectedProbe);
            PageUpdatedCommand = new RelayCommand<object>((args) =>
            {
                if (args is FunctionEventArgs<int> e)
                {
                    PageIndex = e.Info;
                    Loaded();
                }
            });
            Messenger.Register<bool>("RefreshData", RefreshData);
            repo = new TechnologyRepository();
            dynamicTechnologyRepository = new DynamicTechnologyRepository();
        }



        private void SelectedProbe(TechnologyParamDto technologyParam)
        {
            GlobalSession.Instance.SetProbeSelect(string.Empty);
            GlobalSession.Instance.SetProbeSelect(technologyParam.DeviceIds);
            var probeView = new ProbeView();
            probeView.Owner = Application.Current.MainWindow;
            probeView.Closed += (s, e) =>
            {
                //SelectedTPData.DeviceIds =
                TechnologyParamDatas.Where(x => x.ParamName == technologyParam.ParamName).First().DeviceIds = string.Join(",", GlobalSession.Instance.Probes.Where(x => x.IsSelected)?.Select(x => x.Id));
            };
            probeView.ShowDialog();
        }

        private async Task SaveMItem()
        {
            var saves = TechnologyParamDatas;
            SaveTPs = new List<TechnologyParam>();
            var ss = saves.Where(x => !x.IsDelete && x.DeviceIds.Equals("")).ToList();
            if (ss.Count > 0)
            {
                var techNames = ss
                .Select(x => x.ParamName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()   // 如果不想重复，可以保留
                .ToList();
                Growl.Warning($"以下工艺未绑定测量设备：\n{string.Join("\n", techNames)}");
                return;
            }
            int count = saves.Count(x => x.IsDelete == false);
            if (count == 0)
            {
                Growl.Warning("启用测量项为0, 不能保存");
                return;
            }
            for (int i = 0; i < saves.Count; i++)
            {
                TechnologyParam technologyParam = new TechnologyParam
                {
                    Id = saves[i].Id,
                    DeviceIds = saves[i].DeviceIds,
                    LowerTolerance = saves[i].LowerTolerance,
                    MeasureType = saves[i].MeasureType,
                    ParamName = saves[i].ParamName,
                    ParamValue = saves[i].ParamValue,
                    TargetValue = saves[i].TargetValue,
                    FilterValue = saves[i].FilterValue,
                    CompensationValue = saves[i].CompensationValue,
                    IsDelete = saves[i].IsDelete,
                    Sort = i + 1,
                    UpperTolerance = saves[i].UpperTolerance,
                    TechnologyId = SelectedData.Id,
                    TechnologyName = SelectedData.TechnologyName
                };
                if (!technologyParam.IsDelete)
                {
                    technologyParam.Validate(out string error);
                    if (!string.IsNullOrEmpty(error))
                    {
                        Growl.Warning(error);
                        return;
                    }
                }
                SaveTPs.Add(technologyParam);
            }
            ;
            var result = await dynamicTechnologyRepository.AddOrUpdateRangeAsync(SaveTPs, SelectedData.Id);
            if (result > 0)
            {
                ResetData();
            }
            RefreshData(true);
            IsDrawerOpen = false;
        }

        private async Task AddMItem()
        {
            IsDrawerOpen = true;
           
            var datas = await dynamicTechnologyRepository.GetByTechnologyIdAsync(SelectedData.Id);
            if (datas != null && datas.Count > 0)
            {
                Title = $"编辑【{SelectedData.TechnologyName}】-测量项";
                for (int i = 0; i < datas.Count; i++)
                {
                    var dbItem = datas[i];
                    // 找到原来的 DTO
                    var dto = technologyParamDatas.FirstOrDefault(x => x.ParamName == dbItem.ParamName);
                    if (dto != null)
                    {
                        dto.Id = dbItem.Id;
                        dto.DeviceIds = dbItem.DeviceIds;
                        dto.LowerTolerance = dbItem.LowerTolerance;
                        dto.MeasureType = dbItem.MeasureType;
                        dto.ParamName = dbItem.ParamName;
                        dto.ParamValue = dbItem.ParamValue;
                        dto.TargetValue = dbItem.TargetValue;
                        dto.FilterValue = dbItem.FilterValue;
                        dto.CompensationValue = dbItem.CompensationValue;
                        dto.IsDelete = dbItem.IsDelete;
                        dto.Sort = dbItem.Sort;
                        dto.UpperTolerance = dbItem.UpperTolerance;
                    }

                }
                OnPropertyChanged(nameof(TechnologyParamDatas));
            }
            else
            {
                //ToleranceColumnReadOnly = SelectedData.TechnologyType == 2 ? true : false;
                Title = $"新增【{SelectedData.TechnologyName}】-测量项";
                for (int i = 0; i < TechnologyParamDatas.Count; i++)
                {
                    TechnologyParamDatas[i].IsDelete = true;
                }

                // 测量
                if (SelectedData.TechnologyType == 1)
                {
                    UpdateByMeasureType(1, x => { x.UpperTolerance = 0; x.LowerTolerance = 0; });
                    UpdateByMeasureType(2, x => { x.UpperTolerance = 0.01; x.LowerTolerance = 0; });
                    UpdateByMeasureType(4, x => { x.UpperTolerance = 0.15; x.LowerTolerance = 0; });
                }
                else
                {

                    UpdateByMeasureType(1, x => { x.UpperTolerance = 0.002; x.LowerTolerance = -0.002; });
                    UpdateByMeasureType(2, x => { x.UpperTolerance = 0; x.LowerTolerance = 0; });
                    UpdateByMeasureType(4, x => { x.UpperTolerance = 0; x.LowerTolerance = 0; });

                }
            }
        }

        public void UpdateByMeasureType(int measureType, Action<TechnologyParamDto> updateAction)
        {
            foreach (var item in TechnologyParamDatas.Where(x => x.MeasureType == measureType))
            {
                updateAction(item);
            }

            OnPropertyChanged(nameof(TechnologyParamDatas));
        }

        private void ResetKeyword()
        {
            Keyword = string.Empty;
            Query();
        }

        private void RefreshData(bool isRefresh)
        {

            if (isRefresh)
            {
                IsRefresh = true;
                Loaded();
            }

        }

        private void AddProducted()
        {
            var view = new TechnologyDetailControl(null);
            view.Owner = Application.Current.MainWindow;
            view.ShowDialog();
        }

        private void DeleteProducted()
        {
            Task.Run(async () =>
            {
                try
                {
                    var result = await repo.SoftDeleteAsync(SelectedData.Id);
                    if (result > 0)
                    {
                        Growl.Success("删除成功", DialogTokens.DynamicTechnologyView);
                        Loaded();
                    }
                }
                catch (Exception ex)
                {
                    Growl.Error($"删除失败:{ex.Message}", DialogTokens.DynamicTechnologyView);
                }

            });
        }

        private void ResetData()
        {
            TechnologyParamDatas = new List<TechnologyParamDto>() {
            new TechnologyParamDto{  IsDelete=true, ParamName="M1",DeviceIds="", ParamValue="一档直径", MeasureType=1 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M2",DeviceIds="", ParamValue="二档直径", MeasureType=1 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M3",DeviceIds="", ParamValue="三档直径", MeasureType=1 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M4",DeviceIds="", ParamValue="四档直径", MeasureType=1 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M5",DeviceIds="", ParamValue="五档直径", MeasureType=1 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M6",DeviceIds="", ParamValue="一档圆度", MeasureType=2 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M7",DeviceIds="", ParamValue="二档圆度", MeasureType=2 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M8",DeviceIds="", ParamValue="三档圆度", MeasureType=2 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M9",DeviceIds="", ParamValue="四档圆度", MeasureType=2 },
            new TechnologyParamDto{  IsDelete=true, ParamName="M10",DeviceIds="", ParamValue="五档圆度", MeasureType=2},
            new TechnologyParamDto{  IsDelete=true, ParamName="M11", DeviceIds="",ParamValue="一档跳动", MeasureType=4},
            new TechnologyParamDto{  IsDelete=true, ParamName="M12", DeviceIds="",ParamValue="二档跳动", MeasureType=4},
            new TechnologyParamDto{  IsDelete=true, ParamName="M13", DeviceIds="",ParamValue="三档跳动", MeasureType=4},
            new TechnologyParamDto{  IsDelete=true, ParamName="M14",DeviceIds="", ParamValue="四档跳动", MeasureType=4},
            new TechnologyParamDto{  IsDelete=true, ParamName="M15",DeviceIds="", ParamValue="五档跳动", MeasureType=4},
        };
        }
        private void EditProducted()
        {
            var view = new TechnologyDetailControl(SelectedData);
            view.Owner = Application.Current.MainWindow;
            view.ShowDialog();
        }

        private void Loaded()
        {
            Query();
        }

        private void Query()
        {
            _ = Task.Run(async () =>
            {
                IsLoading = true;
                var result = string.IsNullOrEmpty(Keyword) ? await repo.GetPageAsync(PageIndex, 10) : await repo.GetPageAsync(PageIndex, 10, Keyword);
                IsLoading = false;
                // 总页数 = 总条数 / 每页条数 向上取整
                int totalPages = (int)Math.Ceiling(result.TotalCount / 10.0);
                MaxPageCount = totalPages;
                Datas = result.Items;
            });

        }
    }
}

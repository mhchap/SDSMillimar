using HandyControl.Controls;
using SDSMillimar.Common;
using SDSMillimar.Models;
using SDSMillimar.Services;
using SDSMillimar.Utils;
using SDSMillimar.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SDSMillimar.UserControls.ViewModel
{
    internal class DynamicTechnologyDetailViewModel : BaseViewModel
    {
        private TechnologyRepository technologyRepository = null;
        private DynamicTechnologyRepository dynamicTechnologyRepository = null;
        private ObservableCollection<TagDataModel> dataList;

        public ObservableCollection<TagDataModel> DataList
        {
            get { return dataList; }
            set { dataList = value; OnPropertyChanged(); }
        }

        private string selectProductedId;

        public string SelectProductedId
        {
            get { return selectProductedId; }
            set
            {
                selectProductedId = value;
                OnPropertyChanged();
            }
        }


        private TechnologyParam technologyParamInfo;

        public TechnologyParam TechnologyParamInfo
        {
            get { return technologyParamInfo; }
            set
            {
                technologyParamInfo = value;
                if (technologyParamInfo.Id == 0)
                    TitleText = "新增动态工艺参数";
                else
                    TitleText = $"编辑[{technologyParamInfo.ParamValue}]";
                OnPropertyChanged();
            }
        }

        private List<MeasureType> measureTypes = new List<MeasureType> { new MeasureType { Id = 1, Name = "直径" }, new MeasureType { Id = 2, Name = "圆度" }, new MeasureType { Id = 3, Name = "圆柱度" }, new MeasureType { Id = 4, Name = "跳动" } };

        public List<MeasureType> MeasureTypes
        {
            get { return measureTypes; }
            set { measureTypes = value; OnPropertyChanged(); }
        }


        private List<Technology> technologys;

        public List<Technology> Technologys
        {
            get { return technologys; }
            set { technologys = value; OnPropertyChanged(); }
        }


        private MeasureType selectedMeasureType;

        public MeasureType SelectedMeasureType
        {
            get { return selectedMeasureType; }
            set
            {
                selectedMeasureType = value;
                OnPropertyChanged();
            }
        }


        private Technology selectedTechnology;

        public Technology SelectedTechnology
        {
            get { return selectedTechnology; }
            set
            {
                selectedTechnology = value;
                TechnologyParamInfo.TechnologyName = selectedTechnology?.TechnologyName;
                OnPropertyChanged();
            }
        }

        private string titleText;

        public string TitleText
        {
            get { return titleText; }
            set { titleText = value; OnPropertyChanged(); }
        }


        private bool isAdd = true;

        public bool IsAdd
        {
            get { return isAdd; }
            set { isAdd = value; OnPropertyChanged(); }
        }

        public ICommand ClosedCommand { get; set; }
        public ICommand SubmitCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand SelectProbeCommand { get; set; }
        public DynamicTechnologyDetailViewModel(TechnologyParam info)
        {
            IsAdd = info == null;
            TechnologyParamInfo = info ?? new TechnologyParam();
            if (!string.IsNullOrEmpty(TechnologyParamInfo.DeviceIds))
            {
                var ids = TechnologyParamInfo.DeviceIds.Split(',');
                DataList = new ObservableCollection<TagDataModel>();
                for (int i = 0; i < ids.Length; i++)
                {
                    ProbeItem item = GlobalSession.Instance.Probes.Where(x => x.Id == Convert.ToInt64(ids[i])).FirstOrDefault();
                    if (item == null)
                        continue;
                    DataList.Add(new TagDataModel
                    {
                        Id = Convert.ToInt64(ids[i]),
                        IsSelected = true,
                        Name = item.Title
                    });
                    GlobalSession.Instance.Probes.Where(x => x.Id == Convert.ToInt64(ids[i])).First().IsSelected = true;
                }
            }
            else
            {
                foreach (var probe in GlobalSession.Instance.Probes)
                {
                    probe.IsSelected = false;
                }
                ;
            }
            technologyRepository = new TechnologyRepository();
            dynamicTechnologyRepository = new DynamicTechnologyRepository();
            SubmitCommand = new RelayCommand(async () => await SubmitAsync());
            ClosedCommand = new RelayCommand(Closed);
            LoadedCommand = new RelayCommand(Loaded);
            SelectProbeCommand = new RelayCommand(SelectProbe);
            Messenger.Register<ObservableCollection<ProbeItem>>("GetSelectedProbes", GetSelectedProbes);
        }

        private void GetSelectedProbes(ObservableCollection<ProbeItem> collection)
        {
            DataList = new ObservableCollection<TagDataModel>();
            for (int i = 0; i < collection.Count; i++)
            {
                DataList.Add(new TagDataModel
                {
                    Id = collection[i].Id,
                    IsSelected = true,
                    Name = collection[i].Title
                });
            }
        }

        private void SelectProbe()
        {
            var probeView = new ProbeView();
            probeView.Owner = Application.Current.MainWindow;
           
            probeView.ShowDialog();

        }

        private void Loaded()
        {
            Task.Run(async () =>
            {
                var result = await technologyRepository.GetPageAsync(1, 1000);
                Technologys = result.Items;

            });
        }

        private async Task SubmitAsync()
        {
            try
            {
                if (DataList != null)
                    TechnologyParamInfo.DeviceIds = string.Join(",", DataList?.Select(x => x.Id));
                TechnologyParamInfo.Validate(out string error);
                if (!string.IsNullOrEmpty(error))
                {
                    Growl.Warning(error);
                    return;
                }
                if (DataList.Count != 2)
                {

                    Growl.Warning("请选择2个探针");
                    return;
                }

                var result = IsAdd ? await dynamicTechnologyRepository.AddAsync(TechnologyParamInfo) : await dynamicTechnologyRepository.UpdateAsync(TechnologyParamInfo);
                if (result > 0)
                {
                    Growl.Success("提交成功");
                    Closed();
                }
            }
            catch (Exception ex)
            {
                Growl.Error($"提交失败:\n{ex.Message}");
            }
            finally
            {
                Messenger.Send("RefreshData", true);
            }
        }




        private void Closed()
        {
            Dialog.Close(DialogTokens.DynamicTechnologyView);
        }
    }
}

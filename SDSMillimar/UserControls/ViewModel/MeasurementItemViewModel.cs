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
    public class MeasurementItemViewModel : BaseViewModel
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

        private List<TechnologyParam> datas = new List<TechnologyParam>() {
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M1", ParamValue="一档直径", MeasureType=1, UpdateTime=DateTime.Now },
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M2", ParamValue="二档直径", MeasureType=1, UpdateTime=DateTime.Now },
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M3", ParamValue="三档直径", MeasureType=1, UpdateTime=DateTime.Now },
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M4", ParamValue="四档直径", MeasureType=1, UpdateTime=DateTime.Now },
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M5", ParamValue="五档直径", MeasureType=1, UpdateTime=DateTime.Now },
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M6", ParamValue="一档圆度", MeasureType=2, UpdateTime=DateTime.Now },
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M7", ParamValue="二档圆度", MeasureType=2, UpdateTime=DateTime.Now },
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M8", ParamValue="三档圆度", MeasureType=2, UpdateTime=DateTime.Now },
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M9", ParamValue="四档圆度", MeasureType=2, UpdateTime=DateTime.Now },
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M10", ParamValue="五档圆度", MeasureType=2, UpdateTime=DateTime.Now },
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M11", ParamValue="一档跳动", MeasureType=3, UpdateTime=DateTime.Now },
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M12", ParamValue="二档跳动", MeasureType=3, UpdateTime=DateTime.Now },
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M13", ParamValue="三档跳动", MeasureType=3, UpdateTime=DateTime.Now },
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M14", ParamValue="四档跳动", MeasureType=3, UpdateTime=DateTime.Now },
            new TechnologyParam{ CreateTime = DateTime.Now, IsDelete=false, ParamName="M15", ParamValue="五档跳动", MeasureType=3, UpdateTime=DateTime.Now },
        };

        public List<TechnologyParam> Datas
        {
            get { return datas; }
            set { datas = value; OnPropertyChanged(); }
        }


        public ICommand ClosedCommand { get; set; }
        public ICommand SubmitCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand SelectProbeCommand { get; set; }
        public MeasurementItemViewModel()
        {

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

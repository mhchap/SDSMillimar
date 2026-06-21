using SDSMillimar.Common;
using SDSMillimar.Utils;
using SDSMillimar.View;
using System.Windows;
using System.Windows.Input;

namespace SDSMillimar.ViewModel
{
    public partial class MainViewModel : BaseViewModel
    {


        private object currentView;

        public object CurrentView
        {
            get { return currentView; }
            set { currentView = value; OnPropertyChanged(); }
        }


        public ICommand MenuCommand { get; set; }
        public MainViewModel()
        {
            NavigateToTestItem(false);
            MenuCommand = new RelayCommand<string>(MenuClick);

        }

        private void NavigateToTestItem(bool isCgkMeasurement)
        {
            GlobalSession.Instance.IsPull = isCgkMeasurement ? 1 : 0;

            // 测量 ViewModel 会订阅设备、PLC 和 Messenger 事件。
            // 已经位于测量页面时不要重复创建，避免同一事件被处理多次。
            if (!(CurrentView is TestItemViewModel))
                CurrentView = new TestItemViewModel();
        }

        private void MenuClick(string menu)
        {
            switch (menu)
            {
                case "Home":
                    NavigateToTestItem(false);
                    break;
                case "CGKMeasurement":
                    NavigateToTestItem(true);
                    break;
                case "Product":
                    var productedView = new ProductedView();
                    productedView.Owner = Application.Current.MainWindow;
                    productedView.ShowDialog();
                    break;
                case "Technology":
                    var technologyView = new TechnologyView();
                    technologyView.Owner = Application.Current.MainWindow;
                    technologyView.ShowDialog();

                    //CurrentView = new TechnologyViewModel();
                    break;
                case "DynamicTechnology":
                    var dynamicTechnology = new DynamicTechnologyView();
                    dynamicTechnology.Owner = Application.Current.MainWindow;
                    dynamicTechnology.ShowDialog();
                    break;
                case "ModelSelection":
                    var modelSelectionView = new ModelSelectionView();
                    modelSelectionView.Owner = Application.Current.MainWindow;
                    modelSelectionView.ShowDialog();
                    break;
                case "QueryData":
                    var queryDataView = new QueryDataView();
                    queryDataView.Owner = Application.Current.MainWindow;
                    queryDataView.ShowDialog();
                    break;
                case "SPCAnalyse":
                    var spcAnalyse = new SPCAnalyseView();
                    spcAnalyse.Owner = Application.Current.MainWindow;
                    spcAnalyse.ShowDialog();
                    break;
                case "ProbeData":
                    GlobalSession.Instance.IsTestItem = false;
                    var probeDataView = new ProbeDataView();
                    probeDataView.Owner = Application.Current.MainWindow;
                    probeDataView.ShowDialog();
                    break;
                default:
                    break;
            }
        }
    }
}

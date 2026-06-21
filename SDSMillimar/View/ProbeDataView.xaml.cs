using SDSMillimar.Common;
using SDSMillimar.Utils;
using SDSMillimar.ViewModel;
using System;
using System.Windows;

namespace SDSMillimar.View
{
    /// <summary>
    /// ProbeDataView.xaml 的交互逻辑
    /// </summary>
    public partial class ProbeDataView : Window
    {
        public ProbeDataView()
        {
            InitializeComponent();
            this.DataContext = new ProbeDataViewModel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GlobalSession.Instance.IsTestItem = true;
            MillN1700Helper.Instance.N1700StopContinuousRequestAllData();
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (DataContext is ProbeDataViewModel vm)
            {
                if (vm.IsRefresh)
                {
                    Messenger.Send("ModelSelectionShow", true);
                    vm.IsRefresh = false;
                }
            }
        }
    }
}

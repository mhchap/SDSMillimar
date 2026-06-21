using SDSMillimar.ViewModel;
using System.Windows;

namespace SDSMillimar.View
{
    /// <summary>
    /// ProbeView.xaml 的交互逻辑
    /// </summary>
    public partial class ProbeView : Window
    {
        public ProbeView()
        {
            InitializeComponent();
            var vm = new ProbeViewModel();
            vm.RequestClose = () => this.Close();
            this.DataContext = vm;
        }
    }
}

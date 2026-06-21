using SDSMillimar.ViewModel;
using System.Windows;

namespace SDSMillimar.View
{
    /// <summary>
    /// ModelSelectionView.xaml 的交互逻辑
    /// </summary>
    public partial class ModelSelectionView : Window
    {
        public ModelSelectionView()
        {
            InitializeComponent();
            var vm = new ModelSelectionViewModel();
            vm.RequestClose = () => this.Close();
            this.DataContext = vm;
        }
    }
}

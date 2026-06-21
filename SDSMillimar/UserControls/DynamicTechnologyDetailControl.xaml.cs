using SDSMillimar.Models;
using SDSMillimar.UserControls.ViewModel;
using System.Windows.Controls;

namespace SDSMillimar.UserControls
{
    /// <summary>
    /// DynamicTechnologyDetailControl.xaml 的交互逻辑
    /// </summary>
    public partial class DynamicTechnologyDetailControl : UserControl
    {
        public DynamicTechnologyDetailControl(TechnologyParam technologyParam)
        {
            InitializeComponent();
            this.DataContext = new DynamicTechnologyDetailViewModel(technologyParam);
        }
    }
}

using SDSMillimar.Models;
using SDSMillimar.UserControls.ViewModel;
using System.Windows.Controls;

namespace SDSMillimar.UserControls
{
    /// <summary>
    /// ProductedDetailControl.xaml 的交互逻辑
    /// </summary>
    public partial class ProductedDetailControl : UserControl
    {
        public ProductedDetailControl(Product productedInfo)
        {
            InitializeComponent();
            this.DataContext = new ProductedDetailViewModel(productedInfo);
        }
    }
}

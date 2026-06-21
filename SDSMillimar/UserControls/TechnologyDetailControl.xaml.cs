using SDSMillimar.Models;
using SDSMillimar.UserControls.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SDSMillimar.UserControls
{
    /// <summary>
    /// TechnologyDetailControl.xaml 的交互逻辑
    /// </summary>
    public partial class TechnologyDetailControl : Window
    {
        public TechnologyDetailControl(Technology technology)
        {
            InitializeComponent();

            var vm = new TechnologyDetailViewModel(technology);
            vm.RequestClose = () => this.Close();
            this.DataContext = vm;
            //this.DataContext = new TechnologyDetailViewModel(technology);
        }
    }
}

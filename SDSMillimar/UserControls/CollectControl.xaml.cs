using System.Windows;
using System.Windows.Controls;

namespace SDSMillimar.UserControls
{
    /// <summary>
    /// CollectControl.xaml 的交互逻辑
    /// </summary>
    public partial class CollectControl : UserControl
    {
        public CollectControl()
        {
            InitializeComponent();
        }
        // 左上角文本
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(CollectControl), new PropertyMetadata(""));

        // 右上角单位
        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(CollectControl), new PropertyMetadata(""));

        // 下方值
        public string ValueTextA
        {
            get { return (string)GetValue(ValueTextAProperty); }
            set { SetValue(ValueTextAProperty, value); }
        }
        public static readonly DependencyProperty ValueTextAProperty =
            DependencyProperty.Register("ValueTextA", typeof(string), typeof(CollectControl), new PropertyMetadata("---"));

        public string ValueTextB
        {
            get { return (string)GetValue(ValueTextBProperty); }
            set { SetValue(ValueTextBProperty, value); }
        }
        public static readonly DependencyProperty ValueTextBProperty =
            DependencyProperty.Register("ValueTextB", typeof(string), typeof(CollectControl), new PropertyMetadata("---"));

    }
}

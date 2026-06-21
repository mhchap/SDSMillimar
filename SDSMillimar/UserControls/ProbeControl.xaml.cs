using SDSMillimar.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SDSMillimar.UserControls
{
    /// <summary>
    /// ProbeControl.xaml 的交互逻辑
    /// </summary>
    public partial class ProbeControl : UserControl
    {
        public ProbeControl()
        {
            InitializeComponent();
        }



        public static readonly DependencyProperty ClickCommandProperty =
    DependencyProperty.Register(
        nameof(ClickCommand),
        typeof(ICommand),
        typeof(ProbeControl),
        new PropertyMetadata(null));

        public ICommand ClickCommand
        {
            get => (ICommand)GetValue(ClickCommandProperty);
            set => SetValue(ClickCommandProperty, value);
        }


        public static readonly DependencyProperty ClickCommandParameterProperty =
    DependencyProperty.Register(
        nameof(ClickCommandParameter),
        typeof(object),
        typeof(ProbeControl),
        new PropertyMetadata(null));

        public object ClickCommandParameter
        {
            get => GetValue(ClickCommandParameterProperty);
            set => SetValue(ClickCommandParameterProperty, value);
        }



        public Brush ProbeBackground
        {
            get => (Brush)GetValue(ProbeBackgroundProperty);
            set => SetValue(ProbeBackgroundProperty, value);
        }

        public static readonly DependencyProperty ProbeBackgroundProperty =
            DependencyProperty.Register(
                nameof(ProbeBackground),
                typeof(Brush),
                typeof(ProbeControl),
                new PropertyMetadata(Brushes.LightGray));

        // 左上角文本
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ProbeControl), new PropertyMetadata(""));

        // 右上角单位
        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(ProbeControl), new PropertyMetadata(""));


        // 1 向右，-1 向左
        public int Direction
        {
            get => (int)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register(nameof(Direction), typeof(int),
                typeof(ProbeControl), new PropertyMetadata(1));



        public OptionsModel SelectedChannel
        {
            get => (OptionsModel)GetValue(SelectedChannelProperty);
            set => SetValue(ChannelEnableProperty, value);
        }

        public static readonly DependencyProperty SelectedChannelProperty =
            DependencyProperty.Register(nameof(SelectedChannel), typeof(OptionsModel),
                typeof(ProbeControl), new PropertyMetadata(null));


        public bool ChannelEnable
        {
            get => (bool)GetValue(ChannelEnableProperty);
            set => SetValue(ChannelEnableProperty, value);
        }

        public static readonly DependencyProperty ChannelEnableProperty =
            DependencyProperty.Register(nameof(ChannelEnable), typeof(bool),
                typeof(ProbeControl), new PropertyMetadata(false));


        public int ChannelIndex
        {
            get => (int)GetValue(ChannelIndexProperty);
            set => SetValue(ChannelIndexProperty, value);
        }

        public static readonly DependencyProperty ChannelIndexProperty =
            DependencyProperty.Register(nameof(ChannelIndex), typeof(int),
                typeof(ProbeControl), new PropertyMetadata(0));


        public Brush RodBrush
        {
            get => (Brush)GetValue(RodBrushProperty);
            set => SetValue(RodBrushProperty, value);
        }

        public static readonly DependencyProperty RodBrushProperty =
            DependencyProperty.Register(nameof(RodBrush), typeof(Brush),
                typeof(ProbeControl), new PropertyMetadata(Brushes.Gray));

        public Brush ArrowBrush
        {
            get => (Brush)GetValue(ArrowBrushProperty);
            set => SetValue(ArrowBrushProperty, value);
        }

        public static readonly DependencyProperty ArrowBrushProperty =
            DependencyProperty.Register(nameof(ArrowBrush), typeof(Brush),
                typeof(ProbeControl), new PropertyMetadata(Brushes.Black));

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ClickCommand?.CanExecute(ClickCommandParameter) == true)
            {
                ClickCommand.Execute(ClickCommandParameter);
            }
        }
    }
}

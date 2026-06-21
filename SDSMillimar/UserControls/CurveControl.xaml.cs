using LiveCharts;
using LiveCharts.Wpf;
using SDSMillimar.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SDSMillimar.UserControls
{
    /// <summary>
    /// CurveControl.xaml 的交互逻辑
    /// </summary>
    public partial class CurveControl : UserControl
    {
        public CurveControl()
        {
            InitializeComponent();
        }
        public double XMin
        {
            get => (double)GetValue(XMinProperty);
            set => SetValue(XMinProperty, value);
        }

        public static readonly DependencyProperty XMinProperty =
            DependencyProperty.Register("XMin", typeof(double), typeof(CurveControl), new PropertyMetadata(0d));

        public double XMax
        {
            get => (double)GetValue(XMaxProperty);
            set => SetValue(XMaxProperty, value);
        }

        public static readonly DependencyProperty XMaxProperty =
            DependencyProperty.Register("XMax", typeof(double), typeof(CurveControl), new PropertyMetadata(10d));



        public ObservableCollection<LegendItemModel> LegendItems
        {
            get { return (ObservableCollection<LegendItemModel>)GetValue(LegendItemsProperty); }
            set { SetValue(LegendItemsProperty, value); }
        }

        public static readonly DependencyProperty LegendItemsProperty =
            DependencyProperty.Register("LegendItems", typeof(ObservableCollection<LegendItemModel>), typeof(CurveControl), new PropertyMetadata(new ObservableCollection<LegendItemModel>()));

        public SeriesCollection SeriesData
        {
            get { return (SeriesCollection)GetValue(SeriesDataProperty); }
            set { SetValue(SeriesDataProperty, value); }
        }

        public static readonly DependencyProperty SeriesDataProperty =
            DependencyProperty.Register("SeriesData", typeof(SeriesCollection), typeof(CurveControl), new PropertyMetadata(new SeriesCollection()));

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is LegendItemModel item)
            {
                if (item.Title.Equals("全部显示"))
                {
                    for (int i = 0; i < item.seriesCollection.Count; i++)
                    {
                        var series = (LineSeries)item.seriesCollection[i];
                        series.Visibility = Visibility.Visible;

                        //     series.Visibility == Visibility.Visible
                        //? Visibility.Hidden
                        //: Visibility.Visible;
                    }
                }
                else if (item.Title.Equals("全部隐藏"))
                {
                    for (int i = 0; i < item.seriesCollection.Count; i++)
                    {
                        var series = (LineSeries)item.seriesCollection[i];
                        series.Visibility = Visibility.Hidden;

                        //     series.Visibility == Visibility.Visible
                        //? Visibility.Hidden
                        //: Visibility.Visible;
                    }
                }
                else
                {

                    var series = item.Series;
                    series.Visibility = series.Visibility == Visibility.Visible
                        ? Visibility.Hidden
                        : Visibility.Visible;
                }
            }
        }
    }
}

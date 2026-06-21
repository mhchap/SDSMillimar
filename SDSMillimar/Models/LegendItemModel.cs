using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace SDSMillimar.Models
{
    public class LegendItemModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public Brush Color { get; set; }
        public LineSeries Series { get; set; } // 对应 Chart 的折线
        public SeriesCollection seriesCollection { get; set; }

        private Visibility _isVisible = Visibility.Visible;
        public Visibility IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    Series.Visibility = value;  // 控制折线显示/隐藏
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

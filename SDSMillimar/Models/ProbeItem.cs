using SDSMillimar.Common;
using SDSMillimar.Utils;
using System.ComponentModel;
using System.Windows.Media;

namespace SDSMillimar.Models
{
    public class ProbeItem : BaseViewModel
    {
        public long Id { get; set; }     // A / B / C
        public string Title { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProbeBackground));
                OnPropertyChanged(nameof(Unit));
            }
        }

        public int Direction { get; set; }

        public int GridRow { get; set; }
        public int GridColumn { get; set; }

        private double measuredValue;

        public double MeasuredValue
        {
            get { return measuredValue; }
            set
            {
                measuredValue = value;
                OnPropertyChanged();

            }
        }




        // 根据选中状态派生 UI
        public Brush ProbeBackground =>
            IsSelected ? new SolidColorBrush(Color.FromRgb(0x09, 0x5F, 0xA8)) : new SolidColorBrush(Color.FromRgb(0xD0, 0xC9, 0xC9));

        public string Unit =>
            IsSelected ? "已选择" : "";
    }
}

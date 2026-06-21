using System.ComponentModel;

namespace SDSMillimar.Models
{
    public class OptionsModel : INotifyPropertyChanged
    {
        public int Key { get; set; }
        public int Value { get; set; }

        private bool isSelect = false;

        public bool IsSelect
        {
            get { return isSelect; }
            set
            {
                isSelect = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelect)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

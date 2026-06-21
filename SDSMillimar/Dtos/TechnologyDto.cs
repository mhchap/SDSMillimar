using System.Collections.Generic;
using System.ComponentModel;

namespace SDSMillimar.Dtos
{
    public class TechnologyDto : INotifyPropertyChanged
    {
        public long TechnologyID { get; set; }
        public int TechnologyType { get; set; }
        public int IsOilGroove { get; set; }
        public string TechnologyCode { get; set; }

        private string technologyName;

        public string TechnologyName
        {
            get { return technologyName; }
            set
            {
                technologyName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TechnologyName)));
            }
        }

        public List<TechnologyParamDto> Params { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

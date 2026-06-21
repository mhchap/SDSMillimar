using SDSMillimar.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDSMillimar.Models
{
    public class ProbeGroup : BaseViewModel
    {
        public string Ids { get; set; }     // A / B / C
        public string TitleA { get; set; }
        public string TitleB { get; set; }

        private double measuredValueA;

        public double MeasuredValueA
        {
            get { return measuredValueA; }
            set
            {
                measuredValueA = value;
                OnPropertyChanged();
            }
        }

        private double measuredValueB;

        public double MeasuredValueB
        {
            get { return measuredValueB; }
            set
            {
                measuredValueB = value;
                OnPropertyChanged();
            }
        }
    }
}

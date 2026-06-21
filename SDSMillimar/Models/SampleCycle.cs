using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDSMillimar.Models
{
    public class SampleCycle
    {
        public DateTime Time { get; set; } = DateTime.Now;

        public List<SampleFrame> Frames { get; set; } = new List<SampleFrame>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDSMillimar.Models
{
    /// <summary>
    /// 采样数据
    /// </summary>
    public class SampleFrame
    {
        public int FrameIndex { get; set; }   // 第几组设备
        public DateTime Time { get; set; } = DateTime.Now;
        public List<ProbePoint> APoints { get; } = new List<ProbePoint>();
        public List<ProbePoint> BPoints { get; } = new List<ProbePoint>();
    }
}

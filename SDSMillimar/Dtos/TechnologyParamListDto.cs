using System;

namespace SDSMillimar.Dtos
{
    public class TechnologyParamListDto
    {
        public long Id { get; set; }
        public long TechnologyId { get; set; }
        public string TechnologyName { get; set; }  // 汉字显示工艺名称
        public string ParamName { get; set; }
        public string ParamValue { get; set; }
        public double TargetValue { get; set; }
        public double UpperTolerance { get; set; }
        public double LowerTolerance { get; set; }
        public int MeasureType { get; set; }
        public string DeviceIds { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }


}

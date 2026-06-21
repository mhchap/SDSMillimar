using System;

namespace SDSMillimar.Dtos
{
    public class ProcessDataListDto
    {
        public long Id { get; set; }

        public string Barcode { get; set; }

        public long ProductId { get; set; }
        public string ProductName { get; set; }

        public long TechnologyId { get; set; }
        public string TechnologyName { get; set; }

        public string ParamValue { get; set; }

        public double TargetValue { get; set; }
        public double UpperTolerance { get; set; }
        public double LowerTolerance { get; set; }

        public int MeasureType { get; set; }
        public string MeasureTypeText
        {
            get
            {
                switch (MeasureType)
                {
                    case 1: return "直径";
                    case 2: return "圆度";
                    case 3: return "圆柱度";
                    case 4: return "跳动";
                    default: return "未知";
                }
            }
        }

        public double MeasureValue { get; set; }
        public string GroupUuid { get; set; }
        public bool Status { get; set; }
        public bool IsDelete { get; set; }

        // 直径
        // 直径
        public double? M1 { get; set; }
        public double? M2 { get; set; }
        public double? M3 { get; set; }
        public double? M4 { get; set; }
        public double? M5 { get; set; }

        // 圆度
        public double? M6 { get; set; }
        public double? M7 { get; set; }
        public double? M8 { get; set; }
        public double? M9 { get; set; }
        public double? M10 { get; set; }

        // 跳动
        public double? M11 { get; set; }
        public double? M12 { get; set; }
        public double? M13 { get; set; }
        public double? M14 { get; set; }
        public double? M15 { get; set; }


        public bool? M1Status { get; set; }
        public bool? M2Status { get; set; }
        public bool? M3Status { get; set; }
        public bool? M4Status { get; set; }
        public bool? M5Status { get; set; }

        public bool? M6Status { get; set; }
        public bool? M7Status { get; set; }
        public bool? M8Status { get; set; }
        public bool? M9Status { get; set; }
        public bool? M10Status { get; set; }

        public bool? M11Status { get; set; }
        public bool? M12Status { get; set; }
        public bool? M13Status { get; set; }
        public bool? M14Status { get; set; }
        public bool? M15Status { get; set; }

        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }


}

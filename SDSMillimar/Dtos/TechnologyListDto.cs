using System;

namespace SDSMillimar.Dtos
{
    public class TechnologyListDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }   // 显示汉字
        public string TechnologyCode { get; set; }
        public string TechnologyName { get; set; }
        public int TechnologyType { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}

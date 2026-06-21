using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDSMillimar.Models
{
    [Table("process_data")]
    public class ProcessData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        /// <summary>
        /// 条码 / 追溯码
        /// </summary>
        [Column("barcode")]
        [MaxLength(40)]
        public string Barcode { get; set; }

        /// <summary>
        /// 产品ID
        /// </summary>
        [Column("product_id")]
        [Required]
        public long ProductId { get; set; }

        /// <summary>
        /// 工艺ID
        /// </summary>
        [Column("technology_id")]
        [Required]
        public long TechnologyId { get; set; }

        [Column("product_name")]
        [Required]
        public string ProductName { get; set; }

        [Column("technology_name")]
        [Required]
        public string TechnologyName { get; set; }

        [Column("param_name")]
        [Required]
        public string ParamName { get; set; }

        [Column("param_value")]
        [Required]
        public string ParamValue { get; set; }

        /// <summary>
        /// 目标值
        /// </summary>
        [Column("target_value")]
        [Required]
        public double TargetValue { get; set; }

        /// <summary>
        /// 上公差
        /// </summary>
        [Column("upper_tolerance")]
        [Required]
        public double UpperTolerance { get; set; }

        /// <summary>
        /// 下公差
        /// </summary>
        [Column("lower_tolerance")]
        [Required]
        public double LowerTolerance { get; set; }

        /// <summary>
        /// 测量类型
        /// 1-直径 2-圆度 3-圆柱度 4-跳动
        /// </summary>
        [Column("measure_type")]
        [Required]
        public int MeasureType { get; set; }

        /// <summary>
        /// 实际测量值
        /// </summary>
        [Column("measure_value")]
        [Required]
        public double MeasureValue { get; set; }

        /// <summary>
        /// 分组UUID（一次测量）
        /// </summary>
        [Column("group_uuid")]
        [Required]
        [MaxLength(32)]
        public string GroupUuid { get; set; }


        /// <summary>
        /// 状态 0:不合格 1:合格
        /// </summary>
        [Column("status")]
        [Required]
        public bool Status { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [Column("is_delete")]
        [Required]
        public bool IsDelete { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("create_time")]
        [Required]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("update_time")]
        [Required]
        public DateTime UpdateTime { get; set; }
    }
}

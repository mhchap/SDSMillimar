using SDSMillimar.Utils.ValidationAttributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDSMillimar.Models
{
    [Table("technology_param")]
    public class TechnologyParam
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("technology_id")]
        [ForeignKey("Technology")]
        [Display(Name = "工艺")]
        [NotZero(ErrorMessage = "请选择{0}")]
        public long TechnologyId { get; set; }


        [Column("technology_name")]
        public string TechnologyName { get; set; }

        [Display(Name = "参数编号")]
        [Required(ErrorMessage = "{0}不能为空")]
        [MaxLength(100, ErrorMessage = "{0}长度不能超过100")]
        [Column("param_name")]
        public string ParamName { get; set; }


        [Display(Name = "参数名称")]
        [Required(ErrorMessage = "{0}不能为空")]
        [MaxLength(100, ErrorMessage = "{0}长度不能超过100")]
        [Column("param_value")]
        public string ParamValue { get; set; }

        [Display(Name = "过滤值")]
        [Required(ErrorMessage = "{0}不能为空")]
        [Column("filter_value")]
        public double FilterValue { get; set; }


        [Display(Name = "补偿值")]
        [Required(ErrorMessage = "{0}不能为空")]
        [Column("compensation_value")]
        public double CompensationValue { get; set; }

        [Display(Name = "目标值")]
        [Required(ErrorMessage = "{0}不能为空")]
        [Column("target_value")]
        public double TargetValue { get; set; }

        [Display(Name = "上公差值")]
        [Required(ErrorMessage = "{0}不能为空")]
        [Column("upper_tolerance")]
        public double UpperTolerance { get; set; }

        [Display(Name = "下公差值")]
        [Required(ErrorMessage = "{0}不能为空")]
        [Column("lower_tolerance")]
        public double LowerTolerance { get; set; }



        [Display(Name = "测量类型")]
        [Required(ErrorMessage = "{0}不能为空")]
        [Column("measure_type")]
        public int MeasureType { get; set; }

        [Display(Name = "排序")]
        [Column("sort")]
        public int Sort { get; set; }

        [Display(Name = "探针组")]
        [MaxLength(30, ErrorMessage = "{0}长度不能超过30")]
        [Column("device_ids")]
        public string DeviceIds { get; set; }

        [MaxLength(100, ErrorMessage = "{0}长度不能超过100")]
        [Column("remark")]
        public string Remark { get; set; }

        [Required]
        [Column("is_delete")]
        public bool IsDelete { get; set; } = false;
        [Required]
        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Required]
        [Column("update_time")]
        public DateTime UpdateTime { get; set; } = DateTime.Now;

        public virtual Technology Technology { get; set; }
    }
}

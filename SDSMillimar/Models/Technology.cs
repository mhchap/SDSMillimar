using SDSMillimar.Utils.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDSMillimar.Models
{
    [Table("technology")]
    public class Technology
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("product_id")]
        [ForeignKey("Product")]
        [Display(Name = "零件")]
        [NotZero(ErrorMessage = "请选择{0}")]
        public long ProductId { get; set; }

        [Column("product_name")]
        public string ProductName { get; set; }

        [Display(Name = "工艺编号")]
        [Required(ErrorMessage = "{0}不能为空")]
        [MaxLength(36, ErrorMessage = "{0}长度不能超过32")]
        [Column("technology_code")]
        public string TechnologyCode { get; set; }

        [Display(Name = "工艺名称")]
        [Required(ErrorMessage = "{0}不能为空")]
        [MaxLength(50, ErrorMessage = "{0}长度不能超过50")]
        [Column("technology_name")]
        public string TechnologyName { get; set; }

        [Display(Name = "工艺类型")]
        [Required(ErrorMessage = "{0}不能为空")]
        [NotZero(ErrorMessage = "请选择{0}")]
        [Column("technology_type")]
        public int TechnologyType { get; set; }


        [Display(Name = "油槽过滤")]
        [Required(ErrorMessage = "{0}不能为空")]
        [NotZero(ErrorMessage = "请选择{0}")]
        [Column("is_oilgroove")]
        public int IsOilGroove { get; set; }

        [Display(Name = "备注")]
        [StringLength(50, ErrorMessage = "{0}长度不能超过50")]
        [Column("remark")]
        public string Remark { get; set; }


        [Required]
        [Column("is_addParams")]
        public bool IsAddParams { get; set; } = false;

        [Required]
        [Column("is_delete")]
        public bool IsDelete { get; set; } = false;
        [Required]
        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Required]
        [Column("update_time")]
        public DateTime UpdateTime { get; set; } = DateTime.Now;

        public virtual Product Product { get; set; }
        public virtual ICollection<TechnologyParam> Params { get; set; }
    }
}

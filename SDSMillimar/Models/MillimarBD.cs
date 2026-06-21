using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDSMillimar.Models
{
    [Table("millimar_bd")]
    public class MillimarBD
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Display(Name = "唯一值")]
        [Required(ErrorMessage = "{0}不能为空")]
        [Column("key")]
        public double Key { get; set; }

        [Display(Name = "标定值")]
        [Required(ErrorMessage = "{0}不能为空")]
        [Column("value")]
        public double Value { get; set; }

        [Required]
        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

    }
}

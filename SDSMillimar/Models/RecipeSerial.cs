using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDSMillimar.Models
{
    [Table("recipe_serial")]
    public class RecipeSerial
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 配方号
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("recipe_no")]
        public string RecipeNo { get; set; } = string.Empty;

        /// <summary>
        /// 日期
        /// </summary>
        [Column("serial_date")]
        public DateTime SerialDate { get; set; }

        /// <summary>
        /// 当前流水号
        /// </summary>
        [Column("current_value")]
        public int CurrentValue { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDSMillimar.Models
{
    [Table("product")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(32)]
        [Index("uq_product_id", IsUnique = true)]
        [Column("product_id")]
        public string ProductId { get; set; }

        [Required]
        [StringLength(50)]
        [Index("idx_product_name")]
        [Column("product_name")]
        public string ProductName { get; set; }

        [StringLength(50)]
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
    }

}

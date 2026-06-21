using SDSMillimar.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace SDSMillimar.Utils
{
    public class AppDbContext : DbContext
    {
        // 构造函数使用 MySQL 连接字符串
        public AppDbContext() : base("name=MySqlConnection")
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Technology> Technologys { get; set; }
        public DbSet<TechnologyParam> TechnologyParams { get; set; }
        public DbSet<ProcessData> ProcessDatas { get; set; }
        public DbSet<MillimarBD> MillimarBDs { get; set; }
        public DbSet<RecipeSerial> RecipeSerials { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 设置表和字段映射，可选，EF6 默认用属性
            modelBuilder.Entity<Product>()
                .Property(p => p.CreateTime)
                .HasColumnType("timestamp")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Product>()
                .Property(p => p.UpdateTime)
                .HasColumnType("timestamp")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
        }
    }
}

using SDSMillimar.Dtos;
using SDSMillimar.Models;
using SDSMillimar.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace SDSMillimar.Services
{
    internal class MillimarBDRepository
    {
        #region 新增
        public async Task<int> AddAsync(List<MillimarBD> entities)
        {
            using (var db = new AppDbContext())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    // 1️⃣ 先删除所有旧数据
                    await db.Database.ExecuteSqlCommandAsync(
                        "DELETE FROM millimar_bd"
                    );

                    // 2️⃣ 再新增
                    db.MillimarBDs.AddRange(entities);

                    int result = await db.SaveChangesAsync();
                    tran.Commit();
                    return result;
                }
                catch (Exception)
                {
                    tran.Rollback();
                    return -1;
                }
            }
        }

        #endregion



        #region 分页查询（核心）
        public async Task<List<MillimarBD>> GetMillimarBDs()
        {
            using (var db = new AppDbContext())
            {
                return await db.MillimarBDs.OrderBy(x => x.Key).ToListAsync();
            }
        }
        #endregion


    }
}

using SDSMillimar.Common;
using SDSMillimar.Models;
using SDSMillimar.Utils;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace SDSMillimar.Services
{
    internal class TechnologyRepository
    {
        #region 新增
        public async Task<int> AddAsync(Technology entity)
        {
            using (var db = new AppDbContext())
            {
                try
                {
                    var query = db.Technologys
                             .Where(x => !x.IsDelete && x.TechnologyType == entity.TechnologyType && x.ProductId == entity.ProductId);
                    var totalCount = await query.CountAsync();
                    //var p = await GetByTechnologyIdAsync(entity.TechnologyCode);
                    if (totalCount > 0)
                        throw new Exception($"产品【{entity.ProductName}】已存在【{(entity.TechnologyType == 1 ? "测量" : "校准")}】的工艺,请勿重复添加!");
                    db.Technologys.Add(entity);
                    return await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {

                    throw new Exception(ex.Message);
                }
            }
        }
        #endregion

        #region 修改
        public async Task<int> UpdateAsync(Technology entity)
        {
            using (var db = new AppDbContext())
            {
                db.Entry(entity).State = EntityState.Modified;
                return await db.SaveChangesAsync();
            }
        }
        #endregion

        #region 软删除
        public async Task<int> SoftDeleteAsync(long id)
        {
            using (var db = new AppDbContext())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    // ===== 1. 主表 =====
                    var technology = await db.Technologys
                                             .FirstOrDefaultAsync(x => x.Id == id);

                    if (technology == null)
                        return -1;

                    technology.IsDelete = true;
                    technology.UpdateTime = DateTime.Now;

                    // ===== 2. 子表（批量软删除）=====
                    var paramList = await db.TechnologyParams
                                            .Where(p => p.TechnologyId == id && !p.IsDelete)
                                            .ToListAsync();

                    foreach (var param in paramList)
                    {
                        param.IsDelete = true;
                        param.UpdateTime = DateTime.Now;
                    }

                    int result = await db.SaveChangesAsync();
                    tran.Commit();

                    return result;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    AppLog.Production.Error($"SoftDeleteAsync-> {ex.Message}");
                    return -1;
                }
            }
        }

        #endregion

        #region 根据 ID 查询
        public async Task<Technology> GetByIdAsync(long id)
        {
            using (var db = new AppDbContext())
            {
                return await db.Technologys
                               .AsNoTracking()
                               .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            }
        }
        #endregion

        #region 根据 TechnologyId 查询
        public async Task<Technology> GetByTechnologyIdAsync(string technologyId)
        {
            using (var db = new AppDbContext())
            {
                return await db.Technologys
                               .AsNoTracking()
                               .FirstOrDefaultAsync(x => x.TechnologyCode == technologyId && !x.IsDelete);
            }
        }
        #endregion

        #region 分页查询（核心）
        public async Task<PageResult<Technology>> GetPageAsync(
            int pageIndex,
            int pageSize,
            string keyword = null)
        {
            using (var db = new AppDbContext())
            {

                var query = db.Technologys
                              .Where(x => !x.IsDelete);

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(x =>
                        x.TechnologyName.Contains(keyword) ||
                        x.TechnologyCode.Contains(keyword));
                }

                query = query.OrderByDescending(x => x.CreateTime);

                var totalCount = await query.CountAsync();
                var items = await query.Skip((pageIndex - 1) * pageSize)
                                       .Take(pageSize)
                                       .AsNoTracking()
                                       .ToListAsync();

                return new PageResult<Technology>
                {
                    TotalCount = totalCount,
                    Items = items
                };
            }
        }
        #endregion
    }
}

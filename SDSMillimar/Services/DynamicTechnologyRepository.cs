using SDSMillimar.Common;
using SDSMillimar.Dtos;
using SDSMillimar.Models;
using SDSMillimar.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace SDSMillimar.Services
{
    internal class DynamicTechnologyRepository
    {
        #region 新增
        public async Task<int> AddAsync(TechnologyParam entity)
        {
            using (var db = new AppDbContext())
            {
                try
                {
                    //var p = await GetByTechnologyIdAsync(entity.TechnologyCode);
                    //if (p != null)
                    //    throw new Exception($"参数编号【{entity.TechnologyCode}】已存在");
                    db.TechnologyParams.Add(entity);
                    return await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {

                    throw new Exception(ex.Message);
                }
            }
        }

        //public async Task<int> AddOrUpdateRangeAsync(IEnumerable<TechnologyParam> entities)
        //{
        //    using (var db = new AppDbContext())
        //    {
        //        try
        //        {
        //            foreach (var entity in entities)
        //            {
        //                if (entity.Id > 0)
        //                {
        //                    // 已存在的，先附加到上下文并标记为修改
        //                    db.TechnologyParams.Attach(entity);
        //                    db.Entry(entity).State = EntityState.Modified;
        //                }
        //                else
        //                {
        //                    // 新增
        //                    db.TechnologyParams.Add(entity);
        //                }
        //            }

        //            return await db.SaveChangesAsync();
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception(ex.Message, ex);
        //        }
        //    }
        //}

        public async Task<int> AddOrUpdateRangeAsync(
       IEnumerable<TechnologyParam> entities,
       long technologyId)
        {
            using (var db = new AppDbContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var entity in entities)
                    {
                        if (entity.Id > 0)
                        {
                            // EF6 推荐：先查，再赋值（避免全字段覆盖）
                            var existing = await db.TechnologyParams
                                                   .FirstOrDefaultAsync(p => p.Id == entity.Id);

                            if (existing != null)
                            {
                                db.Entry(existing).CurrentValues.SetValues(entity);
                            }
                        }
                        else
                        {
                            db.TechnologyParams.Add(entity);
                        }
                    }

                    // ===== 同步更新另一张表 =====
                    var tech = await db.Technologys
                                       .FirstOrDefaultAsync(t => t.Id == technologyId);

                    if (tech != null)
                    {
                        tech.IsAddParams = true;
                        tech.UpdateTime = DateTime.Now;
                    }

                    int result = await db.SaveChangesAsync();
                    transaction.Commit();

                    return result;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    AppLog.Production.Error($"AddOrUpdateRangeAsync-> {ex.Message}");
                    return -1;
                }
            }
        }




        #endregion

        #region 修改
        public async Task<int> UpdateAsync(TechnologyParam entity)
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
            {
                var entity = await db.TechnologyParams.FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) return -1;

                entity.IsDelete = true;
                return await db.SaveChangesAsync();
            }
        }
        #endregion

        #region 根据 ID 查询
        public async Task<TechnologyParam> GetByIdAsync(long id)
        {
            using (var db = new AppDbContext())
            {
                return await db.TechnologyParams
                               .AsNoTracking()
                               .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            }
        }
        #endregion

        #region 根据 TechnologyId 查询
        public async Task<List<TechnologyParam>> GetByTechnologyIdAsync(long technologyId)
        {
            using (var db = new AppDbContext())
            {
                return await db.TechnologyParams
                               .AsNoTracking()
                               .Where(x => x.TechnologyId == technologyId)
                               .ToListAsync();
            }
        }

        #endregion

        #region 分页查询（核心）
        public async Task<PageResult<TechnologyParam>> GetPageAsync(
            int pageIndex,
            int pageSize,
            string keyword = null)
        {
            using (var db = new AppDbContext())
            {
                try
                {
                    var query = db.TechnologyParams
                          .Where(x => !x.IsDelete);

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        query = query.Where(x =>
                            x.ParamName.Contains(keyword) ||
                            x.ParamValue.Contains(keyword));
                    }

                    query = query.OrderByDescending(x => x.CreateTime);

                    var totalCount = await query.CountAsync();
                    var items = await query.Skip((pageIndex - 1) * pageSize)
                                           .Take(pageSize)
                                           .AsNoTracking()
                                           .ToListAsync();

                    return new PageResult<TechnologyParam>
                    {
                        TotalCount = totalCount,
                        Items = items
                    };
                }
                catch (Exception ex)
                {

                    throw;
                }


            }
        }
        #endregion


    }
}

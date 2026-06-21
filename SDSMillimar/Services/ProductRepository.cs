using SDSMillimar.Dtos;
using SDSMillimar.Models;
using SDSMillimar.Utils;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace SDSMillimar.Services
{
    internal class ProductRepository
    {
        #region 新增
        public async Task<int> AddAsync(Product entity)
        {
            using (var db = new AppDbContext())
            {
                try
                {
                    var p = await GetByProductIdAsync(entity.ProductId);
                    if (p != null)
                        throw new Exception($"零件编号【{entity.ProductId}】已存在");
                    db.Products.Add(entity);
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
        public async Task<int> UpdateAsync(Product entity)
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
                var entity = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) return -1;

                entity.IsDelete = true;
                return await db.SaveChangesAsync();
            }
        }
        #endregion

        #region 根据 ID 查询
        public async Task<Product> GetByIdAsync(long id)
        {
            using (var db = new AppDbContext())
            {
                return await db.Products
                               .AsNoTracking()
                               .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            }
        }
        #endregion

        #region 根据 ProductId 查询
        public async Task<Product> GetByProductIdAsync(string productId)
        {
            using (var db = new AppDbContext())
            {
                return await db.Products
                               .AsNoTracking()
                               .FirstOrDefaultAsync(x => x.ProductId == productId && !x.IsDelete);
            }
        }
        #endregion

        #region 分页查询（核心）
        public async Task<PageResult<Product>> GetPageAsync(
            int pageIndex,
            int pageSize,
            string keyword = null)
        {
            using (var db = new AppDbContext())
            {
                var query = db.Products
                              .Where(x => !x.IsDelete);

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(x =>
                        x.ProductName.Contains(keyword) ||
                        x.ProductId.Contains(keyword));
                }

                query = query.OrderByDescending(x => x.CreateTime);

                var totalCount = await query.CountAsync();
                var items = await query.Skip((pageIndex - 1) * pageSize)
                                       .Take(pageSize)
                                       .AsNoTracking()
                                       .ToListAsync();

                return new PageResult<Product>
                {
                    TotalCount = totalCount,
                    Items = items
                };
            }
        }
        #endregion
        #region 根据 ProductId 查询工艺及工艺参数
        public async Task<ProductDetailDto> GetTechnologiesByProductIdAsync(string productId)
        {
            using (var db = new AppDbContext())
            {
                try
                {
                    return await db.Products
               .Where(p => p.ProductId == productId && !p.IsDelete)
               .Select(p => new ProductDetailDto
               {
                   ProductId = p.ProductId,
                   ProductName = p.ProductName,

                   Technologies = db.Technologys
                       .Where(t => t.ProductId == p.Id && !t.IsDelete)
                       .Select(t => new TechnologyDto
                       {
                           TechnologyCode = t.TechnologyCode,
                           TechnologyName = t.TechnologyName,
                           TechnologyType = t.TechnologyType,
                           TechnologyID = t.Id,
                           IsOilGroove = t.IsOilGroove,
                           Params = db.TechnologyParams
                               .Where(tp => tp.TechnologyId == t.Id && !tp.IsDelete)
                               .Select(tp => new TechnologyParamDto
                               {
                                   ParamName = tp.ParamName,
                                   ParamValue = tp.ParamValue,
                                   TargetValue = tp.TargetValue,
                                   UpperTolerance = tp.UpperTolerance,
                                   LowerTolerance = tp.LowerTolerance,
                                   FilterValue = tp.FilterValue,
                                   CompensationValue = tp.CompensationValue,
                                   DeviceIds = tp.DeviceIds,
                                   Sort = tp.Sort,
                                   MeasureType = tp.MeasureType
                               }).OrderBy(x => x.Sort).ToList()
                       }).ToList()
               })
               .AsNoTracking()
               .FirstOrDefaultAsync();
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

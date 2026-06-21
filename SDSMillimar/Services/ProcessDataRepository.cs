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
    internal class ProcessDataRepository
    {
        private RecipeSerialRepository recipeSerialRepository = new RecipeSerialRepository();
        #region 批量新增（外部传 groupUuid，推荐）

        public async Task<int> AddBatchAsync(
            IEnumerable<ProcessData> entities,
            string groupUuid)
        {
            if (entities == null || !entities.Any())
                return 0;

            using (var db = new AppDbContext())
            {
                var now = DateTime.Now;

                foreach (var item in entities)
                {
                    item.GroupUuid = groupUuid;
                    item.CreateTime = now;
                    item.UpdateTime = now;
                    item.IsDelete = false;
                }

                db.ProcessDatas.AddRange(entities);
                return await db.SaveChangesAsync();
            }
        }

        #endregion

        #region 批量新增（内部生成 groupUuid）

        public async Task<int> AddBatchAsync(IEnumerable<ProcessData> entities)
        {
            if (entities == null || !entities.Any())
                return -1;

            using (var db = new AppDbContext())
            {
                var now = DateTime.Now;
                var groupUuid = Guid.NewGuid().ToString("N");
                var serial = await recipeSerialRepository.GetNextSerial("TULIP10");
                foreach (var item in entities)
                {
                    item.GroupUuid = $"{DateTime.Now:yyyyMMdd}{serial:D4}";
                    ;
                    item.CreateTime = now;
                    item.UpdateTime = now;
                    item.IsDelete = false;
                }

                db.ProcessDatas.AddRange(entities);
                return await db.SaveChangesAsync();

            }
        }

        #endregion

        #region 按 group_uuid 查询（一整次测量）

        public async Task<List<ProcessData>> GetByGroupUuidAsync(string groupUuid)
        {
            using (var db = new AppDbContext())
            {
                return await db.ProcessDatas
                    .Where(x => x.GroupUuid == groupUuid && !x.IsDelete)
                    .OrderBy(x => x.Id)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        #endregion

        #region 按时间 + 条码 + 产品 + 工艺 查询（核心）

        public async Task<PageResult<ProcessDataListDto>> GetPageAsync(
      int pageIndex,
      int pageSize,
      DateTime? startTime = null,
      DateTime? endTime = null,
      string barcode = null,
      long? productId = null,
      long? technologyId = null)
        {
            using (var db = new AppDbContext())
            {
                // 关联查询 Product 和 Technology
                var query = from pd in db.ProcessDatas
                            join p in db.Products on pd.ProductId equals p.Id
                            join t in db.Technologys on pd.TechnologyId equals t.Id
                            // where !pd.IsDelete && !p.IsDelete && !t.IsDelete
                            select new ProcessDataListDto
                            {
                                Id = pd.Id,
                                Barcode = pd.Barcode,
                                ProductId = pd.ProductId,
                                ProductName = p.ProductName,
                                TechnologyId = pd.TechnologyId,
                                TechnologyName = t.TechnologyName,
                                ParamValue = pd.ParamValue,
                                TargetValue = pd.TargetValue,
                                UpperTolerance = pd.UpperTolerance,
                                LowerTolerance = pd.LowerTolerance,
                                MeasureType = pd.MeasureType,
                                MeasureValue = pd.MeasureValue,
                                GroupUuid = pd.GroupUuid,
                                Status = pd.Status,
                                IsDelete = pd.IsDelete,
                                CreateTime = pd.CreateTime,
                                UpdateTime = pd.UpdateTime
                            };

                if (startTime.HasValue)
                    query = query.Where(x => x.CreateTime >= startTime.Value);

                if (endTime.HasValue)
                    query = query.Where(x => x.CreateTime <= endTime.Value);

                if (!string.IsNullOrWhiteSpace(barcode))
                    query = query.Where(x => x.Barcode.Contains(barcode));

                if (productId.HasValue && productId != 0)
                    query = query.Where(x => x.ProductId == productId.Value);

                if (technologyId.HasValue && technologyId != 0)
                    query = query.Where(x => x.TechnologyId == technologyId.Value);

                query = query.OrderByDescending(x => x.CreateTime);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                return new PageResult<ProcessDataListDto>
                {
                    TotalCount = totalCount,
                    Items = items
                };
            }
        }


        public async Task<PageResult<ProcessDataListDto>> GetPageAsyncGroupUuid(
    int pageIndex,
    int pageSize,
    DateTime? startTime = null,
    DateTime? endTime = null,
    string barcode = null,
    long? productId = null,
    long? technologyId = null, bool? status = null)
        {
            using (var db = new AppDbContext())
            {
                var baseQuery = from pd in db.ProcessDatas
                                join p in db.Products on pd.ProductId equals p.Id
                                join t in db.Technologys on pd.TechnologyId equals t.Id
                                select new
                                {
                                    pd,
                                    p.ProductName,
                                    t.TechnologyName
                                };

                if (startTime.HasValue)
                    baseQuery = baseQuery.Where(x => x.pd.CreateTime >= startTime.Value);

                if (endTime.HasValue)
                    baseQuery = baseQuery.Where(x => x.pd.CreateTime <= endTime.Value);

                if (!string.IsNullOrWhiteSpace(barcode))
                    baseQuery = baseQuery.Where(x => x.pd.Barcode.Contains(barcode));

                if (productId.HasValue && productId != 0)
                    baseQuery = baseQuery.Where(x => x.pd.ProductId == productId.Value);

                if (technologyId.HasValue && technologyId != 0)
                    baseQuery = baseQuery.Where(x => x.pd.TechnologyId == technologyId.Value);



                var groupQuery = baseQuery
                                   .GroupBy(x => x.pd.GroupUuid)
                                   .Select(g => new
                                   {
                                       GroupUuid = g.Key,
                                       CreateTime = g.Max(x => x.pd.CreateTime),

                                       // 组状态
                                       GroupStatus = g.All(x => x.pd.Status)
                                   });

                if (status.HasValue)
                {
                    if (status.Value)
                    {
                        // 查询全部OK
                        groupQuery = groupQuery.Where(x => x.GroupStatus);
                    }
                    else
                    {
                        // 查询有NG
                        groupQuery = groupQuery.Where(x => !x.GroupStatus);
                    }
                }

                groupQuery = groupQuery
                    .OrderByDescending(x => x.CreateTime);

                var totalCount = await groupQuery.CountAsync();

                var groupPage = await groupQuery
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var groupIds = groupPage.Select(x => x.GroupUuid).ToList();

                // ② 查询这些组的所有数据
                var data = await baseQuery
                    .Where(x => groupIds.Contains(x.pd.GroupUuid))
                    .ToListAsync();

                // ③ 内存分组
                var result = data
                                .GroupBy(x => x.pd.GroupUuid)
                                .Select(g =>
                                {
                                    var first = g.First();

                                    var dto = new ProcessDataListDto
                                    {
                                        GroupUuid = g.Key,
                                        Barcode = first.pd.Barcode,
                                        ProductId = first.pd.ProductId,
                                        ProductName = first.ProductName,
                                        TechnologyId = first.pd.TechnologyId,
                                        TechnologyName = first.TechnologyName,
                                        CreateTime = first.pd.CreateTime,
                                        Status = g.All(x => x.pd.Status)
                                    };

                                    foreach (var item in g)
                                    {
                                        switch (item.pd.ParamName)
                                        {
                                            case "M1":
                                                dto.M1 = item.pd.MeasureValue;
                                                dto.M1Status = item.pd.Status;
                                                break;

                                            case "M2":
                                                dto.M2 = item.pd.MeasureValue;
                                                dto.M2Status = item.pd.Status;
                                                break;

                                            case "M3":
                                                dto.M3 = item.pd.MeasureValue;
                                                dto.M3Status = item.pd.Status;
                                                break;

                                            case "M4":
                                                dto.M4 = item.pd.MeasureValue;
                                                dto.M4Status = item.pd.Status;
                                                break;

                                            case "M5":
                                                dto.M5 = item.pd.MeasureValue;
                                                dto.M5Status = item.pd.Status;
                                                break;

                                            case "M6":
                                                dto.M6 = item.pd.MeasureValue;
                                                dto.M6Status = item.pd.Status;
                                                break;

                                            case "M7":
                                                dto.M7 = item.pd.MeasureValue;
                                                dto.M7Status = item.pd.Status;
                                                break;

                                            case "M8":
                                                dto.M8 = item.pd.MeasureValue;
                                                dto.M8Status = item.pd.Status;
                                                break;

                                            case "M9":
                                                dto.M9 = item.pd.MeasureValue;
                                                dto.M9Status = item.pd.Status;
                                                break;

                                            case "M10":
                                                dto.M10 = item.pd.MeasureValue;
                                                dto.M10Status = item.pd.Status;
                                                break;

                                            case "M11":
                                                dto.M11 = item.pd.MeasureValue;
                                                dto.M11Status = item.pd.Status;
                                                break;

                                            case "M12":
                                                dto.M12 = item.pd.MeasureValue;
                                                dto.M12Status = item.pd.Status;
                                                break;

                                            case "M13":
                                                dto.M13 = item.pd.MeasureValue;
                                                dto.M13Status = item.pd.Status;
                                                break;

                                            case "M14":
                                                dto.M14 = item.pd.MeasureValue;
                                                dto.M14Status = item.pd.Status;
                                                break;

                                            case "M15":
                                                dto.M15 = item.pd.MeasureValue;
                                                dto.M15Status = item.pd.Status;
                                                break;
                                        }
                                    }

                                    return dto;
                                })
                                .OrderByDescending(x => x.CreateTime)
                                .ToList();

                return new PageResult<ProcessDataListDto>
                {
                    TotalCount = totalCount,
                    Items = result
                };
            }
        }


        #endregion

        #region 按时间段查询 group_uuid（用于统计 / 报表）

        public async Task<List<string>> GetGroupUuidsByTimeAsync(
            DateTime startTime,
            DateTime endTime)
        {
            using (var db = new AppDbContext())
            {
                return await db.ProcessDatas
                    .Where(x => !x.IsDelete &&
                                x.CreateTime >= startTime &&
                                x.CreateTime <= endTime)
                    .Select(x => x.GroupUuid)
                    .Distinct()
                    .ToListAsync();
            }
        }

        #endregion

        /// <summary>
        /// 根据 technologyId、paramName 和时间段查询数据，并按子组大小生成 SubgroupDto
        /// </summary>
        /// <param name="technologyId"></param>
        /// <param name="paramName"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="subgroupSize">子组大小</param>
        /// <returns></returns>
        public async Task<List<SubgroupDto>> GetSubgroupsAsync(
            long technologyId,
            string paramName,
            DateTime startTime,
            DateTime endTime,
            int subgroupSize, int limit)
        {
            using (var db = new AppDbContext())
            {
                // 1️⃣ 查询原始数据
                var data = await db.ProcessDatas
                    .AsNoTracking()
                    .Where(x => x.TechnologyId == technologyId &&
                                x.ParamName == paramName &&
                                !x.IsDelete &&
                                x.CreateTime >= startTime &&
                                x.CreateTime <= endTime)
                    .OrderBy(x => x.CreateTime)
                    .Take(limit)
                    .Select(x => new
                    {
                        Value = x.MeasureValue
                    })
                    .ToListAsync();

                // 2️⃣ 转成 double
                var allValues = data
                    .Select(v => v.Value)
                    .ToList();

                if (!allValues.Any())
                    return new List<SubgroupDto>();

                // 3️⃣ 按子组大小分组
                int totalPoints = allValues.Count;
                int numSubgroups = totalPoints / subgroupSize;
                var subgroups = new List<SubgroupDto>();

                for (int i = 0; i < numSubgroups; i++)
                {
                    var subgroupValues = allValues
                        .Skip(i * subgroupSize)
                        .Take(subgroupSize)
                        .ToList();

                    subgroups.Add(new SubgroupDto
                    {
                        SubgroupName = $"组{i + 1}",
                        Samples = subgroupValues
                    });
                }

                // 可选：处理剩余不足一个子组的数据
                int remainder = totalPoints % subgroupSize;
                if (remainder > 0)
                {
                    var lastValues = allValues.Skip(numSubgroups * subgroupSize).Take(remainder).ToList();
                    subgroups.Add(new SubgroupDto
                    {
                        SubgroupName = $"组{numSubgroups + 1}",
                        Samples = lastValues
                    });
                }

                return subgroups;
            }
        }
    }
}

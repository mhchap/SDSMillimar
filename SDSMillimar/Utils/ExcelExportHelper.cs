using ClosedXML.Excel;
using Microsoft.Win32;
using SDSMillimar.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SDSMillimar.Utils
{
    public static class ExcelExportHelper
    {
        public static async Task<bool> ExportAsync<T>(
            IEnumerable<T> data,
            string sheetName,
            Dictionary<string, string> columnMap,
            string fileName = "",
            string filter = "")
        {
            if (data == null || !data.Any())
                return false;

            if (columnMap == null || columnMap.Count == 0)
                throw new Exception("columnMap 不能为空");

            var saveFileDialog = new SaveFileDialog
            {
                Filter = string.IsNullOrEmpty(filter)
                    ? "Excel 文件 (*.xlsx)|*.xlsx"
                    : filter,
                FileName = string.IsNullOrEmpty(fileName)
                    ? "Export.xlsx"
                    : fileName
            };

            if (saveFileDialog.ShowDialog() != true)
                return false; // 用户取消

            string filePath = saveFileDialog.FileName;

            try
            {
                await Task.Run(() =>
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add(sheetName);

                        // ===== Property 缓存 =====
                        var propDict = typeof(T)
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.CanRead)
                            .ToDictionary(p => p.Name);

                        // ===== 表头 =====
                        int colIndex = 1;
                        foreach (var col in columnMap)
                        {
                            worksheet.Cell(1, colIndex).Value = col.Value;
                            colIndex++;
                        }

                        // ===== 数据 =====
                        int row = 2;
                        foreach (var item in data)
                        {
                            colIndex = 1;
                            foreach (var col in columnMap)
                            {
                                var cell = worksheet.Cell(row, colIndex);

                                if (propDict.TryGetValue(col.Key, out PropertyInfo prop))
                                {
                                    object value = prop.GetValue(item, null);

                                    // 检查是否有对应 Status 字段
                                    string statusPropName = col.Key + "Status"; // M15 -> M15Status
                                    if (propDict.TryGetValue(statusPropName, out PropertyInfo statusProp))
                                    {
                                        object statusValue = statusProp.GetValue(item, null);
                                        if (statusValue is bool isOk && !isOk)
                                        {
                                            // 对应状态为 false，不合格字体红色
                                            cell.Style.Font.FontColor = XLColor.Red;
                                            cell.Style.Font.Bold = true;
                                        }
                                    }

                                    // 日期处理
                                    if (value is DateTime dt)
                                    {
                                        cell.Value = dt;
                                        cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                                    }
                                    // bool 类型直接显示合格/不合格
                                    else if (value is bool b)
                                    {
                                        cell.Value = b ? "合格" : "不合格";
                                        if (!b)
                                        {
                                            cell.Style.Font.FontColor = XLColor.Red;
                                            cell.Style.Font.Bold = true;
                                        }
                                    }
                                    else
                                    {
                                        cell.Value = value?.ToString();
                                    }
                                }
                                else
                                {
                                    cell.Value = string.Empty;
                                }

                                colIndex++;
                            }
                            row++;
                        }

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(filePath);
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                AppLog.Production.Error($"导出异常${ex.Message}");
                return false;
            }
        }
    }
}
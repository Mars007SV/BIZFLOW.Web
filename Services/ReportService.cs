using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BIZFLOW.Web.Data;
using BIZFLOW.Web.Models;
using BIZFLOW.Web.Models.ViewModels;
using ClosedXML.Excel;

namespace BIZFLOW.Web.Services
{
    public interface IReportService
    {
        Task<ReportViewModel> GenerateReportDataAsync(string userName);
        byte[] GenerateExcelReport(ReportViewModel data);
        byte[] GenerateCsvReport(ReportViewModel data);
    }

    public class ReportService : IReportService
    {
        private readonly BizFlowDbContext _context;

        public ReportService(BizFlowDbContext context)
        {
            _context = context;
        }

        public async Task<ReportViewModel> GenerateReportDataAsync(string userName)
        {
            var report = new ReportViewModel
            {
                GeneratedAt = DateTime.Now,
                GeneratedBy = userName
            };

            // Отримуємо всі дані
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

            var operations = await _context.Operations
                .Include(o => o.Product)
                    .ThenInclude(p => p!.Category)
                .OrderByDescending(o => o.Date)
                .ToListAsync();

            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();

            // Загальна статистика
            report.TotalProducts = products.Count;
            report.TotalCategories = categories.Count;
            report.TotalOperations = operations.Count;

            // Статистика операцій
            var incomingOps = operations.Where(o => o.Type == "Incoming").ToList();
            var outgoingOps = operations.Where(o => o.Type == "Outgoing").ToList();

            report.TotalIncoming = incomingOps.Sum(o => o.Quantity);
            report.TotalOutgoing = outgoingOps.Sum(o => o.Quantity);
            report.NetBalance = report.TotalIncoming - report.TotalOutgoing;
            report.IncomingOperationsCount = incomingOps.Count;
            report.OutgoingOperationsCount = outgoingOps.Count;

            // Середні показники
            report.AverageIncomingQuantity = report.IncomingOperationsCount > 0 
                ? report.TotalIncoming / report.IncomingOperationsCount 
                : 0;

            report.AverageOutgoingQuantity = report.OutgoingOperationsCount > 0 
                ? report.TotalOutgoing / report.OutgoingOperationsCount 
                : 0;

            report.AverageStockPerProduct = report.TotalProducts > 0 
                ? products.Sum(p => p.Quantity) / report.TotalProducts 
                : 0;

            // Період звіту
            report.ReportStartDate = operations.Any() 
                ? operations.Min(o => o.Date) 
                : DateTime.Now;

            report.ReportEndDate = operations.Any() 
                ? operations.Max(o => o.Date) 
                : DateTime.Now;

            // Детальні дані по продуктах
            report.Products = products.Select(p => new ProductReportItem
            {
                ProductName = p.Name,
                CategoryName = p.Category?.Name ?? "Без категорії",
                CurrentQuantity = p.Quantity,
                UnitOfMeasure = GetUnitOfMeasureDisplay(p.UnitOfMeasure),
                OperationsCount = operations.Count(o => o.ProductId == p.Id),
                TotalIncoming = operations.Where(o => o.ProductId == p.Id && o.Type == "Incoming")
                    .Sum(o => o.Quantity),
                TotalOutgoing = operations.Where(o => o.ProductId == p.Id && o.Type == "Outgoing")
                    .Sum(o => o.Quantity),
                LastOperationDate = operations.Where(o => o.ProductId == p.Id)
                    .OrderByDescending(o => o.Date)
                    .Select(o => (DateTime?)o.Date)
                    .FirstOrDefault()
            }).ToList();

            // Детальні дані по категоріях
            report.Categories = categories.Select(c => new CategoryReportItem
            {
                CategoryName = c.Name,
                ProductsCount = c.Products?.Count ?? 0,
                TotalQuantity = c.Products?.Sum(p => p.Quantity) ?? 0,
                OperationsCount = operations.Count(o => o.Product != null && o.Product.CategoryId == c.Id),
                TotalIncoming = operations.Where(o => o.Product != null && o.Product.CategoryId == c.Id && o.Type == "Incoming")
                    .Sum(o => o.Quantity),
                TotalOutgoing = operations.Where(o => o.Product != null && o.Product.CategoryId == c.Id && o.Type == "Outgoing")
                    .Sum(o => o.Quantity)
            }).ToList();

            // Останні операції (топ-100)
            report.RecentOperations = operations.Take(100).Select(o => new OperationReportItem
            {
                Date = o.Date,
                ProductName = o.Product?.Name ?? "Невідомо",
                Type = o.Type == "Incoming" ? "Надходження" : "Списання",
                Quantity = o.Quantity,
                RemainingQuantity = o.RemainingQuantity,
                Description = o.Description ?? "",
                UserName = o.UserName ?? "Невідомо",
                UnitOfMeasure = GetUnitOfMeasureDisplay(o.Product?.UnitOfMeasure ?? UnitOfMeasure.Pieces)
            }).ToList();

            // Топ-10 продуктів за активністю
            report.TopProducts = products
                .Select(p => new TopProductItem
                {
                    ProductName = p.Name,
                    CategoryName = p.Category?.Name ?? "Без категорії",
                    OperationsCount = operations.Count(o => o.ProductId == p.Id),
                    TotalTurnover = operations.Where(o => o.ProductId == p.Id)
                        .Sum(o => o.Quantity),
                    CurrentQuantity = p.Quantity,
                    UnitOfMeasure = GetUnitOfMeasureDisplay(p.UnitOfMeasure)
                })
                .OrderByDescending(p => p.OperationsCount)
                .Take(10)
                .ToList();

            return report;
        }

        public byte[] GenerateExcelReport(ReportViewModel data)
        {
            using var workbook = new XLWorkbook();

            // Аркуш 1: Загальна інформація
            var summarySheet = workbook.Worksheets.Add("Загальна інформація");
            CreateSummarySheet(summarySheet, data);

            // Аркуш 2: Продукти
            var productsSheet = workbook.Worksheets.Add("Продукти");
            CreateProductsSheet(productsSheet, data);

            // Аркуш 3: Категорії
            var categoriesSheet = workbook.Worksheets.Add("Категорії");
            CreateCategoriesSheet(categoriesSheet, data);

            // Аркуш 4: Операції
            var operationsSheet = workbook.Worksheets.Add("Операції");
            CreateOperationsSheet(operationsSheet, data);

            // Аркуш 5: Топ продуктів
            var topProductsSheet = workbook.Worksheets.Add("Топ продуктів");
            CreateTopProductsSheet(topProductsSheet, data);

            using var stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerateCsvReport(ReportViewModel data)
        {
            var csv = new StringBuilder();

            // Заголовок звіту
            csv.AppendLine("ЗВІТ ПО ОПЕРАЦІЯХ ТА ПРОДУКТАХ");
            csv.AppendLine($"Згенеровано: {data.GeneratedAt:dd.MM.yyyy HH:mm}");
            csv.AppendLine($"Користувач: {data.GeneratedBy}");
            csv.AppendLine($"Період: {data.ReportStartDate:dd.MM.yyyy} - {data.ReportEndDate:dd.MM.yyyy}");
            csv.AppendLine();

            // Загальна статистика
            csv.AppendLine("=== ЗАГАЛЬНА СТАТИСТИКА ===");
            csv.AppendLine($"Всього продуктів,{data.TotalProducts}");
            csv.AppendLine($"Всього категорій,{data.TotalCategories}");
            csv.AppendLine($"Всього операцій,{data.TotalOperations}");
            csv.AppendLine($"Надходжень (к-ть),{data.IncomingOperationsCount}");
            csv.AppendLine($"Списань (к-ть),{data.OutgoingOperationsCount}");
            csv.AppendLine($"Загальні надходження,{data.TotalIncoming:F2}");
            csv.AppendLine($"Загальні списання,{data.TotalOutgoing:F2}");
            csv.AppendLine($"Чистий баланс,{data.NetBalance:F2}");
            csv.AppendLine($"Середнє надходження,{data.AverageIncomingQuantity:F2}");
            csv.AppendLine($"Середнє списання,{data.AverageOutgoingQuantity:F2}");
            csv.AppendLine($"Середній залишок на продукт,{data.AverageStockPerProduct:F2}");
            csv.AppendLine();

            // Продукти
            csv.AppendLine("=== ПРОДУКТИ ===");
            csv.AppendLine("Назва,Категорія,Поточна к-ть,Од. виміру,К-ть операцій,Надходження,Списання,Остання операція");
            foreach (var product in data.Products)
            {
                csv.AppendLine($"\"{product.ProductName}\",\"{product.CategoryName}\",{product.CurrentQuantity:F2}," +
                    $"\"{product.UnitOfMeasure}\",{product.OperationsCount},{product.TotalIncoming:F2}," +
                    $"{product.TotalOutgoing:F2},{product.LastOperationDate?.ToString("dd.MM.yyyy HH:mm") ?? "-"}");
            }
            csv.AppendLine();

            // Категорії
            csv.AppendLine("=== КАТЕГОРІЇ ===");
            csv.AppendLine("Назва,К-ть продуктів,Загальна к-ть,К-ть операцій,Надходження,Списання");
            foreach (var category in data.Categories)
            {
                csv.AppendLine($"\"{category.CategoryName}\",{category.ProductsCount},{category.TotalQuantity:F2}," +
                    $"{category.OperationsCount},{category.TotalIncoming:F2},{category.TotalOutgoing:F2}");
            }
            csv.AppendLine();

            // Топ продуктів
            csv.AppendLine("=== ТОП-10 ПРОДУКТІВ ЗА АКТИВНІСТЮ ===");
            csv.AppendLine("Позиція,Назва,Категорія,К-ть операцій,Загальний оборот,Поточна к-ть,Од. виміру");
            int position = 1;
            foreach (var product in data.TopProducts)
            {
                csv.AppendLine($"{position},\"{product.ProductName}\",\"{product.CategoryName}\"," +
                    $"{product.OperationsCount},{product.TotalTurnover:F2},{product.CurrentQuantity:F2}," +
                    $"\"{product.UnitOfMeasure}\"");
                position++;
            }
            csv.AppendLine();

            // Операції
            csv.AppendLine("=== ОСТАННІ ОПЕРАЦІЇ ===");
            csv.AppendLine("Дата,Продукт,Тип,Кількість,Залишок,Опис,Користувач,Од. виміру");
            foreach (var operation in data.RecentOperations)
            {
                csv.AppendLine($"{operation.Date:dd.MM.yyyy HH:mm},\"{operation.ProductName}\"," +
                    $"\"{operation.Type}\",{operation.Quantity:F2},{operation.RemainingQuantity:F2}," +
                    $"\"{operation.Description}\",\"{operation.UserName}\",\"{operation.UnitOfMeasure}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        private void CreateSummarySheet(IXLWorksheet sheet, ReportViewModel data)
        {
            int row = 1;

            // Заголовок
            sheet.Cell(row, 1).Value = "ЗВІТ ПО ОПЕРАЦІЯХ ТА ПРОДУКТАХ";
            sheet.Cell(row, 1).Style.Font.Bold = true;
            sheet.Cell(row, 1).Style.Font.FontSize = 16;
            row += 2;

            // Інформація про звіт
            sheet.Cell(row, 1).Value = "Згенеровано:";
            sheet.Cell(row, 2).Value = data.GeneratedAt.ToString("dd.MM.yyyy HH:mm");
            row++;

            sheet.Cell(row, 1).Value = "Користувач:";
            sheet.Cell(row, 2).Value = data.GeneratedBy;
            row++;

            sheet.Cell(row, 1).Value = "Період:";
            sheet.Cell(row, 2).Value = $"{data.ReportStartDate:dd.MM.yyyy} - {data.ReportEndDate:dd.MM.yyyy}";
            row += 2;

            // Загальна статистика
            sheet.Cell(row, 1).Value = "ЗАГАЛЬНА СТАТИСТИКА";
            sheet.Cell(row, 1).Style.Font.Bold = true;
            sheet.Cell(row, 1).Style.Font.FontSize = 14;
            sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            sheet.Range(row, 1, row, 2).Merge();
            row++;

            AddDataRow(sheet, row++, "Всього продуктів:", data.TotalProducts);
            AddDataRow(sheet, row++, "Всього категорій:", data.TotalCategories);
            AddDataRow(sheet, row++, "Всього операцій:", data.TotalOperations);
            row++;

            sheet.Cell(row, 1).Value = "СТАТИСТИКА ОПЕРАЦІЙ";
            sheet.Cell(row, 1).Style.Font.Bold = true;
            sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
            sheet.Range(row, 1, row, 2).Merge();
            row++;

            AddDataRow(sheet, row++, "Надходжень (к-ть):", data.IncomingOperationsCount);
            AddDataRow(sheet, row++, "Списань (к-ть):", data.OutgoingOperationsCount);
            AddDataRow(sheet, row++, "Загальні надходження:", $"{data.TotalIncoming:F2}");
            AddDataRow(sheet, row++, "Загальні списання:", $"{data.TotalOutgoing:F2}");
            AddDataRow(sheet, row++, "Чистий баланс:", $"{data.NetBalance:F2}");
            row++;

            sheet.Cell(row, 1).Value = "СЕРЕДНІ ПОКАЗНИКИ";
            sheet.Cell(row, 1).Style.Font.Bold = true;
            sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightYellow;
            sheet.Range(row, 1, row, 2).Merge();
            row++;

            AddDataRow(sheet, row++, "Середнє надходження:", $"{data.AverageIncomingQuantity:F2}");
            AddDataRow(sheet, row++, "Середнє списання:", $"{data.AverageOutgoingQuantity:F2}");
            AddDataRow(sheet, row++, "Середній залишок на продукт:", $"{data.AverageStockPerProduct:F2}");

            // Автоширина колонок
            sheet.Columns().AdjustToContents();
        }

        private void CreateProductsSheet(IXLWorksheet sheet, ReportViewModel data)
        {
            // Заголовок
            sheet.Cell(1, 1).Value = "ПРОДУКТИ";
            sheet.Cell(1, 1).Style.Font.Bold = true;
            sheet.Cell(1, 1).Style.Font.FontSize = 14;
            sheet.Range(1, 1, 1, 8).Merge();

            // Заголовки колонок
            int headerRow = 3;
            sheet.Cell(headerRow, 1).Value = "Назва";
            sheet.Cell(headerRow, 2).Value = "Категорія";
            sheet.Cell(headerRow, 3).Value = "Поточна к-ть";
            sheet.Cell(headerRow, 4).Value = "Од. виміру";
            sheet.Cell(headerRow, 5).Value = "К-ть операцій";
            sheet.Cell(headerRow, 6).Value = "Надходження";
            sheet.Cell(headerRow, 7).Value = "Списання";
            sheet.Cell(headerRow, 8).Value = "Остання операція";

            // Стиль заголовків
            var headerRange = sheet.Range(headerRow, 1, headerRow, 8);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Дані
            int row = headerRow + 1;
            foreach (var product in data.Products)
            {
                sheet.Cell(row, 1).Value = product.ProductName;
                sheet.Cell(row, 2).Value = product.CategoryName;
                sheet.Cell(row, 3).Value = product.CurrentQuantity;
                sheet.Cell(row, 4).Value = product.UnitOfMeasure;
                sheet.Cell(row, 5).Value = product.OperationsCount;
                sheet.Cell(row, 6).Value = product.TotalIncoming;
                sheet.Cell(row, 7).Value = product.TotalOutgoing;
                sheet.Cell(row, 8).Value = product.LastOperationDate?.ToString("dd.MM.yyyy HH:mm") ?? "-";
                row++;
            }

            // Автоширина
            sheet.Columns().AdjustToContents();
        }

        private void CreateCategoriesSheet(IXLWorksheet sheet, ReportViewModel data)
        {
            // Заголовок
            sheet.Cell(1, 1).Value = "КАТЕГОРІЇ";
            sheet.Cell(1, 1).Style.Font.Bold = true;
            sheet.Cell(1, 1).Style.Font.FontSize = 14;
            sheet.Range(1, 1, 1, 6).Merge();

            // Заголовки колонок
            int headerRow = 3;
            sheet.Cell(headerRow, 1).Value = "Назва";
            sheet.Cell(headerRow, 2).Value = "К-ть продуктів";
            sheet.Cell(headerRow, 3).Value = "Загальна к-ть";
            sheet.Cell(headerRow, 4).Value = "К-ть операцій";
            sheet.Cell(headerRow, 5).Value = "Надходження";
            sheet.Cell(headerRow, 6).Value = "Списання";

            // Стиль заголовків
            var headerRange = sheet.Range(headerRow, 1, headerRow, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Дані
            int row = headerRow + 1;
            foreach (var category in data.Categories)
            {
                sheet.Cell(row, 1).Value = category.CategoryName;
                sheet.Cell(row, 2).Value = category.ProductsCount;
                sheet.Cell(row, 3).Value = category.TotalQuantity;
                sheet.Cell(row, 4).Value = category.OperationsCount;
                sheet.Cell(row, 5).Value = category.TotalIncoming;
                sheet.Cell(row, 6).Value = category.TotalOutgoing;
                row++;
            }

            // Автоширина
            sheet.Columns().AdjustToContents();
        }

        private void CreateOperationsSheet(IXLWorksheet sheet, ReportViewModel data)
        {
            // Заголовок
            sheet.Cell(1, 1).Value = "ОСТАННІ ОПЕРАЦІЇ";
            sheet.Cell(1, 1).Style.Font.Bold = true;
            sheet.Cell(1, 1).Style.Font.FontSize = 14;
            sheet.Range(1, 1, 1, 8).Merge();

            // Заголовки колонок
            int headerRow = 3;
            sheet.Cell(headerRow, 1).Value = "Дата";
            sheet.Cell(headerRow, 2).Value = "Продукт";
            sheet.Cell(headerRow, 3).Value = "Тип";
            sheet.Cell(headerRow, 4).Value = "Кількість";
            sheet.Cell(headerRow, 5).Value = "Залишок";
            sheet.Cell(headerRow, 6).Value = "Опис";
            sheet.Cell(headerRow, 7).Value = "Користувач";
            sheet.Cell(headerRow, 8).Value = "Од. виміру";

            // Стиль заголовків
            var headerRange = sheet.Range(headerRow, 1, headerRow, 8);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightYellow;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Дані
            int row = headerRow + 1;
            foreach (var operation in data.RecentOperations)
            {
                sheet.Cell(row, 1).Value = operation.Date.ToString("dd.MM.yyyy HH:mm");
                sheet.Cell(row, 2).Value = operation.ProductName;
                sheet.Cell(row, 3).Value = operation.Type;
                sheet.Cell(row, 4).Value = operation.Quantity;
                sheet.Cell(row, 5).Value = operation.RemainingQuantity;
                sheet.Cell(row, 6).Value = operation.Description;
                sheet.Cell(row, 7).Value = operation.UserName;
                sheet.Cell(row, 8).Value = operation.UnitOfMeasure;
                row++;
            }

            // Автоширина
            sheet.Columns().AdjustToContents();
        }

        private void CreateTopProductsSheet(IXLWorksheet sheet, ReportViewModel data)
        {
            // Заголовок
            sheet.Cell(1, 1).Value = "ТОП-10 ПРОДУКТІВ ЗА АКТИВНІСТЮ";
            sheet.Cell(1, 1).Style.Font.Bold = true;
            sheet.Cell(1, 1).Style.Font.FontSize = 14;
            sheet.Range(1, 1, 1, 7).Merge();

            // Заголовки колонок
            int headerRow = 3;
            sheet.Cell(headerRow, 1).Value = "Позиція";
            sheet.Cell(headerRow, 2).Value = "Назва";
            sheet.Cell(headerRow, 3).Value = "Категорія";
            sheet.Cell(headerRow, 4).Value = "К-ть операцій";
            sheet.Cell(headerRow, 5).Value = "Загальний оборот";
            sheet.Cell(headerRow, 6).Value = "Поточна к-ть";
            sheet.Cell(headerRow, 7).Value = "Од. виміру";

            // Стиль заголовків
            var headerRange = sheet.Range(headerRow, 1, headerRow, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.Gold;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Дані
            int row = headerRow + 1;
            int position = 1;
            foreach (var product in data.TopProducts)
            {
                sheet.Cell(row, 1).Value = position;
                sheet.Cell(row, 2).Value = product.ProductName;
                sheet.Cell(row, 3).Value = product.CategoryName;
                sheet.Cell(row, 4).Value = product.OperationsCount;
                sheet.Cell(row, 5).Value = product.TotalTurnover;
                sheet.Cell(row, 6).Value = product.CurrentQuantity;
                sheet.Cell(row, 7).Value = product.UnitOfMeasure;

                // Виділення топ-3
                if (position <= 3)
                {
                    var rowRange = sheet.Range(row, 1, row, 7);
                    rowRange.Style.Fill.BackgroundColor = position == 1 ? XLColor.Gold :
                                                           position == 2 ? XLColor.Silver :
                                                           XLColor.FromArgb(205, 127, 50); // Bronze
                }

                row++;
                position++;
            }

            // Автоширина
            sheet.Columns().AdjustToContents();
        }

        private void AddDataRow(IXLWorksheet sheet, int row, string label, object value)
        {
            sheet.Cell(row, 1).Value = label;
            sheet.Cell(row, 1).Style.Font.Bold = true;
            sheet.Cell(row, 2).Value = value.ToString();
        }

        private string GetUnitOfMeasureDisplay(UnitOfMeasure unit)
        {
            return unit switch
            {
                UnitOfMeasure.Kilograms => "кг",
                UnitOfMeasure.Liters => "л",
                _ => "шт"
            };
        }
    }
}

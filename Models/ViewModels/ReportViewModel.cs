using System;
using System.Collections.Generic;

namespace BIZFLOW.Web.Models.ViewModels
{
    public class ReportViewModel
    {
        // Загальна статистика
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public decimal TotalStockValue { get; set; }
        public int TotalOperations { get; set; }

        // Статистика операцій
        public decimal TotalIncoming { get; set; }
        public decimal TotalOutgoing { get; set; }
        public decimal NetBalance { get; set; }
        public int IncomingOperationsCount { get; set; }
        public int OutgoingOperationsCount { get; set; }

        // Середні показники
        public decimal AverageIncomingQuantity { get; set; }
        public decimal AverageOutgoingQuantity { get; set; }
        public decimal AverageStockPerProduct { get; set; }

        // Період звіту
        public DateTime ReportStartDate { get; set; }
        public DateTime ReportEndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;

        // Детальні дані
        public List<ProductReportItem> Products { get; set; } = new();
        public List<CategoryReportItem> Categories { get; set; } = new();
        public List<OperationReportItem> RecentOperations { get; set; } = new();
        public List<TopProductItem> TopProducts { get; set; } = new();
    }

    public class ProductReportItem
    {
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal CurrentQuantity { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
        public int OperationsCount { get; set; }
        public decimal TotalIncoming { get; set; }
        public decimal TotalOutgoing { get; set; }
        public DateTime? LastOperationDate { get; set; }
    }

    public class CategoryReportItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ProductsCount { get; set; }
        public decimal TotalQuantity { get; set; }
        public int OperationsCount { get; set; }
        public decimal TotalIncoming { get; set; }
        public decimal TotalOutgoing { get; set; }
    }

    public class OperationReportItem
    {
        public DateTime Date { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public string Description { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
    }

    public class TopProductItem
    {
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int OperationsCount { get; set; }
        public decimal TotalTurnover { get; set; }
        public decimal CurrentQuantity { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
    }
}

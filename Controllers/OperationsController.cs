using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BIZFLOW.Web.Data;
using BIZFLOW.Web.Models;
using BIZFLOW.Web.Models.ViewModels;
using BIZFLOW.Web.Services;

namespace BIZFLOW.Web.Controllers
{
    public class OperationsController : Controller
    {
        private readonly BizFlowDbContext _context;
        private readonly IReportService _reportService;

        public OperationsController(BizFlowDbContext context, IReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        // GET: Operations
        public async Task<IActionResult> Index(string sortOrder, string searchString, string operationType, DateTime? startDate, DateTime? endDate)
        {
            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["ProductSortParm"] = sortOrder == "Product" ? "product_desc" : "Product";
            ViewData["TypeSortParm"] = sortOrder == "Type" ? "type_desc" : "Type";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentType"] = operationType;
            ViewData["StartDate"] = startDate;
            ViewData["EndDate"] = endDate;

            var operations = _context.Operations.Include(o => o.Product).AsQueryable();

            // Search filter
            if (!String.IsNullOrEmpty(searchString))
            {
                operations = operations.Where(o => o.Product!.Name.Contains(searchString) 
                    || (o.Description != null && o.Description.Contains(searchString)));
            }

            // Type filter
            if (!String.IsNullOrEmpty(operationType))
            {
                operations = operations.Where(o => o.Type == operationType);
            }

            // Date range filter
            if (startDate.HasValue)
            {
                operations = operations.Where(o => o.Date >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                operations = operations.Where(o => o.Date <= endDate.Value);
            }

            // Sorting
            operations = sortOrder switch
            {
                "date_desc" => operations.OrderByDescending(o => o.Date),
                "Product" => operations.OrderBy(o => o.Product!.Name),
                "product_desc" => operations.OrderByDescending(o => o.Product!.Name),
                "Type" => operations.OrderBy(o => o.Type),
                "type_desc" => operations.OrderByDescending(o => o.Type),
                _ => operations.OrderBy(o => o.Date),
            };

            return View(await operations.ToListAsync());
        }

        // GET: Operations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operation = await _context.Operations
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (operation == null)
            {
                return NotFound();
            }

            return View(operation);
        }

        // GET: Operations/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // POST: Operations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductId,Quantity,Type,Date,Description")] Operation operation)
        {
            if (ModelState.IsValid)
            {
                // date of operation
                if (operation.Date == default)
                {
                    operation.Date = DateTime.Now;
                }

                // Set current user (get from session)
                operation.UserName = HttpContext.Session.GetString("UserName") ?? "Система";

                // find the product
                var product = await _context.Products.FindAsync(operation.ProductId);

                if (product == null)
                {
                    return NotFound();
                }

                // receipt/debit logic
                if (operation.Type == "Incoming")
                {
                    product.Quantity += operation.Quantity;
                }
                else if (operation.Type == "Outgoing")
                {
                    if (product.Quantity < operation.Quantity)
                    {
                        ModelState.AddModelError("", "Недостатньо товару на складі");
                        ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", operation.ProductId);
                        return View(operation);
                    }

                    product.Quantity -= operation.Quantity;
                }

                // Save remaining quantity after operation
                operation.RemainingQuantity = product.Quantity;

                // we keep
                _context.Add(operation);
                await _context.SaveChangesAsync();

                string unitDisplay = product.UnitOfMeasure switch
                {
                    UnitOfMeasure.Kilograms => "кг",
                    UnitOfMeasure.Liters => "л",
                    _ => "шт"
                };

                TempData["SuccessMessage"] = $"Операцію успішно створено. Поточний залишок: {product.Quantity} {unitDisplay}";
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", operation.ProductId);
            return View(operation);
        }

        // GET: Operations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operation = await _context.Operations.FindAsync(id);
            if (operation == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", operation.ProductId);
            return View(operation);
        }

        // POST: Operations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,Quantity,Type,Date")] Operation operation)
        {
            if (id != operation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(operation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OperationExists(operation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", operation.ProductId);
            return View(operation);
        }

        // GET: Operations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operation = await _context.Operations
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (operation == null)
            {
                return NotFound();
            }

            return View(operation);
        }

        // POST: Operations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var operation = await _context.Operations.FindAsync(id);
            if (operation != null)
            {
                _context.Operations.Remove(operation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OperationExists(int id)
        {
            return _context.Operations.Any(e => e.Id == id);
        }

        // GET: Operations/ProductHistory/5
        public async Task<IActionResult> ProductHistory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewData["ProductName"] = product.Name;
            ViewData["CurrentQuantity"] = product.Quantity;

            var operations = await _context.Operations
                .Include(o => o.Product)
                .Where(o => o.ProductId == id)
                .OrderByDescending(o => o.Date)
                .ToListAsync();

            return View(operations);
        }

        // GET: Operations/CreateSale
        // Show form to create a new sale
        public IActionResult CreateSale()
        {
            // Get products for dropdown
            var products = _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToList();

            ViewData["Products"] = new SelectList(products, "Id", "Name");

            // Pass products as JSON for client-side processing
            ViewData["ProductsJson"] = System.Text.Json.JsonSerializer.Serialize(
                products.Select(p => new 
                { 
                    id = p.Id, 
                    name = p.Name, 
                    quantity = p.Quantity
                })
            );

            return View(new CreateSaleViewModel());
        }

        // POST: Operations/CreateSale
        // Process sale form with multiple items and show confirmation page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSale(CreateSaleViewModel model)
        {
            // Check if at least one item is in the sale
            if (model.Items == null || !model.Items.Any())
            {
                TempData["ErrorMessage"] = "Будь ласка, додайте хоча б один товар";
                return RedirectToAction(nameof(CreateSale));
            }

            // List to store operations for confirmation
            var operations = new List<Operation>();

            // Validate each item and check product availability
            foreach (var item in model.Items)
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product == null)
                {
                    TempData["ErrorMessage"] = $"Товар з ID {item.ProductId} не знайдено";
                    return RedirectToAction(nameof(CreateSale));
                }

                // Validate quantity
                if (item.Quantity <= 0)
                {
                    TempData["ErrorMessage"] = $"Кількість для товару '{product.Name}' повинна бути більше нуля";
                    return RedirectToAction(nameof(CreateSale));
                }

                // Check if enough quantity available
                if (product.Quantity < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"Недостатньо товару '{product.Name}'. Доступно: {product.Quantity}";
                    return RedirectToAction(nameof(CreateSale));
                }

                // Build description with customer name if provided
                string description = model.Notes ?? "Продаж товару";
                if (!string.IsNullOrEmpty(model.CustomerName))
                {
                    description = $"Продаж клієнту: {model.CustomerName}. {description}";
                }

                // Create operation object for confirmation (not saved yet)
                var operation = new Operation
                {
                    ProductId = item.ProductId,
                    Product = product,
                    Quantity = item.Quantity,
                    Type = "Outgoing",
                    Date = DateTime.Now,
                    Description = description,
                    UserName = HttpContext.Session.GetString("UserName") ?? "Система",
                    RemainingQuantity = product.Quantity - item.Quantity
                };

                operations.Add(operation);
            }

            // Store data in TempData for confirmation page
            TempData["ConfirmationData"] = System.Text.Json.JsonSerializer.Serialize(new
            {
                CustomerName = model.CustomerName,
                Notes = model.Notes,
                Items = model.Items.Select(i => new { i.ProductId, i.Quantity }).ToList()
            });

            // Show confirmation page with all operations
            ViewData["CustomerName"] = model.CustomerName;
            ViewData["Notes"] = model.Notes;
            return View("ConfirmSale", operations);
        }

        // POST: Operations/ConfirmSale
        // Confirm and save multiple sale operations
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmSale()
        {
            // Retrieve confirmation data from TempData
            var confirmationDataJson = TempData["ConfirmationData"]?.ToString();
            if (string.IsNullOrEmpty(confirmationDataJson))
            {
                TempData["ErrorMessage"] = "Дані підтвердження не знайдено";
                return RedirectToAction(nameof(CreateSale));
            }

            var confirmationData = System.Text.Json.JsonSerializer.Deserialize<ConfirmationData>(confirmationDataJson);

            if (confirmationData?.Items == null || !confirmationData.Items.Any())
            {
                TempData["ErrorMessage"] = "Немає товарів для продажу";
                return RedirectToAction(nameof(CreateSale));
            }

            var savedOperations = new List<string>();

            // Process each item
            foreach (var item in confirmationData.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                if (product == null)
                {
                    TempData["ErrorMessage"] = $"Товар з ID {item.ProductId} не знайдено";
                    return RedirectToAction(nameof(CreateSale));
                }

                // Double-check quantity is still available
                if (product.Quantity < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"Недостатньо товару '{product.Name}'. Доступно: {product.Quantity}";
                    return RedirectToAction(nameof(CreateSale));
                }

                // Build description
                string description = confirmationData.Notes ?? "Продаж товару";
                if (!string.IsNullOrEmpty(confirmationData.CustomerName))
                {
                    description = $"Продаж клієнту: {confirmationData.CustomerName}. {description}";
                }

                // Decrease product quantity
                product.Quantity -= item.Quantity;

                // Create and save operation
                var operation = new Operation
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Type = "Outgoing",
                    Date = DateTime.Now,
                    Description = description,
                    UserName = HttpContext.Session.GetString("UserName") ?? "Система",
                    RemainingQuantity = product.Quantity
                };

                _context.Operations.Add(operation);

                var unitDisplay = product.UnitOfMeasure switch
                {
                    UnitOfMeasure.Kilograms => "кг",
                    UnitOfMeasure.Liters => "л",
                    _ => "шт"
                };

                savedOperations.Add($"{product.Name} - {item.Quantity} {unitDisplay}");
            }

            // Save all changes
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Продаж успішно створено! Продано товарів: {savedOperations.Count}. {string.Join(", ", savedOperations)}";
            return RedirectToAction(nameof(Index));
        }

        // Helper class for deserialization
        private class ConfirmationData
        {
            public string? CustomerName { get; set; }
            public string? Notes { get; set; }
            public List<ConfirmationItem> Items { get; set; } = new();
        }

        private class ConfirmationItem
        {
            public int ProductId { get; set; }
            public decimal Quantity { get; set; }
        }

        // GET: Operations/ExportToExcel
        public async Task<IActionResult> ExportToExcel()
        {
            var userName = User?.Identity?.Name ?? "Система";
            var reportData = await _reportService.GenerateReportDataAsync(userName);
            var excelFile = _reportService.GenerateExcelReport(reportData);

            var fileName = $"Звіт_BIZFLOW_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";
            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // GET: Operations/ExportToCsv
        public async Task<IActionResult> ExportToCsv()
        {
            var userName = User?.Identity?.Name ?? "Система";
            var reportData = await _reportService.GenerateReportDataAsync(userName);
            var csvFile = _reportService.GenerateCsvReport(reportData);

            var fileName = $"Звіт_BIZFLOW_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv";
            return File(csvFile, "text/csv", fileName);
        }
    }
}

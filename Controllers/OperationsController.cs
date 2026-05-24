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

        // Helper method to get current user ID from session
        private int? GetCurrentUserId()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            return null;
        }

        // GET: Operations
        public async Task<IActionResult> Index(string sortOrder, string searchString, string operationType, DateTime? startDate, DateTime? endDate)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["ProductSortParm"] = sortOrder == "Product" ? "product_desc" : "Product";
            ViewData["TypeSortParm"] = sortOrder == "Type" ? "type_desc" : "Type";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentType"] = operationType;
            ViewData["StartDate"] = startDate;
            ViewData["EndDate"] = endDate;

            // Filter operations by current user
            var operations = _context.Operations
                .Include(o => o.Product)
                .Where(o => o.UserId == currentUserId.Value)
                .AsQueryable();

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

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var operation = await _context.Operations
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUserId.Value);

            if (operation == null)
            {
                return NotFound();
            }

            return View(operation);
        }

        // GET: Operations/Create
        public IActionResult Create()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Only show products that belong to the current user
            var userProducts = _context.Products
                .Where(p => p.UserId == currentUserId.Value)
                .ToList();

            ViewData["ProductId"] = new SelectList(userProducts, "Id", "Name");
            return View();
        }

        // POST: Operations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductId,Quantity,Type,Date,Description")] Operation operation)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Set the UserId for the new operation
            operation.UserId = currentUserId.Value;

            // Remove UserId from ModelState validation
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                // date of operation
                if (operation.Date == default)
                {
                    operation.Date = DateTime.Now;
                }

                // Set current user (get from session)
                operation.UserName = HttpContext.Session.GetString("UserName") ?? "Система";

                // find the product and verify it belongs to current user
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == operation.ProductId && p.UserId == currentUserId.Value);

                if (product == null)
                {
                    ModelState.AddModelError("", "Товар не знайдено або він не належить вам");
                    ViewData["ProductId"] = new SelectList(
                        _context.Products.Where(p => p.UserId == currentUserId.Value),
                        "Id", "Name", operation.ProductId);
                    return View(operation);
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
                        ViewData["ProductId"] = new SelectList(
                            _context.Products.Where(p => p.UserId == currentUserId.Value),
                            "Id", "Name", operation.ProductId);
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

            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.UserId == currentUserId.Value),
                "Id", "Name", operation.ProductId);
            return View(operation);
        }

        // GET: Operations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var operation = await _context.Operations
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == currentUserId.Value);

            if (operation == null)
            {
                return NotFound();
            }

            // Only show products that belong to the current user
            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.UserId == currentUserId.Value),
                "Id", "Name", operation.ProductId);
            return View(operation);
        }

        // POST: Operations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,Quantity,Type,Date,Description")] Operation operation)
        {
            if (id != operation.Id)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the original operation from database and verify ownership
                    var originalOperation = await _context.Operations
                        .AsNoTracking()
                        .Include(o => o.Product)
                        .FirstOrDefaultAsync(o => o.Id == id && o.UserId == currentUserId.Value);

                    if (originalOperation == null)
                    {
                        return NotFound();
                    }

                    // Preserve the UserId
                    operation.UserId = currentUserId.Value;

                    // Get the product(s) involved and verify they belong to current user
                    var oldProduct = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == originalOperation.ProductId && p.UserId == currentUserId.Value);
                    Product? newProduct = null;

                    // Check if product changed
                    bool productChanged = originalOperation.ProductId != operation.ProductId;

                    if (productChanged)
                    {
                        newProduct = await _context.Products
                            .FirstOrDefaultAsync(p => p.Id == operation.ProductId && p.UserId == currentUserId.Value);
                        if (newProduct == null)
                        {
                            ModelState.AddModelError("", "Товар не знайдено або він не належить вам");
                            ViewData["ProductId"] = new SelectList(
                                _context.Products.Where(p => p.UserId == currentUserId.Value),
                                "Id", "Name", operation.ProductId);
                            return View(operation);
                        }
                    }
                    else
                    {
                        newProduct = oldProduct;
                    }

                    if (oldProduct == null || newProduct == null)
                    {
                        return NotFound();
                    }

                    // Step 1: Reverse the original operation effect
                    if (originalOperation.Type == "Incoming")
                    {
                        oldProduct.Quantity -= originalOperation.Quantity;
                    }
                    else if (originalOperation.Type == "Outgoing")
                    {
                        oldProduct.Quantity += originalOperation.Quantity;
                    }

                    // Step 2: Apply the new operation effect
                    if (operation.Type == "Incoming")
                    {
                        newProduct.Quantity += operation.Quantity;
                    }
                    else if (operation.Type == "Outgoing")
                    {
                        // Check if there's enough quantity
                        if (newProduct.Quantity < operation.Quantity)
                        {
                            // Restore original state before showing error
                            if (originalOperation.Type == "Incoming")
                            {
                                oldProduct.Quantity += originalOperation.Quantity;
                            }
                            else if (originalOperation.Type == "Outgoing")
                            {
                                oldProduct.Quantity -= originalOperation.Quantity;
                            }

                            string unitDisplay = newProduct.UnitOfMeasure switch
                            {
                                UnitOfMeasure.Kilograms => "кг",
                                UnitOfMeasure.Liters => "л",
                                _ => "шт"
                            };

                            ModelState.AddModelError("", $"Недостатньо товару '{newProduct.Name}' на складі. Доступно: {newProduct.Quantity} {unitDisplay}");
                            ViewData["ProductId"] = new SelectList(
                                _context.Products.Where(p => p.UserId == currentUserId.Value),
                                "Id", "Name", operation.ProductId);
                            return View(operation);
                        }

                        newProduct.Quantity -= operation.Quantity;
                    }

                    // Check for negative quantity
                    if (oldProduct.Quantity < 0)
                    {
                        // Restore to original state
                        if (originalOperation.Type == "Incoming")
                        {
                            oldProduct.Quantity += originalOperation.Quantity;
                        }
                        else if (originalOperation.Type == "Outgoing")
                        {
                            oldProduct.Quantity -= originalOperation.Quantity;
                        }

                        ModelState.AddModelError("", $"Операція призведе до від'ємного залишку товару '{oldProduct.Name}'");
                        ViewData["ProductId"] = new SelectList(
                            _context.Products.Where(p => p.UserId == currentUserId.Value),
                            "Id", "Name", operation.ProductId);
                        return View(operation);
                    }

                    // Update remaining quantity for the operation
                    operation.RemainingQuantity = newProduct.Quantity;

                    // Preserve original user and ensure current user is set
                    operation.UserName = originalOperation.UserName;

                    // Update the operation
                    _context.Update(operation);
                    await _context.SaveChangesAsync();

                    string unitDisplaySuccess = newProduct.UnitOfMeasure switch
                    {
                        UnitOfMeasure.Kilograms => "кг",
                        UnitOfMeasure.Liters => "л",
                        _ => "шт"
                    };

                    TempData["SuccessMessage"] = $"Операцію успішно оновлено. Поточний залишок '{newProduct.Name}': {newProduct.Quantity} {unitDisplaySuccess}";
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

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var operation = await _context.Operations
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUserId.Value);

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
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var operation = await _context.Operations
                .Include(o => o.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == currentUserId.Value);

            if (operation != null)
            {
                // Verify the product also belongs to the current user
                if (operation.Product != null && operation.Product.UserId != currentUserId.Value)
                {
                    TempData["ErrorMessage"] = "Ви не маєте прав на видалення цієї операції";
                    return RedirectToAction(nameof(Index));
                }

                // Reverse the quantity changes before deleting the operation
                var product = operation.Product;
                if (product != null)
                {
                    if (operation.Type == "Incoming")
                    {
                        // If we're deleting an incoming operation, subtract the quantity
                        // Check if this would result in negative quantity
                        if (product.Quantity < operation.Quantity)
                        {
                            TempData["ErrorMessage"] = $"Неможливо видалити операцію. Це призведе до від'ємного залишку товару '{product.Name}'. Поточний залишок: {product.Quantity}, операція: +{operation.Quantity}";
                            return RedirectToAction(nameof(Index));
                        }
                        product.Quantity -= operation.Quantity;
                    }
                    else if (operation.Type == "Outgoing")
                    {
                        // If we're deleting an outgoing operation, add the quantity back
                        product.Quantity += operation.Quantity;
                    }
                }

                _context.Operations.Remove(operation);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Операцію успішно видалено";
            }
            else
            {
                TempData["ErrorMessage"] = "Операцію не знайдено";
            }

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

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUserId.Value);

            if (product == null)
            {
                return NotFound();
            }

            ViewData["ProductName"] = product.Name;
            ViewData["CurrentQuantity"] = product.Quantity;

            var operations = await _context.Operations
                .Include(o => o.Product)
                .Where(o => o.ProductId == id && o.UserId == currentUserId.Value)
                .OrderByDescending(o => o.Date)
                .ToListAsync();

            return View(operations);
        }

        // GET: Operations/CreateSale
        // Show form to create a new sale
        public IActionResult CreateSale()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get products for dropdown - only for current user
            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.UserId == currentUserId.Value)
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
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

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
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.UserId == currentUserId.Value);

                if (product == null)
                {
                    TempData["ErrorMessage"] = $"Товар з ID {item.ProductId} не знайдено або він не належить вам";
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
                    RemainingQuantity = product.Quantity - item.Quantity,
                    UserId = currentUserId.Value
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
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

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
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.UserId == currentUserId.Value);

                if (product == null)
                {
                    TempData["ErrorMessage"] = $"Товар з ID {item.ProductId} не знайдено або він не належить вам";
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
                    RemainingQuantity = product.Quantity,
                    UserId = currentUserId.Value
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
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userName = HttpContext.Session.GetString("UserName") ?? "Система";
            var reportData = await _reportService.GenerateReportDataAsync(userName, currentUserId.Value);
            var excelFile = _reportService.GenerateExcelReport(reportData);

            var fileName = $"Звіт_BIZFLOW_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";
            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // GET: Operations/ExportToCsv
        public async Task<IActionResult> ExportToCsv()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userName = HttpContext.Session.GetString("UserName") ?? "Система";
            var reportData = await _reportService.GenerateReportDataAsync(userName, currentUserId.Value);
            var csvFile = _reportService.GenerateCsvReport(reportData);

            var fileName = $"Звіт_BIZFLOW_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv";
            return File(csvFile, "text/csv", fileName);
        }
    }
}

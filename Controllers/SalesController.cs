using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BIZFLOW.Web.Data;
using BIZFLOW.Web.Models;
using BIZFLOW.Web.Models.ViewModels;

namespace BIZFLOW.Web.Controllers
{
    // Controller to manage sales operations
    // Handles creating sales receipts and viewing sales history
    public class SalesController : Controller
    {
        private readonly BizFlowDbContext _context;

        // Constructor to inject database context
        public SalesController(BizFlowDbContext context)
        {
            _context = context;
        }

        // GET: Sales
        // Display list of all sales (receipts)
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string searchString)
        {
            // Store filter values for the view
            ViewData["StartDate"] = startDate;
            ViewData["EndDate"] = endDate;
            ViewData["CurrentFilter"] = searchString;

            // Start with all sales, including related items and products
            var sales = _context.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .AsQueryable();

            // Filter by date range if provided
            if (startDate.HasValue)
            {
                sales = sales.Where(s => s.SaleDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                sales = sales.Where(s => s.SaleDate <= endDate.Value);
            }

            // Search filter for receipt number or customer name
            if (!string.IsNullOrEmpty(searchString))
            {
                sales = sales.Where(s => 
                    s.ReceiptNumber.Contains(searchString) || 
                    (s.CustomerName != null && s.CustomerName.Contains(searchString)));
            }

            // Order by date descending (newest first)
            sales = sales.OrderByDescending(s => s.SaleDate);

            return View(await sales.ToListAsync());
        }

        // GET: Sales/Details/5
        // Show detailed view of a specific sale (receipt)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load sale with all related items and products
            var sale = await _context.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        // GET: Sales/Create
        // Display form to create a new sale
        public IActionResult Create()
        {
            // Get all products with their categories for the dropdown
            var products = _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToList();

            // Create SelectList for product dropdown
            ViewData["Products"] = new SelectList(products, "Id", "Name");

            // Pass products as JSON for client-side processing
            ViewData["ProductsJson"] = System.Text.Json.JsonSerializer.Serialize(
                products.Select(p => new 
                { 
                    id = p.Id, 
                    name = p.Name, 
                    price = p.Price,
                    quantity = p.Quantity,
                    unitOfMeasure = p.UnitOfMeasure.ToString()
                })
            );

            return View(new CreateSaleViewModel());
        }

        // POST: Sales/Create
        // Process the creation of a new sale
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSaleViewModel model)
        {
            // Check if at least one item is in the sale
            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError("", "Будь ласка, додайте хоча б один товар до чеку");
                PrepareCreateViewData();
                return View(model);
            }

            // Validate each item and check product availability
            foreach (var item in model.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                if (product == null)
                {
                    ModelState.AddModelError("", $"Товар з ID {item.ProductId} не знайдено");
                    PrepareCreateViewData();
                    return View(model);
                }

                // Check if enough quantity is available
                if (product.Quantity < item.Quantity)
                {
                    ModelState.AddModelError("", 
                        $"Недостатньо товару '{product.Name}'. Доступно: {product.Quantity}, Запитано: {item.Quantity}");
                    PrepareCreateViewData();
                    return View(model);
                }
            }

            // Create the sale entity
            var sale = new Sale
            {
                ReceiptNumber = GenerateReceiptNumber(),
                SaleDate = DateTime.Now,
                CustomerName = model.CustomerName,
                Notes = model.Notes,
                CashierName = HttpContext.Session.GetString("UserName") ?? "Система",
                TotalAmount = 0 // Will be calculated from items
            };

            // Create sale items and update product quantities
            decimal totalAmount = 0;
            foreach (var item in model.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                // Create sale item
                var saleItem = new SaleItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product!.Price,
                    LineTotal = item.Quantity * product.Price
                };

                sale.SaleItems.Add(saleItem);
                totalAmount += saleItem.LineTotal;

                // Decrease product quantity
                product.Quantity -= item.Quantity;

                // Create an operation record for tracking
                var operation = new Operation
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    Type = "Outgoing",
                    Date = DateTime.Now,
                    Description = $"Продаж (чек #{sale.ReceiptNumber})",
                    UserName = sale.CashierName,
                    RemainingQuantity = product.Quantity
                };

                _context.Operations.Add(operation);
            }

            // Set total amount
            sale.TotalAmount = totalAmount;

            // Save to database
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Чек #{sale.ReceiptNumber} успішно створено на суму {sale.TotalAmount:C2}";
            return RedirectToAction(nameof(Details), new { id = sale.Id });
        }

        // Helper method to generate unique receipt number
        private string GenerateReceiptNumber()
        {
            // Format: YYYYMMDD-HHMMSS-RND
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var random = new Random().Next(1000, 9999);
            return $"{timestamp}-{random}";
        }

        // Helper method to prepare view data for Create view
        private void PrepareCreateViewData()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToList();

            ViewData["Products"] = new SelectList(products, "Id", "Name");
            ViewData["ProductsJson"] = System.Text.Json.JsonSerializer.Serialize(
                products.Select(p => new 
                { 
                    id = p.Id, 
                    name = p.Name, 
                    price = p.Price,
                    quantity = p.Quantity,
                    unitOfMeasure = p.UnitOfMeasure.ToString()
                })
            );
        }

        // Check if sale exists
        private bool SaleExists(int id)
        {
            return _context.Sales.Any(e => e.Id == id);
        }
    }
}

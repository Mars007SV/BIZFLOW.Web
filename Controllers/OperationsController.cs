using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BIZFLOW.Web.Data;
using BIZFLOW.Web.Models;
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

                // Set current user (if you have authentication, use User.Identity.Name)
                operation.UserName = User?.Identity?.Name ?? "Система";

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

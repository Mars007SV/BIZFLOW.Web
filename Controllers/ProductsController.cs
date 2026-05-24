using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BIZFLOW.Web.Data;
using BIZFLOW.Web.Models;

namespace BIZFLOW.Web.Controllers
{
    // Controller for managing products in the warehouse
    public class ProductsController : Controller
    {
        private readonly BizFlowDbContext _context;

        // Constructor with dependency injection of database context
        public ProductsController(BizFlowDbContext context)
        {
            _context = context;
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

        // GET: Products
        // Display list of products with search and filter functionality
        public async Task<IActionResult> Index(string searchString, int? categoryFilter)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get products filtered by current user
            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.UserId == currentUserId.Value)
                .AsQueryable();

            // Search by product name
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            // Filter by category
            if (categoryFilter.HasValue && categoryFilter.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryFilter.Value);
            }

            // Pass category list for dropdown menu
            ViewData["Categories"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentCategory"] = categoryFilter;

            return View(await products.ToListAsync());
        }

        // GET: Products/Details/5
        // Show detailed information about specific product
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

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUserId.Value);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        // Display form for creating new product
        public IActionResult Create()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Populate category dropdown list
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        // Handle form submission for creating new product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Quantity,UnitOfMeasure,CategoryId")] Product product)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Set the UserId for the new product
            product.UserId = currentUserId.Value;

            // Remove UserId from ModelState validation since we set it manually
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                // Add product to database
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // If validation fails, reload category list
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        // Display form for editing existing product
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

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUserId.Value);

            if (product == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Quantity,UnitOfMeasure,CategoryId")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Verify the product belongs to the current user
            var existingProduct = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUserId.Value);

            if (existingProduct == null)
            {
                return NotFound();
            }

            // Preserve the UserId
            product.UserId = currentUserId.Value;

            // Remove UserId from ModelState validation
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
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

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUserId.Value);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUserId.Value);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}

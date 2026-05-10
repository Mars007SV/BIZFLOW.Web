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
    public class OperationsController : Controller
    {
        private readonly BizFlowDbContext _context;

        public OperationsController(BizFlowDbContext context)
        {
            _context = context;
        }

        // GET: Operations
        public async Task<IActionResult> Index()
        {
            var bizFlowDbContext = _context.Operations.Include(o => o.Product);
            return View(await bizFlowDbContext.ToListAsync());
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
        public async Task<IActionResult> Create([Bind("Id,ProductId,Quantity,Type,Date")] Operation operation)
        {
            if (ModelState.IsValid)
            {
                // date of operation
                if (operation.Date == default)
                {
                    operation.Date = DateTime.Now;
                }

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

                // we keep
                _context.Add(operation);
                await _context.SaveChangesAsync();

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
    }
}

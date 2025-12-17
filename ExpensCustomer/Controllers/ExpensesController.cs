using ExpensCustomer.Models;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ExpensCustomer.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Expenses
        public async Task<IActionResult> Index()
        {
            try
            {
                var expense = await _context.Expenses
                                      .Include(e => e.Customer)
                                      .Include(e => e.ExpenseItems)
                                      .ToListAsync();
                return View(expense);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var expense = await _context.Expenses
                    .Include(e => e.Customer)
                    .Include(e => e.ExpenseItems)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (expense == null)
                {
                    return NotFound();
                }

                return View(expense);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        // GET: Expenses/Create
        public IActionResult Create()
        {
            try
            {
                ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name");
                return View(new Expense { ExpenseItems = new List<ExpenseItem>() });
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(expense);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", expense.CustomerId);
                return View(expense);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }



        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var expense = await _context.Expenses
                                            .Include(e => e.ExpenseItems)
                                            .FirstOrDefaultAsync(e => e.Id == id);
                if (expense == null)
                {
                    return NotFound();
                }
                ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", expense.CustomerId);
                return View(expense);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Expense expense)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var message = string.Join(" | ", ModelState.Values
                                     .SelectMany(v => v.Errors)
                                     .Select(e => e.ErrorMessage));
                    ModelState.AddModelError("", message);
                   return View(expense);
                   ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", expense.CustomerId);
                    return View(expense);
                }

                // Load existing expense with items
                var existing = await _context.Expenses
                                             .Include(e => e.ExpenseItems)
                                             .FirstOrDefaultAsync(e => e.Id == expense.Id);

                if (existing == null)
                {
                    ModelState.AddModelError("", "Expense not found.");
                    ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", expense.CustomerId);
                    return View(expense);
                }

                // Update main expense fields
                existing.CustomerId = expense.CustomerId;
                existing.ExpenseDate = expense.ExpenseDate;
                existing.Note = expense.Note;

                // Track existing and posted ExpenseItems
                var postedItemIds = expense.ExpenseItems?.Where(i => i.Id > 0).Select(i => i.Id).ToList() ?? new List<int>();

                // Remove deleted items
                var itemsToRemove = existing.ExpenseItems.Where(i => !postedItemIds.Contains(i.Id)).ToList();
                if (itemsToRemove.Any())
                {
                    _context.ExpenseItems.RemoveRange(itemsToRemove);
                }

                // Update existing items and add new items
                if (expense.ExpenseItems != null)
                {
                    foreach (var item in expense.ExpenseItems)
                    {
                        if (item.Id > 0)
                        {
                            // Update existing item
                            var existingItem = existing.ExpenseItems.FirstOrDefault(i => i.Id == item.Id);
                            if (existingItem != null)
                            {
                                existingItem.ExpenseType = item.ExpenseType;
                                existingItem.Amount = item.Amount;
                            }
                        }
                        else
                        {
                            // Add new item
                            item.ExpenseId = existing.Id;
                            _context.ExpenseItems.Add(item);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", expense.CustomerId);
                return View(expense);
            }
        }


        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .Include(e => e.Customer)
                .Include(e=>e.ExpenseItems)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }

        

        public IActionResult CustomerWise(int customerId)
        {
            
            var data = _context.Expenses
                .Include(e => e.Customer)
                .Include(e => e.ExpenseItems)
                .Where(e => e.CustomerId == customerId)
                .ToList();

         
            ViewBag.Total = data.Sum(e => e.ExpenseItems.Sum(i => i.Amount));

         
            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.SelectedCustomerId = customerId;

            return View(data);
        }
    }
}

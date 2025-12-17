using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpensCustomer.Models;

namespace ExpensCustomer.Controllers
{
    public class ExpenseItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpenseItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ExpenseItems
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ExpenseItems.Include(e => e.Expense);
            return View(await applicationDbContext.ToListAsync());
        }

        
    }
}

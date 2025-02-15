using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Leblebi.Data;
using Leblebi.Models;
using Leblebi.ViewModels;

namespace Leblebi.Controllers
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
            var expenses = await _context.Expenses.Include(e => e.ExpenseCategory)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
            return View(expenses);
        }

        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .Include(e => e.ExpenseCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        public async Task<IActionResult> Report(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            if (!endDate.HasValue)
                endDate = DateTime.Now;

            // Xərcləri tarixə görə süz
            var expenses = await _context.Expenses
                .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
                .Include(e => e.ExpenseCategory)
                .ToListAsync();

            // Parent və Subcategory-ləri qur
            var groupedExpenses = expenses
                .GroupBy(e => e.ExpenseCategory)
                .Select(g => new ExpenseReportViewModel
                {
                    CategoryId = g.Key.Id,
                    CategoryName = g.Key.Name,
                    ParentCategoryId = g.Key.ParentCategoryId,
                    TotalAmount = g.Sum(e => e.Amount)
                })
                .ToList();

            // Parent-Child Strukturu Qur
            var parentCategories = groupedExpenses
                .Where(c => c.ParentCategoryId == null)
                .Select(p => new ExpenseReportViewModel
                {
                    CategoryId = p.CategoryId,
                    CategoryName = p.CategoryName,
                    TotalAmount = p.TotalAmount + groupedExpenses
                        .Where(c => c.ParentCategoryId == p.CategoryId)
                        .Sum(c => c.TotalAmount), // Subcategory-ləri əlavə edir
                    Subcategories = groupedExpenses
                        .Where(c => c.ParentCategoryId == p.CategoryId)
                        .ToList()
                })
                .ToList();

            return View(parentCategories);
        }


        // GET: Expenses/Create
        public IActionResult Create()
        {
            List<ExpenseCategory> expenseCategories = _context.ExpenseCategories
                .Where(x => x.ParentCategoryId == null).ToList();
            List<ExpenseCategory> expenseSubCategories = _context.ExpenseCategories
                .Where(x => x.ParentCategoryId != null).ToList();

            ViewData["ExpenseCategoryId"] = new SelectList(expenseCategories, "Id", "Name");
            ViewData["ExpenseSubCategoryId"] = new SelectList(expenseSubCategories, "Id", "Name");
            return View();
        }

        public JsonResult GetSubCategories(int id)
        {
            var expenseSubCategories = _context.ExpenseCategories
                .Where(x => x.ParentCategoryId == id)
                .Select(x=> new
                {
                    Value = x.Id,
                    Text = x.Name
                })
                .ToList();
            return Json(expenseSubCategories);
        }

        // POST: Expenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ExpenseCategoryId,Amount,ExpenseDate,Note")] Expense expense)
        {
            if (ModelState.IsValid)
            {
                expense.ExpenseDate = DateTime.SpecifyKind(expense.ExpenseDate, DateTimeKind.Utc);
                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            List<ExpenseCategory> expenseCategories = _context.ExpenseCategories
                .Where(x=>x.ParentCategoryId == null).ToList();
            ViewData["ExpenseCategoryId"] = new SelectList(expenseCategories, "Id", "Name", expense.ExpenseCategoryId);
            return View(expense);
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }
            ViewData["ExpenseCategoryId"] = new SelectList(_context.ExpenseCategories, "Id", "Name", expense.ExpenseCategoryId);
            return View(expense);
        }

        // POST: Expenses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ExpenseCategoryId,Amount,ExpenseDate,Note")] Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                expense.ExpenseDate = DateTime.SpecifyKind(expense.ExpenseDate, DateTimeKind.Utc);
                try
                {
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
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
            ViewData["ExpenseCategoryId"] = new SelectList(_context.ExpenseCategories, "Id", "Name", expense.ExpenseCategoryId);
            return View(expense);
        }

        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .Include(e => e.ExpenseCategory)
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
    }
}

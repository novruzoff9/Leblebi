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
                .Where(x => x.ExpenseDate.Year == DateTime.Now.Year && x.ExpenseDate.Month == DateTime.Now.Month)
                .OrderBy(x=>x.ExpenseCategoryId)
                .ToListAsync();

            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            List<MonthlyReportViewModel> monthlyReports = new List<MonthlyReportViewModel>();

            MonthlyReportViewModel totalMonthlyReport = new MonthlyReportViewModel
            {
                Title = "Ümumi",
                Reports = new List<DailyReport>()
            };
            foreach (var item in expenses)
            {
                var monthlyReport = monthlyReports.FirstOrDefault(x => x.Title == item.ExpenseCategory.Name);


                if (monthlyReport == null)
                {
                    monthlyReport = new MonthlyReportViewModel
                    {
                        Title = item.ExpenseCategory.Name,
                        Reports = new List<DailyReport>()
                    };
                    monthlyReports.Add(monthlyReport);
                }

                DailyReport dailyReport = new DailyReport
                {
                    Date = DateOnly.FromDateTime(item.ExpenseDate),
                    Value = item.Amount
                };
                monthlyReport.TotalValue += item.Amount;
                monthlyReport.Reports.Add(dailyReport);
            }

            for (var date = firstDayOfMonth; date <= lastDayOfMonth; date = date.AddDays(1))
            {
                var dailyExpense = expenses.Where(x => x.ExpenseDate.Date == date.Date).Sum(x => x.Amount);
                totalMonthlyReport.Reports.Add(new DailyReport
                {
                    Date = DateOnly.FromDateTime(date),
                    Value = dailyExpense
                });
                totalMonthlyReport.TotalValue += dailyExpense;
            }
            monthlyReports.Add(totalMonthlyReport);

            return View(monthlyReports);
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


        public async Task<IActionResult> Create()
        {
            var categories = await _context.ExpenseCategories
                .Include(c => c.Expenses.OrderBy(x=>x.ExpenseDate))
                .Where(c=>c.expenseCategories.Count == 0)
                .ToListAsync();

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

            var categoryViewModels = categories.Select(c => new ExpenseCategoryViewModel
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                DailyExpenses = Enumerable.Range(1, daysInMonth)
                    .ToDictionary(day => day, day =>
                        c.Expenses
                            .FirstOrDefault(e => e.ExpenseDate.Year == currentYear
                                                 && e.ExpenseDate.Month == currentMonth
                                                 && e.ExpenseDate.Day == day)
                            ?.Amount) 
            }).ToList();


            var model = new MonthlyReportCreateViewModel { Expenses = categoryViewModels };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonthlyReportCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                foreach (var expenseCategory in model.Expenses)
                {
                    foreach (var kvp in expenseCategory.DailyExpenses)
                    {
                        if (kvp.Value.HasValue)
                        {
                            var expense = new Expense
                            {
                                ExpenseCategoryId = expenseCategory.CategoryId,
                                Amount = kvp.Value.Value,
                                ExpenseDate = firstDayOfMonth.AddDays(kvp.Key - 1)
                            };

                            _context.Expenses.Add(expense);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }



        //// GET: Expenses/Create
        //public IActionResult Create()
        //{
        //    List<ExpenseCategory> expenseCategories = _context.ExpenseCategories
        //        .Where(x => x.ParentCategoryId == null).ToList();
        //    List<ExpenseCategory> expenseSubCategories = _context.ExpenseCategories
        //        .Where(x => x.ParentCategoryId != null).ToList();

        //    ViewData["ExpenseCategoryId"] = new SelectList(expenseCategories, "Id", "Name");
        //    ViewData["ExpenseSubCategoryId"] = new SelectList(expenseSubCategories, "Id", "Name");
        //    return View();
        //}


        //// POST: Expenses/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,ExpenseCategoryId,Amount,ExpenseDate,Note")] Expense expense)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        expense.ExpenseDate = DateTime.SpecifyKind(expense.ExpenseDate, DateTimeKind.Utc);
        //        _context.Add(expense);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    List<ExpenseCategory> expenseCategories = _context.ExpenseCategories
        //        .Where(x => x.ParentCategoryId == null).ToList();
        //    ViewData["ExpenseCategoryId"] = new SelectList(expenseCategories, "Id", "Name", expense.ExpenseCategoryId);
        //    return View(expense);
        //}

        public JsonResult GetSubCategories(int id)
        {
            var expenseSubCategories = _context.ExpenseCategories
                .Where(x => x.ParentCategoryId == id)
                .Select(x => new
                {
                    Value = x.Id,
                    Text = x.Name
                })
                .ToList();
            return Json(expenseSubCategories);
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

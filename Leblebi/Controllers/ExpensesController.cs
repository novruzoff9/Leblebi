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
using Leblebi.DTOs;
using Leblebi.Helper;

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
        public async Task<IActionResult> Index(int? year, int? month)
        {
            if(month == null)
            {
                month = DateOnly.FromDateTime(DateTime.Now).Month;
            }
            if(year == null)
            {
                year = DateOnly.FromDateTime(DateTime.Now).Year;
            }
            ViewBag.year = year;
            ViewBag.month = month;
            var expenses = await _context.Expenses.Include(e => e.ExpenseCategory)
                .Where(x => x.ExpenseDate.Year == year && x.ExpenseDate.Month == month)
                .OrderBy(x => x.ExpenseCategoryId)
                .ToListAsync();

            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            List<MonthlyReportViewModel> monthlyReports = new List<MonthlyReportViewModel>();

            List<Expense> otherExpenses = new();

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
                    Value = item.Amount,
                    Note = item.Note
                };
                monthlyReport.TotalValue += item.Amount;
                if (item.ExpenseCategoryId == 13)
                {
                    otherExpenses.Add(item);
                }

                if (monthlyReport.Reports.Any(x => x.Date == dailyReport.Date))
                {
                    var existingReport = monthlyReport.Reports.First(x => x.Date == dailyReport.Date);
                    existingReport.Value += dailyReport.Value;
                    monthlyReport.TotalValue += dailyReport.Value;
                    continue;
                }
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

            monthlyReports = monthlyReports.OrderBy(x => x.Title).ToList();

            otherExpenses = otherExpenses.OrderBy(x=>x.ExpenseDate).ToList();
            ExpensesViewModel monthlyReportsViewModel = new ExpensesViewModel
            {
                MonthlyExpenses = monthlyReports,
                OtherExpenses = otherExpenses
            };

            return View(monthlyReportsViewModel);
        }

        [HttpGet]
        public IActionResult AddOtherExpenses()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddOtherExpenses(decimal amount, DateTime expenseDate, string note)
        {
            var expense = new Expense
            {
                Amount = amount,
                ExpenseDate = expenseDate,
                ExpenseCategoryId = 13,
                Note = note
            };
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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


        public async Task<IActionResult> Create(int selectedYear, int selectedMonth)
        {
            var categories = await _context.ExpenseCategories
                .Include(c => c.Expenses.OrderBy(x => x.ExpenseDate))
                .Where(c => c.expenseCategories.Count == 0)
                .ToListAsync();

            var daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);

            ViewBag.year = selectedYear;
            ViewBag.month = selectedMonth;

            var categoryViewModels = categories.Select(c => new ExpenseCategoryViewModel
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                DailyExpenses = Enumerable.Range(1, daysInMonth)
                    .ToDictionary(day => day, day =>
                        c.Expenses
                            .FirstOrDefault(e => e.ExpenseDate.Year == selectedYear
                                                 && e.ExpenseDate.Month == selectedMonth
                                                 && e.ExpenseDate.Day == day)
                            ?.Amount)
            }).OrderBy(x => x.CategoryName)
            .ToList();

            categoryViewModels.Remove(categoryViewModels.FirstOrDefault(x=>x.CategoryName == "Digər xərclər"));


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

                            var alredyData = await _context.Expenses
                                .FirstOrDefaultAsync(x => x.ExpenseCategoryId == expense.ExpenseCategoryId
                                                        && x.ExpenseDate == expense.ExpenseDate);

                            if (alredyData == null)
                            {
                                _context.Expenses.Add(expense);
                            }
                            else
                            {
                                alredyData.Amount = expense.Amount;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }


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

        public IActionResult ExcelReport(int year, int month)
        {
            var expenses = _context.Expenses
                .Include(e => e.ExpenseCategory)
                .Where(x => x.ExpenseDate.Year == year && x.ExpenseDate.Month == month)
                .OrderBy(x => x.ExpenseCategoryId)
                .ToList();
            var categories = _context.ExpenseCategories
                .Include(c => c.expenseCategories)
                .Where(x => x.expenseCategories.Count == 0)
                .Select(x=>x.Name).ToList();
            var report = new ExcelReportDto
            {
                Title = "Xərclər",
                Headers = categories,
                Data = new List<DailyReportDto>()
            };
            foreach (var item in expenses)
            {
                var dailyReport = report.Data.FirstOrDefault(x => x.Title == item.ExpenseCategory.Name);
                if (dailyReport == null)
                {
                    dailyReport = new DailyReportDto
                    {
                        Title = item.ExpenseCategory.Name,
                        ValueofDay = new Dictionary<int, string>()
                    };
                    report.Data.Add(dailyReport);
                }
                if(dailyReport.ValueofDay.ContainsKey(item.ExpenseDate.Day))
                {
                    dailyReport.ValueofDay[item.ExpenseDate.Day] = (Convert.ToDecimal(dailyReport.ValueofDay[item.ExpenseDate.Day]) + item.Amount).ToString();
                    continue;
                }
                dailyReport.ValueofDay.Add(item.ExpenseDate.Day, item.Amount.ToString());
            }

            var fileContent = ExcelHelper.GenerateWorksheet(report, year, month);

            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "expenses.xlsx");
        }
    }
}

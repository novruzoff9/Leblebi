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
    public class ManagementExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagementExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ManagementExpenses
        public async Task<IActionResult> Index(int? year, int? month)
        {
            if (month == null)
            {
                month = DateOnly.FromDateTime(DateTime.Now).Month;
            }
            if (year == null)
            {
                year = DateOnly.FromDateTime(DateTime.Now).Year;
            }
            ViewBag.year = year;
            ViewBag.month = month;
            var expenses = await _context.ManagementExpenses.Include(e => e.ManagementCategory)
                .Where(x => x.ExpenseDate.Year == year && x.ExpenseDate.Month == month)
                .OrderBy(x => x.ManagementCategoryId)
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
                var monthlyReport = monthlyReports.FirstOrDefault(x => x.Title == item.ManagementCategory.Name);


                if (monthlyReport == null)
                {
                    monthlyReport = new MonthlyReportViewModel
                    {
                        Title = item.ManagementCategory.Name,
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

            otherExpenses = otherExpenses.OrderBy(x => x.ExpenseDate).ToList();
            ExpensesViewModel monthlyReportsViewModel = new ExpensesViewModel
            {
                MonthlyExpenses = monthlyReports,
                OtherExpenses = otherExpenses
            };

            return View(monthlyReportsViewModel);
        }

        // GET: ManagementExpenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var managementExpenses = await _context.ManagementExpenses
                .Include(m => m.ManagementCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (managementExpenses == null)
            {
                return NotFound();
            }

            return View(managementExpenses);
        }

        // GET: ManagementExpenses/Create
        public async Task<IActionResult> Create(int selectedYear, int selectedMonth)
        {
            var categories = await _context.ManagementCategories
                .Include(c => c.Expenses.OrderBy(x => x.ExpenseDate))
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
                            var expense = new ManagementExpense
                            {
                                ManagementCategoryId = expenseCategory.CategoryId,
                                Amount = kvp.Value.Value,
                                ExpenseDate = firstDayOfMonth.AddDays(kvp.Key - 1)
                            };

                            var alredyData = await _context.ManagementExpenses
                                .FirstOrDefaultAsync(x => x.ManagementCategoryId == expense.ManagementCategoryId
                                                        && x.ExpenseDate == expense.ExpenseDate);

                            if (alredyData == null)
                            {
                                _context.ManagementExpenses.Add(expense);
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


        // GET: ManagementExpenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var managementExpenses = await _context.ManagementExpenses.FindAsync(id);
            if (managementExpenses == null)
            {
                return NotFound();
            }
            ViewData["ManagementCategoryId"] = new SelectList(_context.ManagementCategories, "Id", "Id", managementExpenses.ManagementCategoryId);
            return View(managementExpenses);
        }

        // POST: ManagementExpenses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Amount,ExpenseDate,ManagementCategoryId")] ManagementExpense managementExpenses)
        {
            if (id != managementExpenses.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(managementExpenses);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ManagementExpensesExists(managementExpenses.Id))
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
            ViewData["ManagementCategoryId"] = new SelectList(_context.ManagementCategories, "Id", "Id", managementExpenses.ManagementCategoryId);
            return View(managementExpenses);
        }

        // GET: ManagementExpenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var managementExpenses = await _context.ManagementExpenses
                .Include(m => m.ManagementCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (managementExpenses == null)
            {
                return NotFound();
            }

            return View(managementExpenses);
        }

        // POST: ManagementExpenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var managementExpenses = await _context.ManagementExpenses.FindAsync(id);
            if (managementExpenses != null)
            {
                _context.ManagementExpenses.Remove(managementExpenses);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ManagementExpensesExists(int id)
        {
            return _context.ManagementExpenses.Any(e => e.Id == id);
        }

        public IActionResult ExcelReport(int year, int month)
        {
            var expenses = _context.ManagementExpenses
                .Include(e => e.ManagementCategory)
                .Where(x => x.ExpenseDate.Year == year && x.ExpenseDate.Month == month)
                .OrderBy(x => x.ManagementCategoryId)
                .ToList();
            var categories = _context.ManagementCategories
                .Select(x => x.Name).ToList();
            var report = new ExcelReportDto
            {
                Title = "Xərclər",
                Headers = categories,
                Data = new List<DailyReportDto>()
            };
            foreach (var item in expenses)
            {
                var dailyReport = report.Data.FirstOrDefault(x => x.Title == item.ManagementCategory.Name);
                if (dailyReport == null)
                {
                    dailyReport = new DailyReportDto
                    {
                        Title = item.ManagementCategory.Name,
                        ValueofDay = new Dictionary<int, string>()
                    };
                    report.Data.Add(dailyReport);
                }
                if (dailyReport.ValueofDay.ContainsKey(item.ExpenseDate.Day))
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

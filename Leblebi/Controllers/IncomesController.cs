using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Leblebi.Data;
using Leblebi.Models;
using Leblebi.Enums;
using Leblebi.Helper;
using Leblebi.ViewModels;

namespace Leblebi.Controllers
{
    public class IncomesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IncomesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Incomes
        public async Task<IActionResult> Index()
        {
            var incomes = await _context.Incomes
                .OrderByDescending(e => e.IncomeDate)
                .ToListAsync();

            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            List<MonthlyReportViewModel> monthlyReports = new List<MonthlyReportViewModel>();

            MonthlyReportViewModel totalMonthlyReport = new MonthlyReportViewModel
            {
                Title = "Ümumi",
                Reports = new List<DailyReport>()
            };

            foreach (var item in incomes)
            {
                var monthlyReport = monthlyReports.FirstOrDefault(x => x.Title == EnumHelper.GetDisplayName(item.IncomeType));


                if (monthlyReport == null)
                {
                    monthlyReport = new MonthlyReportViewModel
                    {
                        Title = EnumHelper.GetDisplayName(item.IncomeType),
                        Reports = new List<DailyReport>()
                    };
                    monthlyReports.Add(monthlyReport);
                }

                DailyReport dailyReport = new DailyReport
                {
                    Date = DateOnly.FromDateTime(item.IncomeDate),
                    Value = item.Amount
                };
                monthlyReport.TotalValue += item.Amount;
                monthlyReport.Reports.Add(dailyReport);
            }

            for (var date = firstDayOfMonth; date <= lastDayOfMonth; date = date.AddDays(1))
            {
                var dailyExpense = incomes.Where(x => x.IncomeDate.Date == date.Date).Sum(x => x.Amount);
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

        // GET: Incomes/Create
        public async Task<IActionResult> Create()
        {
            var incomes = await _context.Incomes
                .OrderByDescending(e => e.IncomeDate)
                .ToListAsync();

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

            List<IncomesOfType> incomesOfTypes = new List<IncomesOfType>();

            foreach (IncomeType incomeType in Enum.GetValues(typeof(IncomeType)))
            {
                var incomesOfType = new IncomesOfType
                {
                    IncomeType = incomeType.ToString(),
                    Incomes = incomes.Where(e => e.IncomeType == incomeType).ToList()
                };
                incomesOfTypes.Add(incomesOfType);
            }

            var incomeViewModels = incomesOfTypes.Select(c => new ExpenseCategoryViewModel
            {
                CategoryId = c.IncomeType.ToString() == "Cash" ? 1 : 0,
                CategoryName = c.IncomeType,
                DailyExpenses = Enumerable.Range(1, daysInMonth)
                    .ToDictionary(day => day, day =>
                        incomes
                            .FirstOrDefault(e => e.IncomeDate.Year == currentYear
                                                 && e.IncomeDate.Month == currentMonth
                                                 && e.IncomeDate.Day == day)
                            ?.Amount)
            }).ToList();


            var model = new MonthlyReportCreateViewModel { Expenses = incomeViewModels };

            return View(model);
        }

        // POST: Incomes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

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
                            var expense = new Income
                            {
                                IncomeType = (expenseCategory.CategoryId == 1 ? IncomeType.Cash : IncomeType.PosTerminal),
                                Amount = kvp.Value.Value,
                                IncomeDate = firstDayOfMonth.AddDays(kvp.Key - 1)
                            };

                            _context.Incomes.Add(expense);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Incomes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var income = await _context.Incomes.FindAsync(id);
            if (income == null)
            {
                return NotFound();
            }
            return View(income);
        }

        // POST: Incomes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Amount,IncomeType,IncomeDate")] Income income)
        {
            if (id != income.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                income.IncomeDate = DateTime.SpecifyKind(income.IncomeDate, DateTimeKind.Utc);
                try
                {
                    _context.Update(income);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IncomeExists(income.Id))
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
            return View(income);
        }

        private bool IncomeExists(int id)
        {
            return _context.Incomes.Any(e => e.Id == id);
        }
    }
}

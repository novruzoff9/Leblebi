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
using Leblebi.DTOs;

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
            var incomes = await _context.Incomes
                .OrderByDescending(e => e.IncomeDate)
                .ToListAsync();

            var firstDayOfMonth = new DateTime(year ?? 1, month ?? 1, 1);
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
        public async Task<IActionResult> Create(int selectedYear, int selectedMonth)
        {
            var incomes = await _context.Incomes
                .OrderByDescending(e => e.IncomeDate)
                .ToListAsync();

            var daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);

            ViewBag.year = selectedYear;
            ViewBag.month = selectedMonth;

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
                            .FirstOrDefault(e => e.IncomeDate.Year == selectedYear
                                                 && e.IncomeDate.Month == selectedMonth
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

                            var alredyData = await _context.Incomes
                                .FirstOrDefaultAsync(x => x.IncomeType == expense.IncomeType
                                                        && x.IncomeDate == expense.IncomeDate);

                            if (alredyData == null)
                            {
                                _context.Incomes.Add(expense);
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


        public IActionResult ExcelReport(int year, int month)
        {
            var expenses = _context.Incomes
                .Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == month)
                .ToList();
            var report = new ExcelReportDto
            {
                Title = "Gəlirlər",
                Headers = new List<string> { "Nağd Pul", "POS Terminal" },
                Data = new List<DailyReportDto>()
            };
            foreach (var item in expenses)
            {
                var dailyReport = report.Data.FirstOrDefault(x => x.Title == EnumHelper.GetDisplayName(item.IncomeType));
                if (dailyReport == null)
                {
                    dailyReport = new DailyReportDto
                    {
                        Title = EnumHelper.GetDisplayName(item.IncomeType),
                        ValueofDay = new Dictionary<int, string>()
                    };
                    report.Data.Add(dailyReport);
                }
                if (dailyReport.ValueofDay.ContainsKey(item.IncomeDate.Day))
                {
                    dailyReport.ValueofDay[item.IncomeDate.Day] = (Convert.ToDecimal(dailyReport.ValueofDay[item.IncomeDate.Day]) + item.Amount).ToString();
                    continue;
                }
                dailyReport.ValueofDay.Add(item.IncomeDate.Day, item.Amount.ToString());
            }

            var fileContent = ExcelHelper.GenerateWorksheet(report, year, month);

            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "incomes.xlsx");
        }
    }
}

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
    public class EmployeeSalariesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeSalariesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EmployeeSalaries
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
            var salaries = await _context.Salaries.Include(e => e.Employee)
                .Where(x => x.SalaryDate.Year == year && x.SalaryDate.Month == month)
                .OrderByDescending(e => e.EmployeeId)
                .ToListAsync();

            var employees = await _context.Employees
                .Where(x => ((x.HireDate.Year < year) || (x.HireDate.Year == year && x.HireDate.Month <= month)) && (( x.FireDate.Value.Year > year ) || 
                    (x.FireDate.Value.Year == year && x.FireDate.Value.Month >= month) || !x.FireDate.HasValue))
                .Include(x => x.Salaries.Where(x => x.SalaryDate.Year == year && x.SalaryDate.Month == month))
                .ToListAsync();

            var firstDayOfMonth = new DateTime(year ?? 1, month ?? 1, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            List<MonthlyReportViewModel> monthlyReports = new List<MonthlyReportViewModel>();

            MonthlyReportViewModel totalMonthlyReport = new MonthlyReportViewModel
            {
                Title = "Ümumi",
                Reports = new List<DailyReport>()
            };
            foreach (var item in employees)
            {
                string employeeName = item.Name + " " + item.Surname;
                var monthlyReport = monthlyReports.FirstOrDefault(x => x.Title == employeeName);


                if (monthlyReport == null)
                {
                    monthlyReport = new MonthlyReportViewModel
                    {
                        Title = employeeName,
                        Reports = new List<DailyReport>()
                    };
                    monthlyReports.Add(monthlyReport);
                }

                foreach (var salary in item.Salaries)
                {
                    DailyReport dailyReport = new DailyReport
                    {
                        Date = DateOnly.FromDateTime(salary.SalaryDate),
                        Value = salary.Amount
                    };
                    monthlyReport.TotalValue += salary.Amount;
                    monthlyReport.Reports.Add(dailyReport);
                }
                    monthlyReport.SecondValue = item.Salary - monthlyReport.TotalValue;

            }

            for (var date = firstDayOfMonth; date <= lastDayOfMonth; date = date.AddDays(1))
            {
                var dailyExpense = salaries.Where(x => x.SalaryDate.Date == date.Date).Sum(x => x.Amount);
                totalMonthlyReport.Reports.Add(new DailyReport
                {
                    Date = DateOnly.FromDateTime(date),
                    Value = dailyExpense
                });
                totalMonthlyReport.TotalValue += dailyExpense;
                totalMonthlyReport.SecondValue = monthlyReports.Sum(r => r.SecondValue);
            }
            monthlyReports.Add(totalMonthlyReport);
            return View(monthlyReports);
        }

        public async Task<IActionResult> Create(int selectedYear, int selectedMonth)
        {
            var employees = await _context.Employees
                .Include(c => c.Salaries.OrderBy(x => x.SalaryDate))
                .Where(x => (x.FireDate.Value.Year < selectedYear) ||
                    (x.FireDate.Value.Year == selectedYear && x.FireDate.Value.Month <= selectedMonth) || !x.FireDate.HasValue)
                .ToListAsync();

            var daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);

            ViewBag.year = selectedYear;
            ViewBag.month = selectedMonth;

            var employeeViewModels = employees.Select(e => new ExpenseCategoryViewModel
            {
                CategoryId = e.Id,
                CategoryName = e.Name + " " + e.Surname,
                DailyExpenses = Enumerable.Range(1, daysInMonth)
                    .ToDictionary(day => day, day =>
                        e.Salaries
                            .FirstOrDefault(e => e.SalaryDate.Year == selectedYear
                                                 && e.SalaryDate.Month == selectedMonth
                                                 && e.SalaryDate.Day == day)
                            ?.Amount)
            }).ToList();


            var model = new MonthlyReportCreateViewModel { Expenses = employeeViewModels };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonthlyReportCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                foreach (var employee in model.Expenses)
                {
                    foreach (var kvp in employee.DailyExpenses)
                    {
                        if (kvp.Value.HasValue)
                        {
                            var newEmp = new EmployeeSalary
                            {
                                EmployeeId = employee.CategoryId,
                                Amount = kvp.Value.Value,
                                SalaryDate = firstDayOfMonth.AddDays(kvp.Key - 1)
                            };

                            var alredyData = await _context.Salaries
                                .FirstOrDefaultAsync(x => x.SalaryDate == newEmp.SalaryDate
                                                        && x.EmployeeId == newEmp.EmployeeId);

                            if (alredyData == null)
                            {
                                _context.Salaries.Add(newEmp);
                            }
                            else
                            {
                                alredyData.Amount = newEmp.Amount;
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

            var employeeSalary = await _context.Salaries
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employeeSalary == null)
            {
                return NotFound();
            }

            return View(employeeSalary);
        }

        // POST: EmployeeSalaries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employeeSalary = await _context.Salaries.FindAsync(id);
            if (employeeSalary != null)
            {
                _context.Salaries.Remove(employeeSalary);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeSalaryExists(int id)
        {
            return _context.Salaries.Any(e => e.Id == id);
        }


        public IActionResult ExcelReport(int year, int month)
        {
            var salaries = _context.Salaries
                .Include(e => e.Employee)
                .Where(x => x.SalaryDate.Year == year && x.SalaryDate.Month == month)
                .OrderBy(x => x.EmployeeId)
                .ToList();
            var employees = _context.Employees
                .Where(x => ((x.HireDate.Year < year) || (x.HireDate.Year == year && x.HireDate.Month <= month)) && ((x.FireDate.Value.Year > year) ||
                    (x.FireDate.Value.Year == year && x.FireDate.Value.Month >= month) || !x.FireDate.HasValue))
                .Select(x => x.Name + " " + x.Surname).ToList();
            var report = new ExcelReportDto
            {
                Title = "Maaşlar",
                Headers = employees,
                Data = new List<DailyReportDto>()
            };
            foreach (var item in salaries)
            {
                var dailyReport = report.Data.FirstOrDefault(x => x.Title == item.Employee.Name + " " + item.Employee.Surname);
                if (dailyReport == null)
                {
                    dailyReport = new DailyReportDto
                    {
                        Title = item.Employee.Name + " " + item.Employee.Surname,
                        ValueofDay = new Dictionary<int, string>()
                    };
                    report.Data.Add(dailyReport);
                }
                if (dailyReport.ValueofDay.ContainsKey(item.SalaryDate.Day))
                {
                    dailyReport.ValueofDay[item.SalaryDate.Day] = (Convert.ToDecimal(dailyReport.ValueofDay[item.SalaryDate.Day]) + item.Amount).ToString();
                    continue;
                }
                dailyReport.ValueofDay.Add(item.SalaryDate.Day, item.Amount.ToString());
            }

            var fileContent = ExcelHelper.GenerateWorksheet(report, year, month);

            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "salaries.xlsx");
        }
    }
}

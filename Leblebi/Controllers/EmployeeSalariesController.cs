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
    public class EmployeeSalariesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeSalariesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EmployeeSalaries
        public async Task<IActionResult> Index()
        {
            var salaries = await _context.Salaries.Include(e => e.Employee)
                .Where(x => x.SalaryDate.Year == DateTime.Now.Year && x.SalaryDate.Month == DateTime.Now.Month)
                .OrderByDescending(e => e.EmployeeId)
                .ToListAsync();

            var employees = await _context.Employees
                .Include(x => x.Salaries.Where(x => x.SalaryDate.Year == DateTime.Now.Year && x.SalaryDate.Month == DateTime.Now.Month))
                .ToListAsync();

            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
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

        // GET: EmployeeSalaries/Details/5
        public async Task<IActionResult> Details(int? id)
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

        public async Task<IActionResult> Create()
        {
            var employees = await _context.Employees
                .Include(c => c.Salaries.OrderBy(x => x.SalaryDate))
                .ToListAsync();

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

            var employeeViewModels = employees.Select(e => new ExpenseCategoryViewModel
            {
                CategoryId = e.Id,
                CategoryName = e.Name + " " + e.Surname,
                DailyExpenses = Enumerable.Range(1, daysInMonth)
                    .ToDictionary(day => day, day =>
                        e.Salaries
                            .FirstOrDefault(e => e.SalaryDate.Year == currentYear
                                                 && e.SalaryDate.Month == currentMonth
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

                            _context.Salaries.Add(newEmp);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        //// GET: EmployeeSalaries/Create
        //public IActionResult Create()
        //{
        //    var employees = _context.Employees
        //        .Select(x => new
        //        {
        //            Id = x.Id,
        //            Name = x.Name + " " + x.Surname
        //        }).ToList();
        //    ViewData["EmployeeId"] = new SelectList(employees, "Id", "Name");
        //    return View();
        //}

        //// POST: EmployeeSalaries/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,EmployeeId,Amount,SalaryDate")] EmployeeSalary employeeSalary)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(employeeSalary);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    var employees = _context.Employees
        //        .Select(x => new
        //        {
        //            Id = x.Id,
        //            Name = x.Name + " " + x.Surname
        //        }).ToList();
        //    ViewData["EmployeeId"] = new SelectList(employees, "Id", "Name", employeeSalary.EmployeeId);
        //    return View(employeeSalary);
        //}

        // GET: EmployeeSalaries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeSalary = await _context.Salaries.FindAsync(id);
            if (employeeSalary == null)
            {
                return NotFound();
            }
            var employees = _context.Employees
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name + " " + x.Surname
                }).ToList();
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "Name", employeeSalary.EmployeeId);
            return View(employeeSalary);
        }

        // POST: EmployeeSalaries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmployeeId,Amount,SalaryDate")] EmployeeSalary employeeSalary)
        {
            if (id != employeeSalary.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeeSalary);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeSalaryExists(employeeSalary.Id))
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
            var employees = _context.Employees
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name + " " + x.Surname
                }).ToList();
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "Name", employeeSalary.EmployeeId);
            return View(employeeSalary);
        }

        // GET: EmployeeSalaries/Delete/5
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
    }
}

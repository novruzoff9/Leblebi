using System.Diagnostics;
using Leblebi.Data;
using Leblebi.Models;
using Leblebi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Leblebi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

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
            decimal totalIncome = _context.Incomes
                .Where(x=>x.IncomeDate.Year == year && x.IncomeDate.Month == month)
                .Sum(i => i.Amount);

            decimal totalExpense = _context.Expenses
                .Where(x => x.ExpenseDate.Year == year && x.ExpenseDate.Month == month)
                .Sum(i => i.Amount);

            decimal totalSalaries = _context.Salaries
                .Where(x => x.SalaryDate.Year == year && x.SalaryDate.Month == month)
                .Sum(i => i.Amount);

            decimal totalManagementExpenses = _context.ManagementExpenses
                .Where(x => x.ExpenseDate.Year == year && x.ExpenseDate.Month == month)
                .Sum(i => i.Amount);

            decimal cashIncome = _context.Incomes
                .Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == month && x.IncomeType == Enums.IncomeType.Cash) 
                .Sum(i => i.Amount);

            decimal onlineIncome = _context.Incomes
                .Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == month && x.IncomeType == Enums.IncomeType.PosTerminal)
                .Sum(i => i.Amount);

            decimal cashProfit = totalIncome - totalExpense - totalSalaries;

            var model = new HomeVM
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                TotalSalaries = totalSalaries,
                TotalManagementExpense = totalManagementExpenses,
                CashProfit = cashProfit,
                Months = new List<string> { "January", "February", "March", "April", "May", "June" },
                MonthlyIncomes = new List<decimal> { 1000, 2000, 3000, 4000, 5000, 6000 },
                MonthlyExpenses = new List<decimal> { 500, 1000, 1500, 2000, 2500, 3000 },
                EmployeeNames = new List<string>(),
                EmployeeSalaries = new List<decimal>()
                //EmployeeNames = new List<string> { "Alice", "Bob", "Charlie", "David" },
                //EmployeeSalaries = new List<decimal> { 1000, 2000, 3000, 4000 }
            };
            var employees = await _context.ExpenseCategories.Include(x=>x.Expenses).ToListAsync();
            foreach (var employee in employees)
            {
                model.EmployeeNames.Add(employee.Name);
                model.EmployeeSalaries.Add(employee.Expenses
                    .Where(x => x.ExpenseDate.Year == year && x.ExpenseDate.Month == month)
                    .Sum(i => i.Amount));
            }
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

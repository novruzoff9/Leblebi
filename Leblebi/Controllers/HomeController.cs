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

        public async Task<IActionResult> Index()
        {
            DateTime now = DateTime.Now;
            decimal totalIncome = _context.Incomes
                .Where(x=>x.IncomeDate.Year == now.Year && x.IncomeDate.Month == now.Month)
                .Sum(i => i.Amount);

            decimal totalExpense = _context.Expenses
                .Where(x => x.ExpenseDate.Year == now.Year && x.ExpenseDate.Month == now.Month)
                .Sum(i => i.Amount);

            decimal totalSalaries = _context.Salaries
                .Where(x => x.SalaryDate.Year == now.Year && x.SalaryDate.Month == now.Month)
                .Sum(i => i.Amount);

            decimal totalElectricityExpense = _context.Expenses
                .Where(x => x.ExpenseDate.Year == now.Year && x.ExpenseDate.Month == now.Month && x.ExpenseCategoryId == 6)
                .Sum(i => i.Amount);


            decimal totalWaterExpense = _context.Expenses
                .Where(x => x.ExpenseDate.Year == now.Year && x.ExpenseDate.Month == now.Month && x.ExpenseCategoryId == 7)
                .Sum(i => i.Amount);

            decimal totalGasExpense = _context.Expenses
                .Where(x => x.ExpenseDate.Year == now.Year && x.ExpenseDate.Month == now.Month && x.ExpenseCategoryId == 8)
                .Sum(i => i.Amount);
            MonthlyUtilitiesReports monthlyUtilities = new MonthlyUtilitiesReports
            {
                Water = totalWaterExpense,
                Electricity = totalElectricityExpense,
                Gas = totalGasExpense
            };
            var model = new HomeVM
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                TotalSalaries = totalSalaries,
                Months = new List<string> { "January", "February", "March", "April", "May", "June" },
                MonthlyIncomes = new List<decimal> { 1000, 2000, 3000, 4000, 5000, 6000 },
                MonthlyExpenses = new List<decimal> { 500, 1000, 1500, 2000, 2500, 3000 },
                EmployeeNames = new List<string>(),
                EmployeeSalaries = new List<decimal>(),
                MonthlyUtilities = monthlyUtilities
                //EmployeeNames = new List<string> { "Alice", "Bob", "Charlie", "David" },
                //EmployeeSalaries = new List<decimal> { 1000, 2000, 3000, 4000 }
            };
            var employees = await _context.Employees.Include(x=>x.Salaries).ToListAsync();
            foreach (var employee in employees)
            {
                model.EmployeeNames.Add(employee.Name);
                model.EmployeeSalaries.Add(employee.Salaries
                    .Where(x => x.SalaryDate.Year == now.Year && x.SalaryDate.Month == now.Month)
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

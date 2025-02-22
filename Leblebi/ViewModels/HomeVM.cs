using Leblebi.Models;

namespace Leblebi.ViewModels;

public class HomeVM
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal TotalSalaries { get; set; }
    public decimal TotalManagementExpense { get; set; }
    public decimal CashProfit { get; set; }
    public decimal OnlineProfit { get; set; }

    public List<string> Months { get; set; }
    public List<decimal> MonthlyIncomes { get; set; }
    public List<decimal> MonthlyExpenses { get; set; }

    public List<string> EmployeeNames { get; set; }
    public List<decimal> EmployeeSalaries { get; set; }
}

public class ExpenseReportViewModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public int? ParentCategoryId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ExpenseReportViewModel> Subcategories { get; set; } = new List<ExpenseReportViewModel>();
}

public class ExpensesViewModel
{
    public List<MonthlyReportViewModel> MonthlyExpenses { get; set; }
    public List<Expense> OtherExpenses { get; set; }
}

public class MonthlyReportViewModel
{
    public string Title { get; set; }
    public List<DailyReport> Reports { get; set; }
    public decimal TotalValue { get; set; }
    public decimal? SecondValue { get; set; }
}
public class DailyReport
{
    public DateOnly Date { get; set; }
    public decimal Value { get; set; }
    public string? Note { get; set; }
}

public class MonthlyReportCreateViewModel
{
    public List<ExpenseCategoryViewModel> Expenses { get; set; }
}

public class ExpenseCategoryViewModel
{
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Dictionary<int, decimal?> DailyExpenses { get; set; } = new();
}

public class IncomesOfType
{
    public string IncomeType { get; set; }
    public List<Income> Incomes { get; set; }
}

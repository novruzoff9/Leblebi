namespace Leblebi.ViewModels;

public class HomeVM
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal TotalSalaries { get; set; }

    public MonthlyUtilitiesReports MonthlyUtilities { get; set; }

    public List<string> Months { get; set; }
    public List<decimal> MonthlyIncomes { get; set; }
    public List<decimal> MonthlyExpenses { get; set; }

    public List<string> EmployeeNames { get; set; }
    public List<decimal> EmployeeSalaries { get; set; }
}

public class MonthlyUtilitiesReports
{
    public decimal Water { get; set; }
    public decimal Electricity { get; set; }
    public decimal Gas { get; set; }
}

public class ExpenseReportViewModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public int? ParentCategoryId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ExpenseReportViewModel> Subcategories { get; set; } = new List<ExpenseReportViewModel>();
}

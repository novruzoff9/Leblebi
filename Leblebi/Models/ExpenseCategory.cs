namespace Leblebi.Models;

public class ExpenseCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentCategoryId { get; set; }
    public ExpenseCategory? ParentCategory { get; set; }
    public List<ExpenseCategory>? expenseCategories { get; set; }
    public List<Expense>? Expenses { get; set; }
}

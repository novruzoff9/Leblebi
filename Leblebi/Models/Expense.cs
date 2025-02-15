namespace Leblebi.Models;

public class Expense
{
    public int Id { get; set; }
    public int ExpenseCategoryId { get; set; }
    public ExpenseCategory? ExpenseCategory { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Note { get; set; }
}

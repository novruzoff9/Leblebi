namespace Leblebi.Models;

public class ManagementExpense
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public int ManagementCategoryId { get; set; }
    public ManagementCategory? ManagementCategory { get; set; }
}
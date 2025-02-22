namespace Leblebi.Models;

public class ManagementCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<ManagementExpense>? Expenses { get; set; }
}

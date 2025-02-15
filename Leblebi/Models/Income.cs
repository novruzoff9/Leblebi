using Leblebi.Enums;

namespace Leblebi.Models;

public class Income
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public IncomeType IncomeType { get; set; }
    public DateTime IncomeDate { get; set; }
}
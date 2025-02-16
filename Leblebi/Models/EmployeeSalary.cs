using System.ComponentModel.DataAnnotations.Schema;

namespace Leblebi.Models;

public class EmployeeSalary
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public decimal Amount { get; set; }
    public DateTime SalaryDate { get; set; }
}

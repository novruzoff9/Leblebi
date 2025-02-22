namespace Leblebi.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public List<EmployeeSalary>? Salaries { get; set; }
    public decimal? Salary { get; set; }
    public DateOnly HireDate { get; set; }
    public DateOnly? FireDate { get; set; }
}

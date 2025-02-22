using Leblebi.Models;
using Microsoft.EntityFrameworkCore;

namespace Leblebi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeSalary> Salaries { get; set; }
    public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Income> Incomes { get; set; }
    public DbSet<ManagementCategory> ManagementCategories { get; set; }
    public DbSet<ManagementExpense> ManagementExpenses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExpenseCategory>()
            .HasMany(e => e.Expenses)
            .WithOne(e => e.ExpenseCategory)
            .HasForeignKey(e => e.ExpenseCategoryId);

        modelBuilder.Entity<ExpenseCategory>()
            .HasMany(e => e.expenseCategories)
            .WithOne(e => e.ParentCategory)
            .HasForeignKey(e => e.ParentCategoryId);

        modelBuilder.Entity<Employee>()
            .HasMany(e => e.Salaries)
            .WithOne(e => e.Employee)
            .HasForeignKey(e => e.EmployeeId);

        modelBuilder.Entity<ManagementCategory>()
            .HasMany(e => e.Expenses)
            .WithOne(e => e.ManagementCategory)
            .HasForeignKey(e => e.ManagementCategoryId);
    }
}

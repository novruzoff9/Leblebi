using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Leblebi.Data;
using Leblebi.Models;

namespace Leblebi.Controllers
{
    public class EmployeeSalariesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeSalariesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EmployeeSalaries
        public async Task<IActionResult> Index()
        {
            var salaries = await _context.Salaries.Include(e => e.Employee)
                .OrderByDescending(e => e.SalaryDate)
                .ToListAsync();
            return View(salaries);
        }

        // GET: EmployeeSalaries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeSalary = await _context.Salaries
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employeeSalary == null)
            {
                return NotFound();
            }

            return View(employeeSalary);
        }

        // GET: EmployeeSalaries/Create
        public IActionResult Create()
        {
            var employees = _context.Employees
                .Select(x=> new
                {
                    Id = x.Id,
                    Name = x.Name + " " + x.Surname
                }).ToList();
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "Name");
            return View();
        }

        // POST: EmployeeSalaries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmployeeId,Amount,SalaryDate")] EmployeeSalary employeeSalary)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employeeSalary);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var employees = _context.Employees
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name + " " + x.Surname
                }).ToList();
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "Name", employeeSalary.EmployeeId);
            return View(employeeSalary);
        }

        // GET: EmployeeSalaries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeSalary = await _context.Salaries.FindAsync(id);
            if (employeeSalary == null)
            {
                return NotFound();
            }
            var employees = _context.Employees
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name + " " + x.Surname
                }).ToList();
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "Name", employeeSalary.EmployeeId);
            return View(employeeSalary);
        }

        // POST: EmployeeSalaries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmployeeId,Amount,SalaryDate")] EmployeeSalary employeeSalary)
        {
            if (id != employeeSalary.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeeSalary);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeSalaryExists(employeeSalary.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            var employees = _context.Employees
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name + " " + x.Surname
                }).ToList();
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "Name", employeeSalary.EmployeeId);
            return View(employeeSalary);
        }

        // GET: EmployeeSalaries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeSalary = await _context.Salaries
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employeeSalary == null)
            {
                return NotFound();
            }

            return View(employeeSalary);
        }

        // POST: EmployeeSalaries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employeeSalary = await _context.Salaries.FindAsync(id);
            if (employeeSalary != null)
            {
                _context.Salaries.Remove(employeeSalary);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeSalaryExists(int id)
        {
            return _context.Salaries.Any(e => e.Id == id);
        }
    }
}

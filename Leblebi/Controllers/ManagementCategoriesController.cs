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
    public class ManagementCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagementCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ManagementCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.ManagementCategories.ToListAsync());
        }

        // GET: ManagementCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var managementCategories = await _context.ManagementCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (managementCategories == null)
            {
                return NotFound();
            }

            return View(managementCategories);
        }

        // GET: ManagementCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ManagementCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] ManagementCategory managementCategories)
        {
            if (ModelState.IsValid)
            {
                _context.Add(managementCategories);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(managementCategories);
        }

        // GET: ManagementCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var managementCategories = await _context.ManagementCategories.FindAsync(id);
            if (managementCategories == null)
            {
                return NotFound();
            }
            return View(managementCategories);
        }

        // POST: ManagementCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] ManagementCategory managementCategories)
        {
            if (id != managementCategories.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(managementCategories);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ManagementCategoriesExists(managementCategories.Id))
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
            return View(managementCategories);
        }

        // GET: ManagementCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var managementCategories = await _context.ManagementCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (managementCategories == null)
            {
                return NotFound();
            }

            return View(managementCategories);
        }

        // POST: ManagementCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var managementCategories = await _context.ManagementCategories.FindAsync(id);
            if (managementCategories != null)
            {
                _context.ManagementCategories.Remove(managementCategories);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ManagementCategoriesExists(int id)
        {
            return _context.ManagementCategories.Any(e => e.Id == id);
        }
    }
}

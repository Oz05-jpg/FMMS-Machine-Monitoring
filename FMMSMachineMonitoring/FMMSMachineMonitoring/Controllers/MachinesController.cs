
using FMMSMachineMonitoring.Data;
using FMMSMachineMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class MachinesController : Controller
{
    private readonly AppDbContext _context;

    public MachinesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: MACHINES
    public async Task<IActionResult> Index()
    {
        return View(await _context.Machines.ToListAsync());
    }

    // GET: MACHINES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var machine = await _context.Machines
            .FirstOrDefaultAsync(m => m.Id == id);
        if (machine == null)
        {
            return NotFound();
        }

        return View(machine);
    }

    // GET: MACHINES/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: MACHINES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Code,Name,Model,Location,InstallDate,Status")] Machine machine)
    {
        if (ModelState.IsValid)
        {
            //DbUpdateException
            try
            {
                _context.Add(machine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                // Log the exception (ex) if needed
                ModelState.AddModelError("Code", "รหัสเครื่องนี้มีอยู่แล้ว");
            }
        }
        return View(machine);
    }

    // GET: MACHINES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var machine = await _context.Machines.FindAsync(id);
        if (machine == null)
        {
            return NotFound();
        }
        return View(machine);
    }

    // POST: MACHINES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Code,Name,Model,Location,InstallDate,Status")] Machine machine)
    {
        if (id != machine.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(machine);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MachineExists(machine.Id))
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
        return View(machine);
    }

    // GET: MACHINES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var machine = await _context.Machines
            .FirstOrDefaultAsync(m => m.Id == id);
        if (machine == null)
        {
            return NotFound();
        }

        return View(machine);
    }

    // POST: MACHINES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var machine = await _context.Machines.FindAsync(id);
        if (machine != null)
        {
            _context.Machines.Remove(machine);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool MachineExists(int? id)
    {
        return _context.Machines.Any(e => e.Id == id);
    }
}

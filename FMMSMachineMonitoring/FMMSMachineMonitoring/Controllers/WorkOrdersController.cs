
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FMMSMachineMonitoring.Models;
using FMMSMachineMonitoring.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

public class WorkOrdersController : Controller
{
    private readonly AppDbContext _context;

    public WorkOrdersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: WORKORDERS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.WorkOrders.ToListAsync());
    }

    // GET: WORKORDERS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var workorder = await _context.WorkOrders
            .FirstOrDefaultAsync(m => m.Id == id);
        if (workorder == null)
        {
            return NotFound();
        }

        return View(workorder);
    }

    // GET: WORKORDERS/Create
    public IActionResult Create()
    {
        ViewData["MachineId"] = new SelectList(_context.Machines,"Id", "Code");
        return View();
    }

    // POST: WORKORDERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,MachineId,TechnicianId,Description,Urgency")] WorkOrder workorder)
    {
        if (ModelState.IsValid)
        {
            workorder.CreatedDate = DateTime.Now; //ส่วนนี้จะกำหนดวันที่สร้างงานซ่อมเป็นวันที่ปัจจุบัน
            _context.Add(workorder);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Code", workorder.MachineId); //ส่วนนี้จะใช้สำหรับแสดงรายการเครื่องจักรใน dropdown list
        return View(workorder);
    }

    // GET: WORKORDERS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var workorder = await _context.WorkOrders.FindAsync(id);
        if (workorder == null)
        {
            return NotFound();
        }
        return View(workorder);
    }

    // POST: WORKORDERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,MachineId,Machine,TechnicianId,Technician,Description,Urgency,CreatedDate,ClosedDate")] WorkOrder workorder)
    {
        if (id != workorder.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(workorder);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkOrderExists(workorder.Id))
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
        return View(workorder);
    }

    // GET: WORKORDERS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var workorder = await _context.WorkOrders
            .FirstOrDefaultAsync(m => m.Id == id);
        if (workorder == null)
        {
            return NotFound();
        }

        return View(workorder);
    }

    // POST: WORKORDERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var workorder = await _context.WorkOrders.FindAsync(id);
        if (workorder != null)
        {
            _context.WorkOrders.Remove(workorder);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool WorkOrderExists(int? id)
    {
        return _context.WorkOrders.Any(e => e.Id == id);
    }
}

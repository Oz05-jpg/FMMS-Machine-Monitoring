using FMMSMachineMonitoring.Data;
using FMMSMachineMonitoring.Models;
using FMMSMachineMonitoring.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class WorkOrdersController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public WorkOrdersController(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: WORKORDERS
    public async Task<IActionResult> Index()
    {
        return View(await _context.WorkOrders.ToListAsync());
    }

    // GET: WORKORDERS/Details/

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
    [Authorize(Roles = "Operator,Supervisor")]
    public IActionResult Create()
    {
        ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Code");
        return View();
    }

    // POST: WORKORDERS/Create
    [Authorize(Roles = "Operator,Supervisor")]
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

    // GET: WORKORDERS/Edit/
    [Authorize(Roles = "Technician,Supervisor")]
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

        var userId = _userManager.GetUserId(User);
        var technician = await _context.Technicians.FirstOrDefaultAsync(t => t.UserId == userId);
        if (User.IsInRole("Technician"))
        {
            if (technician == null || workorder.TechnicianId != technician.Id)//เลือกใช้ workorder เพราะมันมาจาก FindAsync ซึ่งเป็นใบงานที่มีอยู่ในฐานข้อมูล ไม่ใช่ใบงานที่ถูกส่งมาจากฟอร์ม
            {
                return Forbid(); // ถ้าไม่พบช่าง หรือ ใบงานไม่ใช่ของช่างคนนี้ ให้ปฏิเสธการเข้าถึง
            }
        }
        return View(workorder);
    }

    // POST: WORKORDERS/Edit/
    [Authorize(Roles = "Technician,Supervisor")]
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
            {   //ถ้าพบใบงานที่มีอยู่ในฐานข้อมูลและทำการอัปเดตข้อมูลใบงานนั้น
                var existingWorkOrder = await _context.WorkOrders.FindAsync(workorder.Id);
                if (existingWorkOrder == null)
                {
                    return NotFound();
                }

                //เช็คสิทธิ์ของผู้ใช้ที่กำลังเข้าสู่ระบบที่ส่งคำขอแก้ไขใบงานนี้
                var userId = _userManager.GetUserId(User);
                var technician = await _context.Technicians.FirstOrDefaultAsync(t => t.UserId == userId);
                if (User.IsInRole("Technician"))
                {
                    if (technician == null || existingWorkOrder.TechnicianId != technician.Id)//เลือกใช้ existingWorkOrder เพราะเราต้องการตรวจสอบใบงานที่มีอยู่ในฐานข้อมูล ไม่ใช่ใบงานที่ถูกส่งมาจากฟอร์ม
                    {
                        return Forbid(); // ถ้าไม่พบช่าง หรือ ใบงานไม่ใช่ของช่างคนนี้ ให้ปฏิเสธการเข้าถึง
                    }
                }

                //จำค่าเดิมไว้ก่อนลงค่าใหม่ - เพื่อใช้ตรวจสอบว่ามีการเปลี่ยนแปลงวันที่ปิดงานหรือไม่
                bool wasOpenBefore = existingWorkOrder.ClosedDate == null;

                //ถ้าเป็น Role Supervisor สามารถแก้ไขข้อมูล MachineId, TechnicianId และ Urgency ได้ แต่ถ้าเป็น Role Technician จะไม่สามารถแก้ไขข้อมูลเหล่านี้ได้
                if (!User.IsInRole("Technician"))
                {
                    existingWorkOrder.MachineId = workorder.MachineId;
                    existingWorkOrder.TechnicianId = workorder.TechnicianId;
                    existingWorkOrder.Urgency = workorder.Urgency;
                }

                //ช่างเทคนิคสามารถแก้ไขคำอธิบายและวันที่ปิดงานได้เท่านั้น แต่ไม่สามารถแก้ไขข้อมูลอื่นๆ ได้
                existingWorkOrder.Description = workorder.Description;
                existingWorkOrder.ClosedDate = workorder.ClosedDate;

                //เช็คว่านี่คือจังหวะปิดใบงาน PM จริงไหม
                if (wasOpenBefore && existingWorkOrder.ClosedDate != null && existingWorkOrder.Type == WorkOrderType.Preventive)
                {
                    //กำหนดวันที่ปิดงาน PM ให้กับตาราง MaintenanceSchedules ของเครื่องจักรนั้นๆ
                    var scheduledTask = await _context.MaintenanceSchedules.FirstOrDefaultAsync(st => st.MachineId == existingWorkOrder.MachineId && st.IsActive);
                    if (scheduledTask != null)
                    {
                        scheduledTask.LastCompletedDate = existingWorkOrder.ClosedDate;
                        scheduledTask.NextDueDate = scheduledTask.NextDueDate.AddDays(scheduledTask.IntervalDays); //คำนวณวันที่ PM ครั้งถัดไป
                        _context.Update(scheduledTask);
                    }
                }

                _context.Update(existingWorkOrder);
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

    // GET: WORKORDERS/Delete/
    [Authorize(Roles = "Supervisor")]
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
    [Authorize(Roles = "Supervisor")]
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

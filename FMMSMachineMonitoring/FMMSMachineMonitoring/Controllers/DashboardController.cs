using FMMSMachineMonitoring.Data;
using FMMSMachineMonitoring.Models;
using FMMSMachineMonitoring.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FMMSMachineMonitoring.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            //ดึงข้อมูลเครื่องจักรทั้งหมดจากฐานข้อมูล
            var machines = await _context.Machines
                .AsNoTracking()
                .ToListAsync();

            //ดึงข้อมูลจำนวน Work Order ที่ยังไม่ปิด (ClosedDate == null) และจัดกลุ่มตาม Urgency
            var openWorkOrderCounts = await _context.WorkOrders
                .Where(wo => wo.ClosedDate == null)
                .GroupBy(wo => wo.Urgency)
                .Select(g => new
                {
                    Urgency = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Urgency, x => x.Count);

            // ดึงข้อมูลใบงานซ่อม (Corrective) ที่ปิดงานแล้ว (ClosedDate != null)
            var closedRepairs = await _context.WorkOrders
                .Where(wo => wo.Type == WorkOrderType.Corrective && wo.ClosedDate != null)
                .ToListAsync();

            var mttrByMachine = closedRepairs
                .GroupBy(wo => wo.MachineId)
                .Select(g => new MachineMttr
                {
                    MachineCode = machines.FirstOrDefault(m => m.Id == g.Key)?.Code ?? "เครื่องที่ถูกลบ",
                    MttrHours = g.Average(wo => (wo.ClosedDate.Value - wo.CreatedDate).TotalHours)
                })
                .ToList();


            //สร้าง ViewModel สำหรับส่งข้อมูลไปยัง View
            var model = new Models.DashboardViewModel
            {
                Machines = machines,
                OpenWorkOrderCounts = openWorkOrderCounts,
                MttrByMachine = mttrByMachine

            };
            return View(model);
        }
    }
}

using FMMSMachineMonitoring.Data;
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

            //สร้าง ViewModel สำหรับส่งข้อมูลไปยัง View
            var model = new Models.DashboardViewModel
            {
                Machines = machines,
                OpenWorkOrderCounts = openWorkOrderCounts
            };
            return View(model);
        }
    }
}

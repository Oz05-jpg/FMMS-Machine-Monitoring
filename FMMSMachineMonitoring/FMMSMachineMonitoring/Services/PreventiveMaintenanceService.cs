using FMMSMachineMonitoring.Data;
using FMMSMachineMonitoring.Models;
using FMMSMachineMonitoring.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FMMSMachineMonitoring.Services
{
    public class PreventiveMaintenanceService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<PreventiveMaintenanceService> _logger;


        public PreventiveMaintenanceService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<PreventiveMaintenanceService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromHours(1)); //Run every hour
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    //1. Create a new scope to get the DbContext
                    using var scope = _serviceScopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    //2. Get all schedules that are due for maintenance in 7 days
                    var cutoffDate = DateTime.UtcNow.AddDays(7);
                    var schedules = await db.MaintenanceSchedules
                        .Where(s => s.IsActive && s.NextDueDate <= cutoffDate)
                        .AsNoTracking()
                        .ToListAsync(stoppingToken);

                    //3. ทำทีละกำหนดการ
                    foreach (var schedule in schedules)
                    {
                        //เช็คว่าเครื่องนี้มีใบงาน PM ที่ยังไม่ได้ปิดอยู่แล้วหรือยัง
                        var hasOpenPmWorkOrder = await db.WorkOrders
                            .AnyAsync(wo => wo.MachineId == schedule.MachineId
                            && wo.Type == WorkOrderType.Preventive && wo.ClosedDate == null, stoppingToken);

                        //ถ้าไม่มีใบงาน PM ที่ยังเปิดอยู่ ให้สร้างใบงานใหม่
                        if (!hasOpenPmWorkOrder)
                        {
                            var workOrder = new WorkOrder
                            {
                                MachineId = schedule.MachineId,
                                Type = WorkOrderType.Preventive,
                                CreatedDate = DateTime.UtcNow,
                                Description = "ใบงาน PM ตามกำหนดการอัตโนมัติ",

                            };
                            db.WorkOrders.Add(workOrder);
                            await db.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"Created PM Work Order for Machine ID {schedule.MachineId} based on schedule ID {schedule.Id}");
                        }
                    }
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    //ตอนแอปกำลังหยุดทำงาน ไม่ต้องทำอะไร
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "เกิดข้อผิดพลาดระหว่างสร้างใบงาน PM อัตโนมัติ");

                }
            }
        }

    }
}

using FMMSMachineMonitoring.Models;
using Microsoft.EntityFrameworkCore;

namespace FMMSMachineMonitoring.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //ทุกครั้งที่มีการเปลี่ยนแปลงใน model จะต้องทำการ update database ด้วยคำสั่ง dotnet ef migrations add <ชื่อ migration> และ dotnet ef database update
        public DbSet<Machine> Machines { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<SensorReading> SensorReadings { get; set; }
        public DbSet<Alert> Alerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Machine>()
                .HasIndex(m => m.Code).IsUnique();
        }


    }


}

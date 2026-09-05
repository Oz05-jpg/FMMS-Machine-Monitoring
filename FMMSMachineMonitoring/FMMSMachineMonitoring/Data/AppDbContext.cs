using FMMSMachineMonitoring.Models;
using Microsoft.EntityFrameworkCore;

namespace FMMSMachineMonitoring.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Machine> Machines { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Machine>()
                .HasIndex(m => m.Code).IsUnique();
        }


    }


}

using Microsoft.AspNetCore.Identity;

namespace FMMSMachineMonitoring.Models
{
    public class Technician
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Specialization { get; set; }
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }
    }
}

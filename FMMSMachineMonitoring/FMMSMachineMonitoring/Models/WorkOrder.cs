using FMMSMachineMonitoring.Models.Enums;

namespace FMMSMachineMonitoring.Models
{
    public class WorkOrder
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public Machine? Machine { get; set; }  // ? FK มากจาก MachineId เป็น optional
        public int? TechnicianId { get; set; }
        public Technician? Technician { get; set; } // ? FK มากจาก TechnicianId เป็น optional
        public required string Description { get; set; }

        public UrgencyStatus Urgency { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ClosedDate { get; set; }

    }
}

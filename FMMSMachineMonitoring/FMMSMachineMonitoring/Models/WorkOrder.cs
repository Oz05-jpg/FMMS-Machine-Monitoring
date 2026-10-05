using FMMSMachineMonitoring.Models.Enums;

namespace FMMSMachineMonitoring.Models
{
    public class WorkOrder
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public Machine? Machine { get; set; }  //Machine? is a nullable reference type, allowing it to be null
        public int? AlertId { get; set; }
        public Alert? Alert { get; set; } // Alert? is a nullable reference type, allowing it to be null
        public int? TechnicianId { get; set; }
        public Technician? Technician { get; set; } // Technician? is a nullable reference type, allowing it to be null
        public required string Description { get; set; }

        public UrgencyStatus Urgency { get; set; }

        public WorkOrderType Type { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ClosedDate { get; set; }

    }
}

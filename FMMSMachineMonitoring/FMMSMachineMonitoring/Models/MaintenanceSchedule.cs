namespace FMMSMachineMonitoring.Models
{
    public class MaintenanceSchedule
    {
        public int Id { get; set; }
        public Machine? Machine { get; set; }  // Machine? is a nullable reference type, allowing it to be null
        public int MachineId { get; set; }
        public int IntervalDays { get; set; }

        public DateTime NextDueDate { get; set; }

        //เสริม
        public bool IsActive { get; set; } = true; // Default to true, indicating the schedule is active
        public DateTime? LastCompletedDate { get; set; }

    }
}

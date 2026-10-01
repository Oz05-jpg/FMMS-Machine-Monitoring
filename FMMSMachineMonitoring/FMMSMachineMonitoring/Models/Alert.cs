using FMMSMachineMonitoring.Models.Enums;

namespace FMMSMachineMonitoring.Models
{
    public class Alert
    {
        public int Id { get; set; }

        public Machine? Machine { get; set; }  // Machine? is a nullable reference type, allowing it to be null
        public int MachineId { get; set; }

        public SensorChannel Channel { get; set; }

        public double Value { get; set; }

        public double Threshold { get; set; }  // Nullable double for threshold, allowing it to be null

        public AlertSeverity Severity { get; set; }

        public DateTime DetectedAt { get; set; }
    }
}

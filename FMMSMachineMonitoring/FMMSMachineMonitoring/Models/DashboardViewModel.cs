using FMMSMachineMonitoring.Models.Enums;

namespace FMMSMachineMonitoring.Models
{
    public class DashboardViewModel
    {
        public required List<Machine> Machines { get; set; }

        public Dictionary<UrgencyStatus, int> OpenWorkOrderCounts { get; set; } = new();
    }
}

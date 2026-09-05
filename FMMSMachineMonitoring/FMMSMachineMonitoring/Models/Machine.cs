namespace FMMSMachineMonitoring.Models
{
    public class Machine
    {

        public int Id { get; set; }
        public required string Code
        { get; set; }
        public required string Name { get; set; }
        public required string Model { get; set; }
        public required string Location { get; set; }

        public DateTime InstallDate { get; set; }

        public MachineStatus Status { get; set; }

    }
}
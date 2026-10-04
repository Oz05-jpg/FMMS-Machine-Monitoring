using FMMSMachineMonitoring.Data;
using FMMSMachineMonitoring.Models;
using FMMSMachineMonitoring.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FMMSMachineMonitoring.Services
{
    public class SensorSimulationService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<SensorSimulationService> _logger;
        private readonly Dictionary<string, SensorChannel[]> MachineChannels = new()
        {
            { "CP-001", new[] { SensorChannel.Temperature, SensorChannel.Pressure, SensorChannel.Vibration } },
            { "EX-002", new[] { SensorChannel.HeaterZoneTemp, SensorChannel.MotorCurrent, SensorChannel.ScrewSpeed } },
            { "BM-001", new[] { SensorChannel.BearingVibration, SensorChannel.MotorTemp, SensorChannel.PowerConsumption } }
        };
        private double GenerateRandomValue(SensorChannel channel)
        {
            double normal = channel switch
            {
                SensorChannel.Temperature => 175,
                SensorChannel.Pressure => 200,
                SensorChannel.Vibration => 2.5,
                SensorChannel.HeaterZoneTemp => 165,
                SensorChannel.MotorCurrent => 42,
                SensorChannel.ScrewSpeed => 1450,
                SensorChannel.BearingVibration => 3.1,
                SensorChannel.MotorTemp => 72,
                SensorChannel.PowerConsumption => 18.4,
                _ => throw new ArgumentOutOfRangeException(nameof(channel), $"Unexpected channel value: {channel}"),
            };
            var variance = _random.NextDouble() * 0.3 - 0.15; // -0.15 ถึง +0.15
            var result = normal * (1 + variance);
            return result;
        }

        private (double Warning, double Critical, bool IsHighDirection) GetThresholds(SensorChannel channel)
        {
            return channel switch
            {
                SensorChannel.Temperature => (195, 200, true),
                SensorChannel.Pressure => (180, 160, false),
                SensorChannel.Vibration => (3.75, 5.0, true),
                SensorChannel.HeaterZoneTemp => (180, 200, true),
                SensorChannel.MotorCurrent => (46, 50, true),
                SensorChannel.ScrewSpeed => (1300, 1150, false),
                SensorChannel.BearingVibration => (3.4, 3.7, true),
                SensorChannel.MotorTemp => (80, 85, true),
                SensorChannel.PowerConsumption => (20, 22, true),
                _ => throw new ArgumentOutOfRangeException(nameof(channel), $"Unexpected channel value: {channel}"),
            };
        }
        private readonly Random _random = new Random();

        private readonly Dictionary<(int, SensorChannel), int> _values = new();
        private readonly Dictionary<(int, SensorChannel), int> _normalCounts = new(); //นับค่าปกติ
        private readonly Dictionary<(int, SensorChannel), int> _warningAlertDurations = new(); //นับค่า warning

        public SensorSimulationService(IServiceScopeFactory serviceScopeFactory, ILogger<SensorSimulationService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var machines = await db.Machines.ToListAsync();
                    //ลูปผ่านเครื่องจักรทั้งหมดและสร้าง SensorReading สำหรับแต่ละ Channel
                    foreach (var machine in machines)
                    {

                        if (MachineChannels.TryGetValue(machine.Code, out var channels))
                        {
                            foreach (var channel in channels)
                            {
                                //SensorReading ใหม่ถูกสร้างขึ้นและบันทึกลงในฐานข้อมูล
                                var reading = new SensorReading
                                {
                                    MachineId = machine.Id,
                                    Channel = channel,
                                    Value = GenerateRandomValue(channel),
                                    DateTime = DateTime.UtcNow
                                };
                                db.SensorReadings.Add(reading);


                                var activeAlert = await db.Alerts.FirstOrDefaultAsync(a =>
                                    a.MachineId == machine.Id &&
                                    a.Channel == channel &&
                                    a.ClearedAt == null);

                                //Alerting logic based on thresholds
                                var (warning, critical, isHighDirection) = GetThresholds(channel);

                                //เงื่อนไข เกิน warning เป็น boolean และ เกิน critical เป็น boolean เช่นกัน
                                bool isExceedingWarning = isHighDirection ? reading.Value > warning : reading.Value < warning;

                                if (isExceedingWarning)
                                {
                                    //Update the count of readings for this machine and channel
                                    _values.TryGetValue((machine.Id, channel), out int currentValue);
                                    int newValue = currentValue + 1;
                                    _values[(machine.Id, channel)] = newValue;
                                    _normalCounts[(machine.Id, channel)] = 0;

                                    bool isCritical = isHighDirection ? reading.Value > critical : reading.Value < critical;
                                    var severity = isCritical ? AlertSeverity.Critical : AlertSeverity.Warning;
                                    var threshold = isCritical ? critical : warning;

                                    if (activeAlert != null)
                                    {
                                        var previousSeverity = activeAlert.Severity;//เก็บค่า severity ก่อนหน้า
                                        activeAlert.Value = reading.Value;
                                        activeAlert.Threshold = threshold;
                                        activeAlert.Severity = severity;

                                        if (severity != AlertSeverity.Warning)
                                        {
                                            _warningAlertDurations[(machine.Id, channel)] = 0; //รีเซ็ตค่า warning alert duration ถ้า severity ไม่ใช่ Warning

                                        }


                                        else
                                        {
                                            //FR-AL-050 นับรอบที่ Warning ค้างอยู่
                                            _warningAlertDurations.TryGetValue((machine.Id, channel), out int warningDuration);
                                            _warningAlertDurations[(machine.Id, channel)] = warningDuration + 1; //เพิ่มรอบที่ Warning ค้างอยู่

                                            //FR-AL-070: สร้าง WO เมื่อ severity เปลี่ยนจาก Warning เป็น Critical
                                            if (severity == AlertSeverity.Critical && previousSeverity != AlertSeverity.Critical)
                                            {
                                                bool hasExistingWorkOrder = await db.WorkOrders.AnyAsync(wo => wo.AlertId == activeAlert.Id && wo.ClosedDate == null);
                                                //ถ้า severity เปลี่ยนจาก Warning เป็น Critical ให้สร้าง WorkOrder ใหม่
                                                if (!hasExistingWorkOrder)
                                                {
                                                    var workOrderForCritical = new WorkOrder
                                                    {
                                                        MachineId = machine.Id,
                                                        Alert = activeAlert,
                                                        Description = $"Sensor {channel} exceeded critical threshold. Measured: {reading.Value:F2}, Threshold: {critical:F2}",
                                                        Urgency = UrgencyStatus.Urgent,
                                                        CreatedDate = DateTime.UtcNow
                                                    };
                                                    db.WorkOrders.Add(workOrderForCritical);
                                                }
                                            }

                                        }
                                    }
                                    else if (newValue >= 3) //สร้าง Alert ถ้าเกิน warning >= 3 ครั้ง
                                    {
                                        var alert = new Alert
                                        {
                                            MachineId = machine.Id,
                                            Channel = channel,
                                            Value = reading.Value,
                                            Threshold = threshold,
                                            Severity = severity,
                                            DetectedAt = DateTime.UtcNow
                                        };
                                        db.Alerts.Add(alert);

                                        //FR-AL-050 นับรอบที่ Warning ค้างอยู่
                                        if (severity == AlertSeverity.Warning)
                                        {
                                            _warningAlertDurations[(machine.Id, channel)] = 1; //เริ่มนับรอบ Warning
                                        }
                                        else
                                        {
                                            _warningAlertDurations[(machine.Id, channel)] = 0; //ไม่ใช่ Warning → รีเซ็ต
                                        }

                                        //Create a WorkOrder if the severity is Critical
                                        if (severity == AlertSeverity.Critical)
                                        {
                                            var workOrder = new WorkOrder
                                            {
                                                MachineId = machine.Id,
                                                Alert = alert,
                                                Description = $"Sensor {channel} exceeded critical threshold. Measured: {reading.Value:F2}, Threshold: {critical:F2}",
                                                Urgency = UrgencyStatus.Urgent,
                                                CreatedDate = DateTime.UtcNow
                                            };
                                            db.WorkOrders.Add(workOrder);
                                        }
                                    }
                                }
                                else
                                {
                                    _values[(machine.Id, channel)] = 0; // Reset the count if the reading is within normal range
                                    _normalCounts.TryGetValue((machine.Id, channel), out int normalCount);
                                    int newNormalCount = normalCount + 1;
                                    _normalCounts[(machine.Id, channel)] = newNormalCount;

                                    //FR-AL-060 (Clear): ถ้าค่าปกติติดกัน >= 5 รอบและมี Acive Alert อยู่ -> Clear Alert
                                    if (newNormalCount >= 5 && activeAlert != null)
                                    {
                                        activeAlert.ClearedAt = DateTime.UtcNow;
                                        _normalCounts[(machine.Id, channel)] = 0; // Reset the normal count after clearing the alert
                                        _warningAlertDurations[(machine.Id, channel)] = 0; // Reset the warning duration count after clearing the alert
                                    }
                                }
                                // FR-AL-065: นับและเช็ค Warning duration (ทำงานทั้งรอบเกินและรอบปกติ)
                                if (activeAlert != null && activeAlert.Severity == AlertSeverity.Warning && activeAlert.ClearedAt == null)
                                {
                                    // เพิ่มตัวนับเฉพาะรอบปกติ (รอบเกินเพิ่มไปแล้วในบล็อก if (isExceedingWarning))
                                    if (!isExceedingWarning)
                                    {
                                        _warningAlertDurations.TryGetValue((machine.Id, channel), out int warningDuration);
                                        _warningAlertDurations[(machine.Id, channel)] = warningDuration + 1;
                                    }

                                    // เช็คครบ 5 และสร้าง WO
                                    if (_warningAlertDurations[(machine.Id, channel)] >= 5)
                                    {
                                        bool hasWarningWorkOrder = await db.WorkOrders.AnyAsync(wo => wo.AlertId == activeAlert.Id && wo.ClosedDate == null);
                                        if (!hasWarningWorkOrder)
                                        {
                                            var urgency = machine.Code switch
                                            {
                                                "CP-001" => UrgencyStatus.High,
                                                "EX-002" => UrgencyStatus.Medium,
                                                "BM-001" => UrgencyStatus.Medium,
                                                _ => UrgencyStatus.Medium
                                            };

                                            var workOrderForWarning = new WorkOrder
                                            {
                                                MachineId = machine.Id,
                                                Alert = activeAlert,
                                                Description = $"Sensor {channel} has been in WARNING state for 5 consecutive readings. Current value: {reading.Value:F2}, Threshold: {warning:F2}",
                                                Urgency = urgency,
                                                CreatedDate = DateTime.UtcNow
                                            };
                                            db.WorkOrders.Add(workOrderForWarning);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"No channels defined for machine code: {machine.Code}");
                        }
                    }
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while simulating sensor readings.");
                }
            }
        }
    }
}

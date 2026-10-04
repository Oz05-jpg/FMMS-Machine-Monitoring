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

                                    //Reset the normal count if the reading exceeds the warning threshold
                                    _normalCounts[(machine.Id, channel)] = 0;
                                    bool isCritical = isHighDirection ? reading.Value > critical : reading.Value < critical;
                                    var severity = isCritical ? AlertSeverity.Critical : AlertSeverity.Warning;
                                    var threshold = isCritical ? critical : warning;

                                    if (activeAlert != null)
                                    {
                                        activeAlert.Value = reading.Value;
                                        activeAlert.Threshold = threshold;
                                        activeAlert.Severity = severity;
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

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
        private readonly Random _random = new Random();

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
                                var reading = new SensorReading
                                {
                                    MachineId = machine.Id,
                                    Channel = channel,
                                    Value = GenerateRandomValue(channel),
                                    DateTime = DateTime.UtcNow
                                };
                                db.SensorReadings.Add(reading);
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

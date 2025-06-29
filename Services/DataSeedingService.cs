using DataMonitoringSys.Data;
using DataMonitoringSys.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataMonitoringSys.Services
{
    public class DataSeedingService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DataSeedingService> _logger;

        public DataSeedingService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<DataSeedingService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SeedMockDataAsync()
        {
            try
            {
                // Check if we already have data
                var existingDataCount = await _context.DataPoints.CountAsync();
                if (existingDataCount > 0)
                {
                    _logger.LogInformation($"Mock data already exists. Found {existingDataCount} data points.");
                    return;
                }

                _logger.LogInformation("Starting to seed mock data...");

                // Get admin user
                var adminUser = await _userManager.FindByEmailAsync("admin@engineering.com");
                if (adminUser == null)
                {
                    _logger.LogError("Admin user not found. Cannot seed data.");
                    return;
                }

                // Get engineering units
                var units = await _context.EngineeringUnits.ToListAsync();
                if (!units.Any())
                {
                    _logger.LogError("No engineering units found. Cannot seed data.");
                    return;
                }

                var random = new Random();
                var dataPoints = new List<DataPoint>();

                // Generate data for the last 30 days
                var startDate = DateTime.Now.AddDays(-30);
                
                for (int day = 0; day < 30; day++)
                {
                    var currentDate = startDate.AddDays(day);
                    
                    // Generate multiple readings per day for each unit
                    foreach (var unit in units)
                    {
                        // Generate 3-8 readings per day per unit
                        var readingsPerDay = random.Next(3, 9);
                        
                        for (int reading = 0; reading < readingsPerDay; reading++)
                        {
                            var timestamp = currentDate.AddHours(random.Next(0, 24))
                                                     .AddMinutes(random.Next(0, 60));

                            // Generate different types of parameters based on unit
                            var parameters = GetParametersForUnit(unit.Code);
                            
                            foreach (var param in parameters)
                            {
                                var dataPoint = new DataPoint
                                {
                                    ParameterName = param.Name,
                                    Value = GenerateRealisticValue(param.Name, random),
                                    Unit = param.Unit,
                                    MinValue = param.MinValue,
                                    MaxValue = param.MaxValue,
                                    Notes = GenerateRandomNotes(param.Name, random),
                                    Timestamp = timestamp,
                                    UserId = adminUser.Id,
                                    EngineeringUnitId = unit.Id,
                                    IsValid = random.NextDouble() > 0.05, // 95% valid data
                                    ValidationMessage = random.NextDouble() > 0.05 ? null : "Value outside normal range"
                                };

                                dataPoints.Add(dataPoint);
                            }
                        }
                    }
                }

                // Add all data points to database
                await _context.DataPoints.AddRangeAsync(dataPoints);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully seeded {dataPoints.Count} mock data points!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding mock data");
                throw;
            }
        }

        private List<ParameterTemplate> GetParametersForUnit(string unitCode)
        {
            return unitCode switch
            {
                "PROC-A" => new List<ParameterTemplate>
                {
                    new("Temperature", "°C", 20, 80),
                    new("Pressure", "bar", 1, 10),
                    new("Flow Rate", "L/min", 50, 500),
                    new("pH Level", "pH", 6.5m, 8.5m),
                    new("Conductivity", "µS/cm", 100, 1000)
                },
                "QC-LAB" => new List<ParameterTemplate>
                {
                    new("Viscosity", "cP", 1, 100),
                    new("Density", "g/cm³", 0.8m, 1.2m),
                    new("Moisture Content", "%", 0, 15),
                    new("Purity", "%", 95, 99.9m),
                    new("Particle Size", "µm", 10, 500)
                },
                "MAINT" => new List<ParameterTemplate>
                {
                    new("Vibration", "mm/s", 0, 10),
                    new("Motor Current", "A", 5, 50),
                    new("Bearing Temperature", "°C", 30, 70),
                    new("Oil Pressure", "bar", 2, 8),
                    new("Runtime Hours", "hrs", 0, 8760)
                },
                "SAFE-ENV" => new List<ParameterTemplate>
                {
                    new("CO2 Level", "ppm", 300, 1000),
                    new("Noise Level", "dB", 40, 85),
                    new("Ambient Temperature", "°C", 15, 35),
                    new("Humidity", "%", 30, 70),
                    new("Air Quality Index", "AQI", 0, 300)
                },
                _ => new List<ParameterTemplate>
                {
                    new("Generic Parameter", "units", 0, 100)
                }
            };
        }

        private decimal GenerateRealisticValue(string parameterName, Random random)
        {
            // Add some realistic variation and trends
            var baseValue = parameterName switch
            {
                "Temperature" => 45 + (decimal)(random.NextDouble() * 20 - 10), // 35-55°C
                "Pressure" => 5 + (decimal)(random.NextDouble() * 3 - 1.5), // 3.5-6.5 bar
                "Flow Rate" => 250 + (decimal)(random.NextDouble() * 100 - 50), // 200-300 L/min
                "pH Level" => 7.2m + (decimal)(random.NextDouble() * 0.6 - 0.3), // 6.9-7.5
                "Conductivity" => 500 + (decimal)(random.NextDouble() * 200 - 100), // 400-600
                "Viscosity" => 25 + (decimal)(random.NextDouble() * 20 - 10), // 15-35 cP
                "Density" => 1.0m + (decimal)(random.NextDouble() * 0.1 - 0.05), // 0.95-1.05
                "Moisture Content" => 5 + (decimal)(random.NextDouble() * 4 - 2), // 3-7%
                "Purity" => 98 + (decimal)(random.NextDouble() * 1.5), // 98-99.5%
                "Particle Size" => 100 + (decimal)(random.NextDouble() * 100 - 50), // 50-150 µm
                "Vibration" => 2 + (decimal)(random.NextDouble() * 3 - 1.5), // 0.5-3.5 mm/s
                "Motor Current" => 25 + (decimal)(random.NextDouble() * 10 - 5), // 20-30 A
                "Bearing Temperature" => 50 + (decimal)(random.NextDouble() * 10 - 5), // 45-55°C
                "Oil Pressure" => 5 + (decimal)(random.NextDouble() * 2 - 1), // 4-6 bar
                "Runtime Hours" => (decimal)(random.NextDouble() * 8760), // 0-8760 hrs
                "CO2 Level" => 400 + (decimal)(random.NextDouble() * 200 - 100), // 300-500 ppm
                "Noise Level" => 65 + (decimal)(random.NextDouble() * 10 - 5), // 60-70 dB
                "Ambient Temperature" => 22 + (decimal)(random.NextDouble() * 6 - 3), // 19-25°C
                "Humidity" => 50 + (decimal)(random.NextDouble() * 20 - 10), // 40-60%
                "Air Quality Index" => 50 + (decimal)(random.NextDouble() * 100 - 50), // 0-100 AQI
                _ => (decimal)(random.NextDouble() * 100)
            };

            return Math.Round(baseValue, 2);
        }

        private string? GenerateRandomNotes(string parameterName, Random random)
        {
            var noteOptions = new[]
            {
                "Normal operation",
                "Routine measurement",
                "Calibration check completed",
                "Equipment running smoothly",
                "Within acceptable range",
                "Scheduled maintenance performed",
                "Quality control sample",
                "Baseline measurement",
                "Post-maintenance reading",
                "Shift change reading"
            };

            // 70% chance of having notes
            if (random.NextDouble() > 0.3)
            {
                return noteOptions[random.Next(noteOptions.Length)];
            }

            return null;
        }

        private record ParameterTemplate(string Name, string Unit, decimal MinValue, decimal MaxValue);
    }
}

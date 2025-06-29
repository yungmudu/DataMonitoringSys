using Microsoft.EntityFrameworkCore;
using DataMonitoringSys.Data;
using DataMonitoringSys.Models;
using System.Globalization;
using System.Text;
using CsvHelper;
using ClosedXML.Excel;

namespace DataMonitoringSys.Services;

public class DataPointService : IDataPointService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataPointService> _logger;

    public DataPointService(ApplicationDbContext context, ILogger<DataPointService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<DataPoint>> GetDataPointsAsync(int? unitId = null, DateTime? startDate = null, DateTime? endDate = null, string? parameterName = null)
    {
        var query = _context.DataPoints
            .Include(dp => dp.User)
            .Include(dp => dp.EngineeringUnit)
            .AsQueryable();

        if (unitId.HasValue)
            query = query.Where(dp => dp.EngineeringUnitId == unitId.Value);

        if (startDate.HasValue)
            query = query.Where(dp => dp.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(dp => dp.Timestamp <= endDate.Value);

        if (!string.IsNullOrEmpty(parameterName))
            query = query.Where(dp => dp.ParameterName.Contains(parameterName));

        return await query.OrderByDescending(dp => dp.Timestamp).ToListAsync();
    }

    public async Task<DataPoint> GetDataPointByIdAsync(int id)
    {
        var dataPoint = await _context.DataPoints
            .Include(dp => dp.User)
            .Include(dp => dp.EngineeringUnit)
            .FirstOrDefaultAsync(dp => dp.Id == id);

        return dataPoint ?? throw new ArgumentException($"DataPoint with ID {id} not found.");
    }

    public async Task<DataPoint> CreateDataPointAsync(DataPoint dataPoint)
    {
        // Validate the data point
        await ValidateDataPointAsync(dataPoint);

        _context.DataPoints.Add(dataPoint);
        await _context.SaveChangesAsync();

        return await GetDataPointByIdAsync(dataPoint.Id);
    }

    public async Task<DataPoint> UpdateDataPointAsync(DataPoint dataPoint)
    {
        var existingDataPoint = await _context.DataPoints.FindAsync(dataPoint.Id);
        if (existingDataPoint == null)
            throw new ArgumentException($"DataPoint with ID {dataPoint.Id} not found.");

        // Update properties
        existingDataPoint.ParameterName = dataPoint.ParameterName;
        existingDataPoint.Value = dataPoint.Value;
        existingDataPoint.Unit = dataPoint.Unit;
        existingDataPoint.Notes = dataPoint.Notes;
        existingDataPoint.MinValue = dataPoint.MinValue;
        existingDataPoint.MaxValue = dataPoint.MaxValue;

        // Re-validate
        await ValidateDataPointAsync(existingDataPoint);

        await _context.SaveChangesAsync();
        return await GetDataPointByIdAsync(dataPoint.Id);
    }

    public async Task<bool> DeleteDataPointAsync(int id)
    {
        var dataPoint = await _context.DataPoints.FindAsync(id);
        if (dataPoint == null)
            return false;

        _context.DataPoints.Remove(dataPoint);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<DataPoint>> GetRecentDataPointsAsync(int count = 50, int? unitId = null)
    {
        var query = _context.DataPoints
            .Include(dp => dp.User)
            .Include(dp => dp.EngineeringUnit)
            .AsQueryable();

        if (unitId.HasValue)
            query = query.Where(dp => dp.EngineeringUnitId == unitId.Value);

        return await query
            .OrderByDescending(dp => dp.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetParameterNamesAsync(int? unitId = null)
    {
        var query = _context.DataPoints.AsQueryable();

        if (unitId.HasValue)
            query = query.Where(dp => dp.EngineeringUnitId == unitId.Value);

        return await query
            .Select(dp => dp.ParameterName)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync();
    }

    public Task<bool> ValidateDataPointAsync(DataPoint dataPoint)
    {
        var validationErrors = new List<string>();

        // Check if value is within specified range
        if (dataPoint.MinValue.HasValue && dataPoint.Value < dataPoint.MinValue.Value)
        {
            validationErrors.Add($"Value {dataPoint.Value} is below minimum allowed value {dataPoint.MinValue.Value}");
        }

        if (dataPoint.MaxValue.HasValue && dataPoint.Value > dataPoint.MaxValue.Value)
        {
            validationErrors.Add($"Value {dataPoint.Value} is above maximum allowed value {dataPoint.MaxValue.Value}");
        }

        // Parameter-specific validations
        switch (dataPoint.ParameterName.ToLower())
        {
            case "pressure":
                if (dataPoint.Value < 0)
                    validationErrors.Add("Pressure cannot be negative");
                break;
            case "temperature":
                if (dataPoint.Unit.ToLower().Contains("celsius") && dataPoint.Value < -273.15m)
                    validationErrors.Add("Temperature cannot be below absolute zero (-273.15°C)");
                if (dataPoint.Unit.ToLower().Contains("fahrenheit") && dataPoint.Value < -459.67m)
                    validationErrors.Add("Temperature cannot be below absolute zero (-459.67°F)");
                break;
            case "flow rate":
                if (dataPoint.Value < 0)
                    validationErrors.Add("Flow rate cannot be negative");
                break;
        }

        dataPoint.IsValid = validationErrors.Count == 0;
        dataPoint.ValidationMessage = validationErrors.Count > 0 ? string.Join("; ", validationErrors) : null;

        return Task.FromResult(dataPoint.IsValid);
    }

    public async Task<byte[]> ExportToCsvAsync(IEnumerable<DataPoint> dataPoints)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        await csv.WriteRecordsAsync(dataPoints.Select(dp => new
        {
            dp.Id,
            dp.ParameterName,
            dp.Value,
            dp.Unit,
            dp.Timestamp,
            EngineeringUnit = dp.EngineeringUnit?.Name,
            User = dp.User?.FullName,
            dp.Notes,
            dp.IsValid,
            dp.ValidationMessage
        }));

        return memoryStream.ToArray();
    }

    public Task<byte[]> ExportToExcelAsync(IEnumerable<DataPoint> dataPoints)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Data Points");

        // Headers
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Parameter";
        worksheet.Cell(1, 3).Value = "Value";
        worksheet.Cell(1, 4).Value = "Unit";
        worksheet.Cell(1, 5).Value = "Timestamp";
        worksheet.Cell(1, 6).Value = "Engineering Unit";
        worksheet.Cell(1, 7).Value = "User";
        worksheet.Cell(1, 8).Value = "Notes";
        worksheet.Cell(1, 9).Value = "Valid";
        worksheet.Cell(1, 10).Value = "Validation Message";

        // Data
        var row = 2;
        foreach (var dp in dataPoints)
        {
            worksheet.Cell(row, 1).Value = dp.Id;
            worksheet.Cell(row, 2).Value = dp.ParameterName;
            worksheet.Cell(row, 3).Value = dp.Value;
            worksheet.Cell(row, 4).Value = dp.Unit;
            worksheet.Cell(row, 5).Value = dp.Timestamp;
            worksheet.Cell(row, 6).Value = dp.EngineeringUnit?.Name;
            worksheet.Cell(row, 7).Value = dp.User?.FullName;
            worksheet.Cell(row, 8).Value = dp.Notes;
            worksheet.Cell(row, 9).Value = dp.IsValid;
            worksheet.Cell(row, 10).Value = dp.ValidationMessage;
            row++;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    public async Task<Dictionary<string, object>> GetDashboardStatsAsync(int? unitId = null)
    {
        var query = _context.DataPoints.AsQueryable();

        if (unitId.HasValue)
            query = query.Where(dp => dp.EngineeringUnitId == unitId.Value);

        var totalDataPoints = await query.CountAsync();
        var validDataPoints = await query.CountAsync(dp => dp.IsValid);
        var invalidDataPoints = totalDataPoints - validDataPoints;

        var last24Hours = DateTime.UtcNow.AddDays(-1);
        var recentDataPoints = await query.CountAsync(dp => dp.Timestamp >= last24Hours);

        var parameterCounts = await query
            .GroupBy(dp => dp.ParameterName)
            .Select(g => new { Parameter = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        return new Dictionary<string, object>
        {
            ["TotalDataPoints"] = totalDataPoints,
            ["ValidDataPoints"] = validDataPoints,
            ["InvalidDataPoints"] = invalidDataPoints,
            ["RecentDataPoints"] = recentDataPoints,
            ["TopParameters"] = parameterCounts,
            ["ValidationRate"] = totalDataPoints > 0 ? (double)validDataPoints / totalDataPoints * 100 : 0
        };
    }
}

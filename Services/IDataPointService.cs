using DataMonitoringSys.Models;

namespace DataMonitoringSys.Services;

public interface IDataPointService
{
    Task<IEnumerable<DataPoint>> GetDataPointsAsync(int? unitId = null, DateTime? startDate = null, DateTime? endDate = null, string? parameterName = null);
    Task<DataPoint> GetDataPointByIdAsync(int id);
    Task<DataPoint> CreateDataPointAsync(DataPoint dataPoint);
    Task<DataPoint> UpdateDataPointAsync(DataPoint dataPoint);
    Task<bool> DeleteDataPointAsync(int id);
    Task<IEnumerable<DataPoint>> GetRecentDataPointsAsync(int count = 50, int? unitId = null);
    Task<IEnumerable<string>> GetParameterNamesAsync(int? unitId = null);
    Task<bool> ValidateDataPointAsync(DataPoint dataPoint);
    Task<byte[]> ExportToCsvAsync(IEnumerable<DataPoint> dataPoints);
    Task<byte[]> ExportToExcelAsync(IEnumerable<DataPoint> dataPoints);
    Task<Dictionary<string, object>> GetDashboardStatsAsync(int? unitId = null);
}

using DataMonitoringSys.Models;

namespace DataMonitoringSys.Services;

public interface IEngineeringUnitService
{
    Task<IEnumerable<EngineeringUnit>> GetAllUnitsAsync();
    Task<IEnumerable<EngineeringUnit>> GetActiveUnitsAsync();
    Task<EngineeringUnit> GetUnitByIdAsync(int id);
    Task<EngineeringUnit> GetUnitByCodeAsync(string code);
    Task<EngineeringUnit> CreateUnitAsync(EngineeringUnit unit);
    Task<EngineeringUnit> UpdateUnitAsync(EngineeringUnit unit);
    Task<bool> DeleteUnitAsync(int id);
    Task<bool> UnitExistsAsync(int id);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);
}

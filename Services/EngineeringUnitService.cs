using Microsoft.EntityFrameworkCore;
using DataMonitoringSys.Data;
using DataMonitoringSys.Models;

namespace DataMonitoringSys.Services;

public class EngineeringUnitService : IEngineeringUnitService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EngineeringUnitService> _logger;

    public EngineeringUnitService(ApplicationDbContext context, ILogger<EngineeringUnitService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<EngineeringUnit>> GetAllUnitsAsync()
    {
        return await _context.EngineeringUnits
            .Include(eu => eu.Users)
            .Include(eu => eu.DataPoints)
            .OrderBy(eu => eu.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<EngineeringUnit>> GetActiveUnitsAsync()
    {
        return await _context.EngineeringUnits
            .Where(eu => eu.IsActive)
            .Include(eu => eu.Users)
            .Include(eu => eu.DataPoints)
            .OrderBy(eu => eu.Name)
            .ToListAsync();
    }

    public async Task<EngineeringUnit> GetUnitByIdAsync(int id)
    {
        var unit = await _context.EngineeringUnits
            .Include(eu => eu.Users)
            .Include(eu => eu.DataPoints)
            .FirstOrDefaultAsync(eu => eu.Id == id);

        return unit ?? throw new ArgumentException($"Engineering Unit with ID {id} not found.");
    }

    public async Task<EngineeringUnit> GetUnitByCodeAsync(string code)
    {
        var unit = await _context.EngineeringUnits
            .Include(eu => eu.Users)
            .Include(eu => eu.DataPoints)
            .FirstOrDefaultAsync(eu => eu.Code == code);

        return unit ?? throw new ArgumentException($"Engineering Unit with code '{code}' not found.");
    }

    public async Task<EngineeringUnit> CreateUnitAsync(EngineeringUnit unit)
    {
        // Check if code already exists
        if (await CodeExistsAsync(unit.Code))
            throw new InvalidOperationException($"Engineering Unit with code '{unit.Code}' already exists.");

        unit.CreatedAt = DateTime.UtcNow;
        _context.EngineeringUnits.Add(unit);
        await _context.SaveChangesAsync();

        return await GetUnitByIdAsync(unit.Id);
    }

    public async Task<EngineeringUnit> UpdateUnitAsync(EngineeringUnit unit)
    {
        var existingUnit = await _context.EngineeringUnits.FindAsync(unit.Id);
        if (existingUnit == null)
            throw new ArgumentException($"Engineering Unit with ID {unit.Id} not found.");

        // Check if code already exists (excluding current unit)
        if (await CodeExistsAsync(unit.Code, unit.Id))
            throw new InvalidOperationException($"Engineering Unit with code '{unit.Code}' already exists.");

        // Update properties
        existingUnit.Name = unit.Name;
        existingUnit.Description = unit.Description;
        existingUnit.Code = unit.Code;
        existingUnit.IsActive = unit.IsActive;

        await _context.SaveChangesAsync();
        return await GetUnitByIdAsync(unit.Id);
    }

    public async Task<bool> DeleteUnitAsync(int id)
    {
        var unit = await _context.EngineeringUnits.FindAsync(id);
        if (unit == null)
            return false;

        // Check if unit has associated data points or users
        var hasDataPoints = await _context.DataPoints.AnyAsync(dp => dp.EngineeringUnitId == id);
        var hasUsers = await _context.Users.AnyAsync(u => u.EngineeringUnitId == id);

        if (hasDataPoints || hasUsers)
        {
            // Soft delete - just mark as inactive
            unit.IsActive = false;
            await _context.SaveChangesAsync();
        }
        else
        {
            // Hard delete if no associated records
            _context.EngineeringUnits.Remove(unit);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> UnitExistsAsync(int id)
    {
        return await _context.EngineeringUnits.AnyAsync(eu => eu.Id == id);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        var query = _context.EngineeringUnits.Where(eu => eu.Code == code);
        
        if (excludeId.HasValue)
            query = query.Where(eu => eu.Id != excludeId.Value);

        return await query.AnyAsync();
    }
}

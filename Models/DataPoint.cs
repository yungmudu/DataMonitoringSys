using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataMonitoringSys.Models;

public class DataPoint
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string ParameterName { get; set; } = string.Empty; // e.g., "Temperature", "Pressure"
    
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal Value { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Unit { get; set; } = string.Empty; // e.g., "°C", "PSI", "Bar"
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public int EngineeringUnitId { get; set; }
    
    // Validation properties
    [Column(TypeName = "decimal(18,4)")]
    public decimal? MinValue { get; set; }
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal? MaxValue { get; set; }
    
    public bool IsValid { get; set; } = true;
    
    [StringLength(200)]
    public string? ValidationMessage { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual EngineeringUnit EngineeringUnit { get; set; } = null!;
}

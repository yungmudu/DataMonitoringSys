using System.ComponentModel.DataAnnotations;

namespace DataMonitoringSys.Models;

public class EngineeringUnit
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty; // e.g., "UNIT-001", "DEPT-A"
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<DataPoint> DataPoints { get; set; } = new List<DataPoint>();
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}

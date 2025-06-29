using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DataMonitoringSys.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Department { get; set; }
    
    [StringLength(100)]
    public string? JobTitle { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    // Foreign key for assigned engineering unit
    public int? EngineeringUnitId { get; set; }
    
    // Navigation properties
    public virtual EngineeringUnit? EngineeringUnit { get; set; }
    public virtual ICollection<DataPoint> DataPoints { get; set; } = new List<DataPoint>();
    
    // Computed property
    public string FullName => $"{FirstName} {LastName}";
}

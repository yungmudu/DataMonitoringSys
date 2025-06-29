using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DataMonitoringSys.Models;

namespace DataMonitoringSys.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<EngineeringUnit> EngineeringUnits { get; set; }
    public DbSet<DataPoint> DataPoints { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure relationships
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.EngineeringUnit)
            .WithMany(eu => eu.Users)
            .HasForeignKey(u => u.EngineeringUnitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<DataPoint>()
            .HasOne(dp => dp.User)
            .WithMany(u => u.DataPoints)
            .HasForeignKey(dp => dp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DataPoint>()
            .HasOne(dp => dp.EngineeringUnit)
            .WithMany(eu => eu.DataPoints)
            .HasForeignKey(dp => dp.EngineeringUnitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes
        builder.Entity<DataPoint>()
            .HasIndex(dp => dp.Timestamp);

        builder.Entity<DataPoint>()
            .HasIndex(dp => dp.ParameterName);

        builder.Entity<EngineeringUnit>()
            .HasIndex(eu => eu.Code)
            .IsUnique();

        // Seed data
        SeedData(builder);
    }

    private static void SeedData(ModelBuilder builder)
    {
        // Seed Engineering Units
        builder.Entity<EngineeringUnit>().HasData(
            new EngineeringUnit
            {
                Id = 1,
                Name = "Process Engineering Unit A",
                Description = "Primary process control and monitoring unit",
                Code = "PROC-A",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new EngineeringUnit
            {
                Id = 2,
                Name = "Quality Control Lab",
                Description = "Quality assurance and testing laboratory",
                Code = "QC-LAB",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new EngineeringUnit
            {
                Id = 3,
                Name = "Maintenance Department",
                Description = "Equipment maintenance and reliability",
                Code = "MAINT",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new EngineeringUnit
            {
                Id = 4,
                Name = "Safety & Environmental",
                Description = "Safety monitoring and environmental compliance",
                Code = "SAFE-ENV",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}

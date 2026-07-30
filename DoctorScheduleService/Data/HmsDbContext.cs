using DoctorScheduleService.Models.Entities.Hms;
using Microsoft.EntityFrameworkCore;

namespace DoctorScheduleService.Data;

public class HmsDbContext : DbContext
{
    public HmsDbContext(DbContextOptions<HmsDbContext> options) : base(options) { }
    public DbSet<DoctorMaster> DoctorMasters => Set<DoctorMaster>();
    public DbSet<DepartmentMas> DepartmentMas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<DoctorMaster>()
            .ToTable("Doctor_Master", schema: "dbo");
        modelBuilder.Entity<DoctorMaster>()
            .HasIndex(d => d.MobileNo);

        modelBuilder.Entity<DepartmentMas>()
.ToTable("Department_Mas", schema: "dbo");

    }
}
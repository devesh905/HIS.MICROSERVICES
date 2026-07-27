using OPDRegistrationService.Models.Entities.Hms;
using Microsoft.EntityFrameworkCore;

namespace OPDRegistrationService.Data;

public class HmsDbContext : DbContext
{
    public HmsDbContext(DbContextOptions<HmsDbContext> options) : base(options) { }

    public DbSet<PatientRegistration> PatientRegistrations => Set<PatientRegistration>();
    public DbSet<VerticalDepartmentFeeDets> VerticalDepartmentFeeDets => Set<VerticalDepartmentFeeDets>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientRegistration>()
            .ToTable("PatientRegistration", schema: "dbo");
        modelBuilder.Entity<PatientRegistration>()
            .HasIndex(p => p.MobileNo);

        modelBuilder.Entity<VerticalDepartmentFeeDets>()
            .ToTable("VerticalDepartment_Fee_Det", schema: "dbo")
            .HasNoKey();
    }
}
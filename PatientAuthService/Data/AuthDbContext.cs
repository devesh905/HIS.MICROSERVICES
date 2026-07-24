using Microsoft.EntityFrameworkCore;
using PatientAuthService.Models.Entities.ViphaHms;

namespace PatientAuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<PatientRegistration> PatientRegistrations => Set<PatientRegistration>();
    public DbSet<PatientConsultancy> PatientConsultancies => Set<PatientConsultancy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientRegistration>()
            .ToTable("PatientRegistration", schema: "dbo");
        modelBuilder.Entity<PatientRegistration>()
            .HasIndex(p => p.MobileNo);

        modelBuilder.Entity<PatientConsultancy>()
            .ToTable("Patient_Consultancy", schema: "dbo");
        modelBuilder.Entity<PatientConsultancy>()
            .HasIndex(c => c.UhidNo);
    }
}
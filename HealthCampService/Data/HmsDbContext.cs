using HealthCampMicroservice.Models.Entities.Hms;
using Microsoft.EntityFrameworkCore;

namespace HealthCampMicroservice.Data;

public class HmsDbContext : DbContext
{
    public HmsDbContext(DbContextOptions<HmsDbContext> options) : base(options) { }

    public DbSet<PatientRegistration> PatientRegistrations => Set<PatientRegistration>();
    public DbSet<PatientConsultancy> PatientConsultancies => Set<PatientConsultancy>();
    public DbSet<OpdBillingMas> OpdBillingMas => Set<OpdBillingMas>();
    public DbSet<OpdBillingDet> OpdBillingDet => Set<OpdBillingDet>();
    public DbSet<ServiceMaster> ServiceMasters => Set<ServiceMaster>();
    public DbSet<DoctorMaster> DoctorMasters => Set<DoctorMaster>();
    public DbSet<DepartmentMas> DepartmentMas => Set<DepartmentMas>();
    public DbSet<SponsorMaster> SponsorMasters => Set<SponsorMaster>();

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

        modelBuilder.Entity<OpdBillingMas>()
            .ToTable("Opd_Billing_Mas", schema: "dbo");
        modelBuilder.Entity<OpdBillingMas>()
            .HasIndex(b => b.UhidNo);

        modelBuilder.Entity<OpdBillingDet>()
            .ToTable("Opd_Billing_Det", schema: "dbo");
        modelBuilder.Entity<OpdBillingDet>()
            .HasKey(d => new { d.Id, d.Sno });
        modelBuilder.Entity<OpdBillingDet>()
            .HasIndex(d => d.UhidNo);
        modelBuilder.Entity<OpdBillingDet>()
            .HasIndex(d => d.BillNo);

        modelBuilder.Entity<ServiceMaster>()
            .ToTable("Service_Master", schema: "dbo");
        modelBuilder.Entity<ServiceMaster>()
            .HasIndex(s => s.OrgId);
        modelBuilder.Entity<ServiceMaster>()
            .HasIndex(s => s.Ser_Status);

        modelBuilder.Entity<DoctorMaster>()
            .ToTable("Doctor_Master", schema: "dbo");
        modelBuilder.Entity<DoctorMaster>()
            .HasIndex(d => d.MobileNo);

        modelBuilder.Entity<DepartmentMas>()
            .ToTable("Department_Mas", schema: "dbo");

        modelBuilder.Entity<SponsorMaster>()
            .ToTable("Sponsor_Master", schema: "dbo");
    }
}
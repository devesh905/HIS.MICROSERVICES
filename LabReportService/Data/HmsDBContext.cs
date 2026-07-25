using LabReportService.Models.Entities.ViphaHms;
using Microsoft.EntityFrameworkCore;

namespace LabReportService.Data;

public class HmsDbContext : DbContext
{
    public HmsDbContext(DbContextOptions<HmsDbContext> options) : base(options) { }

    public DbSet<ProcedureReport> ProcedureReports => Set<ProcedureReport>();
    public DbSet<PatientRegistration> PatientRegistrations => Set<PatientRegistration>();
    public DbSet<DoctorMaster> DoctorMasters => Set<DoctorMaster>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcedureReport>()
            .ToTable("Procedure_Report", schema: "dbo");
        modelBuilder.Entity<ProcedureReport>()
            .HasIndex(r => r.UHIDNo);
        modelBuilder.Entity<ProcedureReport>()
            .HasIndex(r => r.Report_No);
        modelBuilder.Entity<ProcedureReport>()
            .Property(r => r.Rep_Result)
            .HasColumnType("ntext");

        modelBuilder.Entity<PatientRegistration>()
            .ToTable("PatientRegistration", schema: "dbo");
        modelBuilder.Entity<PatientRegistration>()
            .HasIndex(p => p.MobileNo);

        modelBuilder.Entity<DoctorMaster>()
            .ToTable("Doctor_Master", schema: "dbo");
        modelBuilder.Entity<DoctorMaster>()
            .HasIndex(d => d.MobileNo);
    }
}
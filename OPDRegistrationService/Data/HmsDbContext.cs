using OPDRegistrationService.Models.Entities.Hms;
using Microsoft.EntityFrameworkCore;

namespace OPDRegistrationService.Data;

public class HmsDbContext : DbContext
{
    public HmsDbContext(DbContextOptions<HmsDbContext> options) : base(options) { }

    public DbSet<PatientRegistration> PatientRegistrations => Set<PatientRegistration>();
    public DbSet<VerticalDepartmentFeeDets> VerticalDepartmentFeeDets => Set<VerticalDepartmentFeeDets>();

    public DbSet<DoctorMaster> DoctorMasters => Set<DoctorMaster>();
    public DbSet<DepartmentMas> DepartmentMas { get; set; }
    public DbSet<SponsorMaster> SponsorMasters => Set<SponsorMaster>();
    public DbSet<VerticalMaster> VerticalMasters { get; set; }

    public DbSet<OpdType> OpdTypes => Set<OpdType>();

    public DbSet<TitleMaster> TitleMasters => Set<TitleMaster>();

    public DbSet<RelationMaster> RelationMasters => Set<RelationMaster>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientRegistration>()
            .ToTable("PatientRegistration", schema: "dbo");
        modelBuilder.Entity<PatientRegistration>()
            .HasIndex(p => p.MobileNo);

        modelBuilder.Entity<VerticalDepartmentFeeDets>()
            .ToTable("VerticalDepartment_Fee_Det", schema: "dbo")
            .HasNoKey();

        modelBuilder.Entity<DoctorMaster>()
            .ToTable("Doctor_Master", schema: "dbo");

        modelBuilder.Entity<DepartmentMas>()
            .ToTable("Department_Mas", schema: "dbo");

        modelBuilder.Entity<SponsorMaster>()
            .ToTable("Sponsor_Master", schema: "dbo");

        modelBuilder.Entity<VerticalMaster>()
            .ToTable("Vertical_Master", schema: "dbo");


        modelBuilder.Entity<OpdType>()
            .ToTable("Opd_Type", schema: "dbo");

        modelBuilder.Entity<TitleMaster>()
         .ToTable("Title_Master", schema: "dbo")
           .HasKey(t => t.Id);

        modelBuilder.Entity<RelationMaster>()
            .ToTable("Relation_Master", schema: "dbo")
            .HasNoKey(); // Id can be NULL in real data, so no PK



    }
}
using IpdService.Models.Entities.ViphaHms;
using Microsoft.EntityFrameworkCore;

namespace IpdService.Data;

public class HmsDbContext : DbContext
{
    public HmsDbContext(DbContextOptions<HmsDbContext> options) : base(options) { }

    public DbSet<PatientRegistration> PatientRegistrations => Set<PatientRegistration>();
    public DbSet<DoctorMaster> DoctorMasters => Set<DoctorMaster>();
    public DbSet<SponsorMaster> SponsorMasters => Set<SponsorMaster>();
    public DbSet<ServiceMaster> ServiceMasters => Set<ServiceMaster>();

    public DbSet<IpdRegistration> IpdRegistrations => Set<IpdRegistration>();
    public DbSet<IpdBillMas> IpdBillMas => Set<IpdBillMas>();
    public DbSet<IpdBillDet> IpdBillDet => Set<IpdBillDet>();
    public DbSet<IpdServiceBillMas> IpdServiceBillMas => Set<IpdServiceBillMas>();
    public DbSet<IpdServiceBillDet> IpdServiceBillDet => Set<IpdServiceBillDet>();

    public DbSet<IpdRoomChargesAutoTimeWise> IpdRoomChargesAutoTimeWise => Set<IpdRoomChargesAutoTimeWise>();
    public DbSet<ReceiptMas> ReceiptMas => Set<ReceiptMas>();
    public DbSet<PaymentMas> PaymentMas => Set<PaymentMas>();
    public DbSet<DischargeReq> DischargeReqs => Set<DischargeReq>();

    // Stored-proc result sets (IpdPackageController)
    public DbSet<IpdPackageDetailResult> IpdPackageDetailResults => Set<IpdPackageDetailResult>();
    public DbSet<PackageListResult> PackageListResults => Set<PackageListResult>();
    public DbSet<PackageMasterDetailResult> PackageMasterDetailResults => Set<PackageMasterDetailResult>();
    public DbSet<PackageServiceRegisterResult> PackageServiceRegisterResults => Set<PackageServiceRegisterResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientRegistration>()
            .ToTable("PatientRegistration", schema: "dbo");
        modelBuilder.Entity<PatientRegistration>()
            .HasIndex(p => p.MobileNo);

        modelBuilder.Entity<DoctorMaster>()
            .ToTable("Doctor_Master", schema: "dbo");
        modelBuilder.Entity<DoctorMaster>()
            .HasIndex(d => d.MobileNo);

        modelBuilder.Entity<SponsorMaster>()
            .ToTable("Sponsor_Master", schema: "dbo");

        modelBuilder.Entity<ServiceMaster>()
            .ToTable("Service_Master", schema: "dbo");
        modelBuilder.Entity<ServiceMaster>()
            .HasIndex(s => s.OrgId);
        modelBuilder.Entity<ServiceMaster>()
            .HasIndex(s => s.Ser_Status);

        modelBuilder.Entity<IpdRegistration>()
            .ToTable("IPDRegistration", schema: "dbo");
        modelBuilder.Entity<IpdRegistration>()
            .HasIndex(i => i.UhidNo);

        modelBuilder.Entity<IpdBillMas>()
            .ToTable("IPD_Bill_Mas", schema: "dbo");
        modelBuilder.Entity<IpdBillMas>()
            .HasIndex(b => b.Adm_No);
        modelBuilder.Entity<IpdBillMas>()
            .HasIndex(b => b.BillNo);
        modelBuilder.Entity<IpdBillMas>()
            .Property(b => b.Adm_Date).HasColumnName("Adm_Date");
        modelBuilder.Entity<IpdBillMas>()
            .Property(b => b.DisDate).HasColumnName("DisDate");

        modelBuilder.Entity<IpdBillDet>()
            .ToTable("IPD_Bill_Det", schema: "dbo")
            .HasNoKey();
        modelBuilder.Entity<IpdBillDet>()
            .HasIndex(d => d.BillNo);
        modelBuilder.Entity<IpdBillDet>()
            .HasIndex(d => d.Adm_No);

        modelBuilder.Entity<IpdServiceBillMas>()
            .ToTable("IPD_Service_Bill_Mas", schema: "dbo");
        modelBuilder.Entity<IpdServiceBillMas>()
            .HasIndex(s => s.Adm_No);
        modelBuilder.Entity<IpdServiceBillMas>()
            .HasIndex(s => s.UhidNo);

        modelBuilder.Entity<IpdServiceBillDet>()
            .ToTable("IPD_Service_Bill_Det", schema: "dbo")
            .HasNoKey();
        modelBuilder.Entity<IpdServiceBillDet>()
            .HasIndex(s => s.Adm_No);
        modelBuilder.Entity<IpdServiceBillDet>()
            .HasIndex(s => s.BillNo);

        modelBuilder.Entity<IpdRoomChargesAutoTimeWise>()
            .ToTable("Ipd_Room_Charges_Auto_TimeWise", schema: "dbo")
            .HasIndex(r => r.Adm_No);

        modelBuilder.Entity<ReceiptMas>()
            .ToTable("Receipt_Mas", schema: "dbo");
        modelBuilder.Entity<ReceiptMas>()
            .HasIndex(r => r.Adm_No);

        modelBuilder.Entity<PaymentMas>()
            .ToTable("Payment_Mas", schema: "dbo");
        modelBuilder.Entity<PaymentMas>()
            .HasIndex(p => p.Adm_No);

        modelBuilder.Entity<DischargeReq>()
            .ToTable("Discharge_Req", schema: "dbo");
        modelBuilder.Entity<DischargeReq>()
            .HasIndex(d => d.Adm_No);

        modelBuilder.Entity<IpdPackageDetailResult>().HasNoKey();
        modelBuilder.Entity<PackageListResult>().HasNoKey();
        modelBuilder.Entity<PackageMasterDetailResult>().HasNoKey();
        modelBuilder.Entity<PackageServiceRegisterResult>().HasNoKey();
    }
}
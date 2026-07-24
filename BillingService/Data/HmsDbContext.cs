using BillingService.Models.Entities.ViphaHms;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace BillingService.Data;

public class HmsDbContext : DbContext
{
    public HmsDbContext(DbContextOptions<HmsDbContext> options) : base(options) { }

    public DbSet<PatientRegistration> PatientRegistrations => Set<PatientRegistration>();
    public DbSet<DoctorMaster> DoctorMasters => Set<DoctorMaster>();
    public DbSet<SponsorMaster> SponsorMasters => Set<SponsorMaster>();
    public DbSet<ServiceMaster> ServiceMasters => Set<ServiceMaster>();

    public DbSet<OpdBillingMas> OpdBillingMas => Set<OpdBillingMas>();
    public DbSet<OpdBillingDet> OpdBillingDet => Set<OpdBillingDet>();

    public DbSet<IpdRegistration> IpdRegistrations => Set<IpdRegistration>();
    public DbSet<IpdBillMas> IpdBillMas => Set<IpdBillMas>();
    public DbSet<IpdBillDet> IpdBillDet => Set<IpdBillDet>();
    public DbSet<IpdServiceBillMas> IpdServiceBillMas => Set<IpdServiceBillMas>();
    public DbSet<IpdServiceBillDet> IpdServiceBillDet => Set<IpdServiceBillDet>();
    public DbSet<IpdRoomChargesAutoTimeWise> IpdRoomChargesAutoTimeWise => Set<IpdRoomChargesAutoTimeWise>();

    public DbSet<DischargeReq> DischargeReqs => Set<DischargeReq>();
    public DbSet<ReceiptMas> ReceiptMas => Set<ReceiptMas>();
    public DbSet<PaymentMas> PaymentMas => Set<PaymentMas>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientRegistration>().ToTable("PatientRegistration", schema: "dbo");
        modelBuilder.Entity<PatientRegistration>().HasIndex(p => p.MobileNo);

        modelBuilder.Entity<DoctorMaster>().ToTable("Doctor_Master", schema: "dbo");
        modelBuilder.Entity<SponsorMaster>().ToTable("Sponsor_Master", schema: "dbo");
        modelBuilder.Entity<ServiceMaster>().ToTable("Service_Master", schema: "dbo");
        modelBuilder.Entity<ServiceMaster>().HasIndex(s => s.OrgId);
        modelBuilder.Entity<ServiceMaster>().HasIndex(s => s.Ser_Status);

        modelBuilder.Entity<OpdBillingMas>().ToTable("Opd_Billing_Mas", schema: "dbo");
        modelBuilder.Entity<OpdBillingMas>().HasIndex(b => b.UhidNo);

        modelBuilder.Entity<OpdBillingDet>().ToTable("Opd_Billing_Det", schema: "dbo");
        modelBuilder.Entity<OpdBillingDet>().HasKey(d => new { d.Id, d.Sno });
        modelBuilder.Entity<OpdBillingDet>().HasIndex(d => d.UhidNo);
        modelBuilder.Entity<OpdBillingDet>().HasIndex(d => d.BillNo);

        modelBuilder.Entity<IpdRegistration>().ToTable("IPDRegistration", schema: "dbo");
        modelBuilder.Entity<IpdRegistration>().HasIndex(i => i.UhidNo);

        modelBuilder.Entity<IpdBillMas>().ToTable("IPD_Bill_Mas", schema: "dbo");
        modelBuilder.Entity<IpdBillMas>().HasIndex(b => b.Adm_No);
        modelBuilder.Entity<IpdBillMas>().HasIndex(b => b.BillNo);
        modelBuilder.Entity<IpdBillMas>().Property(b => b.Adm_Date).HasColumnName("Adm_Date");
        modelBuilder.Entity<IpdBillMas>().Property(b => b.DisDate).HasColumnName("DisDate");

        modelBuilder.Entity<IpdBillDet>().ToTable("IPD_Bill_Det", schema: "dbo").HasNoKey();
        modelBuilder.Entity<IpdBillDet>().HasIndex(d => d.BillNo);
        modelBuilder.Entity<IpdBillDet>().HasIndex(d => d.Adm_No);

        modelBuilder.Entity<IpdServiceBillMas>().ToTable("IPD_Service_Bill_Mas", schema: "dbo");
        modelBuilder.Entity<IpdServiceBillMas>().HasIndex(s => s.Adm_No);
        modelBuilder.Entity<IpdServiceBillMas>().HasIndex(s => s.UhidNo);

        modelBuilder.Entity<IpdServiceBillDet>().ToTable("IPD_Service_Bill_Det", schema: "dbo").HasNoKey();
        modelBuilder.Entity<IpdServiceBillDet>().HasIndex(s => s.Adm_No);
        modelBuilder.Entity<IpdServiceBillDet>().HasIndex(s => s.BillNo);

        modelBuilder.Entity<IpdRoomChargesAutoTimeWise>()
            .ToTable("Ipd_Room_Charges_Auto_TimeWise", schema: "dbo")
            .HasIndex(r => r.Adm_No);

        modelBuilder.Entity<DischargeReq>().ToTable("Discharge_Req", schema: "dbo");
        modelBuilder.Entity<DischargeReq>().HasIndex(d => d.Adm_No);

        modelBuilder.Entity<ReceiptMas>().ToTable("Receipt_Mas", schema: "dbo");
        modelBuilder.Entity<ReceiptMas>().HasIndex(r => r.Adm_No);

        modelBuilder.Entity<PaymentMas>().ToTable("Payment_Mas", schema: "dbo");
        modelBuilder.Entity<PaymentMas>().HasIndex(p => p.Adm_No);
    }
}
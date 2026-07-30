using DoctorScheduleService.Models.Entities.Hms;
using Microsoft.EntityFrameworkCore;

namespace DoctorScheduleService.Data;

public class HmsDbContext : DbContext
{
    public HmsDbContext(DbContextOptions<HmsDbContext> options) : base(options) { }
    public DbSet<PatientRegistration> PatientRegistrations => Set<PatientRegistration>();
    public DbSet<DoctorMaster> DoctorMasters => Set<DoctorMaster>();
    public DbSet<DiagnosisMaster> DiagnosisMasters => Set<DiagnosisMaster>();
    public DbSet<SponsorMaster> SponsorMasters => Set<SponsorMaster>();
    public DbSet<DepartmentMas> DepartmentMas { get; set; }
    public DbSet<VerticalMaster> VerticalMasters { get; set; }
    public DbSet<ProcedureReport> ProcedureReports => Set<ProcedureReport>();
    public DbSet<PatientConsultancy> PatientConsultancies => Set<PatientConsultancy>();
    public DbSet<ServiceMaster> ServiceMasters { get; set; }
    public DbSet<OpdBillingMas> OpdBillingMas { get; set; }
    public DbSet<OpdBillingDet> OpdBillingDet { get; set; }
    public DbSet<IpdRegistration> IpdRegistrations => Set<IpdRegistration>();
    public DbSet<IpdBillMas> IpdBillMas => Set<IpdBillMas>();
    public DbSet<IpdBillDet> IpdBillDet => Set<IpdBillDet>();
    public DbSet<IpdServiceBillMas> IpdServiceBillMas => Set<IpdServiceBillMas>();
    public DbSet<IpdServiceBillDet> IpdServiceBillDet => Set<IpdServiceBillDet>();
    public DbSet<VerticalDepartmentFeeDets> VerticalDepartmentFeeDets { get; set; }

    public DbSet<PatientTypMas> PatientTypMas => Set<PatientTypMas>();
    public DbSet<OpdType> OpdTypes => Set<OpdType>();
    public DbSet<IpdRoomChargesAuto> IpdRoomChargesAuto => Set<IpdRoomChargesAuto>();
    public DbSet<IpdRoomChargesAutoTimeWise> IpdRoomChargesAutoTimeWise => Set<IpdRoomChargesAutoTimeWise>();
    public DbSet<ReceiptMas> ReceiptMas => Set<ReceiptMas>();
    public DbSet<PaymentMas> PaymentMas => Set<PaymentMas>();
    public DbSet<DischargeSummaryMas> DischargeSummaryMas => Set<DischargeSummaryMas>();
    public DbSet<DischargeReq> DischargeReqs => Set<DischargeReq>();
    public DbSet<LocationMaster> LocationMasters { get; set; }
    public DbSet<DischargeSummaryDiagDet> DischargeSummaryDiagDets => Set<DischargeSummaryDiagDet>();

    public DbSet<ServicePackageDet> ServicePackageDets { get; set; }
    public DbSet<ServicePriceListMaster> ServicePriceListMasters { get; set; }

    public DbSet<IpdPackageDetailResult> IpdPackageDetailResults { get; set; }
    public DbSet<PackageListResult> PackageListResults { get; set; }
    public DbSet<PackageMasterDetailResult> PackageMasterDetailResults { get; set; }
    public DbSet<PackageServiceRegisterResult> PackageServiceRegisterResults { get; set; }

    public DbSet<TitleMaster> TitleMasters => Set<TitleMaster>();
    public DbSet<RelationMaster> RelationMasters => Set<RelationMaster>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<PatientRegistration>()
            .ToTable("PatientRegistration", schema: "dbo");

        /// MobileNo is not unique in HIS — same patient can have multiple visits
        /// We'll always fetch the LATEST registration by MobileNo
        modelBuilder.Entity<PatientRegistration>()
            .HasIndex(p => p.MobileNo);

        modelBuilder.Entity<DoctorMaster>()
            .ToTable("Doctor_Master", schema: "dbo");
        modelBuilder.Entity<DoctorMaster>()
            .HasIndex(d => d.MobileNo);

        modelBuilder.Entity<DiagnosisMaster>()
            .ToTable("Diagnosis_Master", schema: "dbo");
        modelBuilder.Entity<DiagnosisMaster>()
            .HasIndex(d => d.Icd_Code);   // ICD code is the natural lookup key

        modelBuilder.Entity<SponsorMaster>()
            .ToTable("Sponsor_Master", schema: "dbo");

        modelBuilder.Entity<ProcedureReport>()
            .ToTable("Procedure_Report", schema: "dbo");
        modelBuilder.Entity<ProcedureReport>()
            .HasIndex(r => r.UHIDNo);  
        modelBuilder.Entity<ProcedureReport>()
            .HasIndex(r => r.Report_No);

        modelBuilder.Entity<ProcedureReport>()
            .Property(r => r.Rep_Result)
            .HasColumnType("ntext");

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
            .HasIndex(d => d.UhidNo);
        modelBuilder.Entity<OpdBillingDet>()
            .HasIndex(d => d.BillNo);

        modelBuilder.Entity<OpdBillingDet>()
            .ToTable("Opd_Billing_Det", schema: "dbo");
        modelBuilder.Entity<OpdBillingDet>()
            .HasKey(d => new { d.Id, d.Sno });
        modelBuilder.Entity<OpdBillingDet>()
            .HasIndex(d => d.UhidNo);
        modelBuilder.Entity<OpdBillingDet>()
            .HasIndex(d => d.BillNo);

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

        // IPD_Service_Bill_Mas (Provisional: service charges master)
        modelBuilder.Entity<IpdServiceBillMas>()
            .ToTable("IPD_Service_Bill_Mas", schema: "dbo");
        modelBuilder.Entity<IpdServiceBillMas>()
            .HasIndex(s => s.Adm_No);
        modelBuilder.Entity<IpdServiceBillMas>()
            .HasIndex(s => s.UhidNo);

        // IPD_Service_Bill_Det (Provisional: service charges detail)
        modelBuilder.Entity<IpdServiceBillDet>()
            .ToTable("IPD_Service_Bill_Det", schema: "dbo")
            .HasNoKey();                          
        modelBuilder.Entity<IpdServiceBillDet>()
            .HasIndex(s => s.Adm_No);
        modelBuilder.Entity<IpdServiceBillDet>()
            .HasIndex(s => s.BillNo);

        // Ipd_Room_Charges_Auto
        modelBuilder.Entity<IpdRoomChargesAuto>()
            .ToTable("Ipd_Room_Charges_Auto", schema: "dbo")
            .HasIndex(r => r.Adm_No);

        // IpdRoomChargesAuto index config with TimeWise version
        modelBuilder.Entity<IpdRoomChargesAutoTimeWise>()
            .ToTable("Ipd_Room_Charges_Auto_TimeWise", schema: "dbo")
            .HasIndex(r => r.Adm_No);

        // Receipt_Mas (Advance deposits)
        modelBuilder.Entity<ReceiptMas>()
            .ToTable("Receipt_Mas", schema: "dbo");
        modelBuilder.Entity<ReceiptMas>()
            .HasIndex(r => r.Adm_No);

        // Payment_Mas (Advance refunds)
        modelBuilder.Entity<PaymentMas>()
            .ToTable("Payment_Mas", schema: "dbo");
        modelBuilder.Entity<PaymentMas>()
            .HasIndex(p => p.Adm_No);

        modelBuilder.Entity<DischargeSummaryMas>()
            .ToTable("DischargeSummary_Mas", schema: "dbo");
        modelBuilder.Entity<DischargeSummaryMas>()
            .HasIndex(d => d.Adm_No);
        modelBuilder.Entity<DischargeSummaryMas>()
            .Property(d => d.DisChargeParticular).HasColumnType("ntext");
        modelBuilder.Entity<DischargeSummaryMas>()
            .Property(d => d.AdviceOnDischarge).HasColumnType("ntext");

        modelBuilder.Entity<DischargeReq>()
            .ToTable("Discharge_Req", schema: "dbo");
        modelBuilder.Entity<DischargeReq>()
            .HasIndex(d => d.Adm_No);

        /// DischargeSummaryDiag_det — no natural single PK, use Id
        modelBuilder.Entity<DischargeSummaryDiagDet>()
            .ToTable("DischargeSummaryDiag_det", schema: "dbo");
        modelBuilder.Entity<DischargeSummaryDiagDet>()
            .HasIndex(d => d.Adm_No);

        modelBuilder.Entity<DepartmentMas>()
    .ToTable("Department_Mas", schema: "dbo");

        modelBuilder.Entity<VerticalMaster>()
            .ToTable("Vertical_Master", schema: "dbo");


        modelBuilder.Entity<VerticalDepartmentFeeDets>()
    .ToTable("VerticalDepartment_Fee_Det", schema: "dbo")
    .HasNoKey(); // no single PK column


        /// Service_Package_Det — no single PK, line items grouped by Id (PackageId)
        modelBuilder.Entity<ServicePackageDet>()
            .ToTable("Service_Package_Det", schema: "dbo")
            .HasNoKey();
        modelBuilder.Entity<ServicePackageDet>()
            .HasIndex(s => s.Id);         // query by package ID
        modelBuilder.Entity<ServicePackageDet>()
            .HasIndex(s => s.OrgId);      // filter by hospital

        modelBuilder.Entity<ServicePriceListMaster>()
            .ToTable("Service_PriceList_Master", schema: "dbo");
        modelBuilder.Entity<ServicePriceListMaster>()
            .HasKey(s => s.S_id);
        modelBuilder.Entity<ServicePriceListMaster>()
            .HasIndex(s => s.OrgId);
        modelBuilder.Entity<ServicePriceListMaster>()
            .HasIndex(s => s.Sponsor_id);


        modelBuilder.Entity<ServiceMaster>()
    .ToTable("Service_Master", schema: "dbo");
        modelBuilder.Entity<ServiceMaster>()
            .HasIndex(s => s.OrgId);
        modelBuilder.Entity<ServiceMaster>()
            .HasIndex(s => s.Ser_Status);


        modelBuilder.Entity<IpdPackageDetailResult>().HasNoKey();
        modelBuilder.Entity<PackageListResult>().HasNoKey();
        modelBuilder.Entity<PackageMasterDetailResult>().HasNoKey();
        modelBuilder.Entity<PackageServiceRegisterResult>().HasNoKey();

        modelBuilder.Entity<TitleMaster>()
    .ToTable("Title_Master", schema: "dbo")
    .HasKey(t => t.Id);

        modelBuilder.Entity<RelationMaster>()
            .ToTable("Relation_Master", schema: "dbo")
            .HasNoKey(); // Id can be NULL in real data, so no PK

    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models.Entities.Hms;

[Table("Service_Master", Schema = "dbo")]
public class ServiceMaster
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    public string? Ser_Name { get; set; }
    public string? Ser_Code { get; set; }
    public int? MgrpId { get; set; }
    public int? SubGrpId { get; set; }
    public string? Ser_type { get; set; }
    public decimal? Opd_Charges { get; set; }
    public decimal? Ipd_Charges { get; set; }
    public string? Editable { get; set; }
    public decimal? Min_Charges { get; set; }
    public decimal? Max_Charges { get; set; }
    public string? Time_Bound_Charges { get; set; }
    public string? Charge_type { get; set; }
    public string? Account_No { get; set; }
    public int OrgId { get; set; }
    public string? Qty_Editable { get; set; }
    public string? Acknoledge_Status { get; set; }
    public string? Appoint_Service_Status { get; set; }
    public short? icu_Day { get; set; }
    public string? Ipd_Account { get; set; }
    public int? Sponsor_id { get; set; }
    public string? Ser_Status { get; set; }
    public string? Ot_Status { get; set; }
    public short? SSNO { get; set; }
    public string? Avail_For { get; set; }
    public string? PayMode { get; set; }
    public string? Discount_App { get; set; }
    public string? Mul_Sitting { get; set; }
    public string? Ser_OutSource { get; set; }
    public string? Def_Ser_Adm { get; set; }
    public string? Is_Daily { get; set; }
    public string? Res_In_Gen { get; set; }
    public string? Avail_Unit { get; set; }
    public short? Package_Days { get; set; }
    public string? Display_Progess_Sheet { get; set; }
    public string? Provided_By { get; set; }
    public string? Reporting_Allowed { get; set; }
    public int? Free_Visit { get; set; }
    public string? Ivf_Package_Status { get; set; }
    public decimal? Refund_Pr { get; set; }
    public string? Sub_Service { get; set; }
    public decimal? Ser_Share_Amount { get; set; }
    public short? GstPer { get; set; }
    public string? HsnCode { get; set; }
    public string? Ser_Description { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? Sno { get; set; }
    public bool? DisplayInProc { get; set; }
    public bool? BillByNurse { get; set; }
    public bool? IsSchemeDiscountAllow { get; set; }
    public int? OSAccountId { get; set; }
    public int? JrPackage { get; set; }
    public bool? MedicineAtCost { get; set; }
    public bool? IsConsumable { get; set; }
    public int? IsAdmitPackage { get; set; }
    public int? IsConduct { get; set; }
    public bool? IsProcedureEntry { get; set; }
    public short? IsNMC { get; set; }
}
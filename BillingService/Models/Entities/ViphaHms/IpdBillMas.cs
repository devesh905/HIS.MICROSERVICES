using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models.Entities.ViphaHms;

[Table("IPD_Bill_Mas", Schema = "dbo")]
public class IpdBillMas
{
    [Key]
    public long Id { get; set; }
    public string? Adm_No { get; set; }
    public string? PatientName { get; set; }
    public string? BillNo { get; set; }
    public DateTime? Bill_Date { get; set; }
    public decimal? TotalNetAmount { get; set; }
    public short? Orgid { get; set; }
    public short? FinYr { get; set; }
    public int? UserId { get; set; }
    public string? T_status { get; set; }
    public short? Mla_Status { get; set; }
    public string? Remarks { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastModifiedby { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? Can_Remarks { get; set; }
    public int? CancelById { get; set; }
    public DateTime? CancelAt { get; set; }
    public string? CancelSystemName { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? DisAmt { get; set; }
    public decimal? ReceiptAmount { get; set; }
    public string? BillTime { get; set; }
    public bool? ReOpenForReturn { get; set; }
    public bool? AccountPosting { get; set; }
    public int? LastAuthById { get; set; }
    public DateTime? LastAuthByDate { get; set; }
    public string? LastAuthSystemName { get; set; }
    public decimal? UnderPackageAmt { get; set; }
    public decimal? GstAmount { get; set; }
    public decimal? UPAmt { get; set; }
    public string? Remarks1 { get; set; }
    public string? IsImplantService { get; set; }
    public DateTime? Adm_Date { get; set; }
    public string? Adm_Time { get; set; }
    public DateTime? DisDate { get; set; }
    public string? DisTime { get; set; }
}
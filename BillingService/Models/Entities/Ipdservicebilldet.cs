using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models.Entities.Hms;

[Table("IPD_Service_Bill_Det", Schema = "dbo")]
public class IpdServiceBillDet
{
    public long Id { get; set; }
    public string? Adm_No { get; set; }
    public string? BillNo { get; set; }
    public DateTime? Bill_Date { get; set; }
    public int? Sno { get; set; }
    public string? Typ { get; set; }    /// S = Service, R = Radiology, L = Lab, etc.
    public int? SerId { get; set; }
    public int? OpDocId { get; set; }
    public decimal? Qty { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Due { get; set; }
    public decimal? Amount { get; set; }
    public int? DisAuthId { get; set; }
    public int? DisId { get; set; }
    public decimal? LimitPer { get; set; }
    public decimal? LimitAmount { get; set; }
    public decimal? DisPer { get; set; }
    public decimal? DisAmt { get; set; }
    public decimal? Bill_Discount { get; set; }
    public decimal? NetAmount { get; set; }
    public string? Ser_Type { get; set; }
    public int? UnderServiceId { get; set; }
    public decimal? ActualRate { get; set; }
    public string? IsPartial_Discount { get; set; }
    public string? Can_Status { get; set; }
    public int? Can_UserID { get; set; }
    public string? Canc_SystemName { get; set; }
    public DateTime? Canc_Time { get; set; }
    public short? OrgId { get; set; }
    public short? Finyr { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public string? IncludeInBill { get; set; }
    public decimal? SerRate { get; set; }
    public decimal? SerAmount { get; set; }
    public decimal? SerDis { get; set; }
    public string? i1 { get; set; }
    public int? Ac_Jr_Id { get; set; }
    public string? Indent_No { get; set; }
    public bool? OutSourced { get; set; }
    public int? OSAccountId { get; set; }
    public decimal? GstPer { get; set; }
    public decimal? GstAmt { get; set; }
    public string? Remarks { get; set; }
}
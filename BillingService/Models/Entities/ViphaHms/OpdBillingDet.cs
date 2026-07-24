using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Models.Entities.ViphaHms;

[Table("Opd_Billing_Det", Schema = "dbo")]
[PrimaryKey(nameof(Id), nameof(Sno))]   // composite key — THIS is the fix
public class OpdBillingDet
{
    public long Id { get; set; }         // master bill Id, NOT a unique row key
    public string? UhidNo { get; set; }
    public string? BillNo { get; set; }
    public int Sno { get; set; }         // remove ? — part of PK, must not be nullable
    public string? Typ { get; set; }
    public int? SerId { get; set; }
    public int? Qty { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Due { get; set; }
    public decimal? Cons { get; set; }
    public decimal? Amount { get; set; }
    public int? DisAuthId { get; set; }
    public int? DisId { get; set; }
    public decimal? LimitPer { get; set; }
    public decimal? LimitAmount { get; set; }
    public decimal? DisPer { get; set; }
    public decimal? DisAmt { get; set; }
    public decimal? Bill_Discount { get; set; }
    public decimal? NetAmount { get; set; }
    public string? Ser_type { get; set; }
    public int? UnderServiceId { get; set; }
    public decimal? ActualRate { get; set; }
    public string? IsPartial_Discount { get; set; }
    public string? Can_Status { get; set; }
    public int? CanUserId { get; set; }
    public DateTime? Canc_Time { get; set; }
    public string? Canc_Pc { get; set; }
    public short? OrgId { get; set; }
    public short? FinYr { get; set; }
    public int? UserId { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? OpDocId { get; set; }
    public string? T_Status { get; set; }
    public short? Mla_Status { get; set; }
    public string? OpNo1 { get; set; }
    public int? Ac_Jr_Id { get; set; }
    public bool? OutSourced { get; set; }
    public int? OSAccountId { get; set; }
    public string? Indent_No { get; set; }
    public decimal? GstPer { get; set; }
    public decimal? GstAmt { get; set; }
}
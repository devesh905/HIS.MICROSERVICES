using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IpdService.Models.Entities.ViphaHms;

[Table("IPD_Bill_Det", Schema = "dbo")]
public class IpdBillDet
{
    public long Id { get; set; }
    public string? BillNo { get; set; }
    public string? Ser_Type { get; set; }
    public int? Ser_Id { get; set; }
    public string? Ser_name { get; set; }
    public int? Qty { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
    public decimal? DisPer { get; set; }
    public decimal? DisAmt { get; set; }
    public decimal? NetAmount { get; set; }
    public short? Orgid { get; set; }
    public short? FinYr { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public string? Adm_No { get; set; }
    public decimal? UPAmt { get; set; }
    public decimal? GstAmt { get; set; }
}
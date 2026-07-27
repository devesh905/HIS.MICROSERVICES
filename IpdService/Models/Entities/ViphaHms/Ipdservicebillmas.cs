using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IpdService.Models.Entities.ViphaHms;

[Table("IPD_Service_Bill_Mas", Schema = "dbo")]
public class IpdServiceBillMas
{
    [Key]
    public long Id { get; set; }
    public string? Adm_No { get; set; }
    public string? BillNo { get; set; }
    public DateTime? Bill_Date { get; set; }
    public string? Timein { get; set; }
    public string? UhidNo { get; set; }
    public long? OrgId { get; set; }
    public short? Finyr { get; set; }
    public int? CWardId { get; set; }
    public int? CRoomId { get; set; }
    public string? CBed { get; set; }
    public int? LocId { get; set; }
    public string? Paymode { get; set; }
    public int? AccId { get; set; }
    public string? ChequeNo { get; set; }
    public string? Auth_code { get; set; }
    public decimal? Qty { get; set; }
    public decimal? TotalAmt { get; set; }
    public decimal? Ser_Cons { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? Round_off { get; set; }
    public decimal? Con_Amount { get; set; }
    public decimal? Receipt_Amount1 { get; set; }
    public string? Paymode2 { get; set; }
    public string? ChequeNo2 { get; set; }
    public int? AccId2 { get; set; }
    public string? Auth_code2 { get; set; }
    public decimal? Receipt_Amount2 { get; set; }
    public decimal? Receipt_Amount { get; set; }
    public decimal? Balance { get; set; }
    public int? Can_UserID { get; set; }
    public string? Canc_Pc { get; set; }
    public DateTime? Canc_Time { get; set; }
    public string? Remarks { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastModifiedby { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? DocId { get; set; }
    public int? SponsId { get; set; }
    public int? DepId { get; set; }
    public int? Ac_Jr_Id { get; set; }
    public string? Indent_No { get; set; }
    public bool? AccountPosting { get; set; }
    public bool? OutSourced { get; set; }
    public int? OSAccountId { get; set; }
    public decimal? GSTAmount { get; set; }
    public bool? IsCancel { get; set; }
    public string? CancelRemarks { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IpdService.Models.Entities.ViphaHms;

[Table("Ipd_Room_Charges_Auto_TimeWise", Schema = "dbo")]
public class IpdRoomChargesAutoTimeWise
{
    [Key]
    [Column(TypeName = "numeric(18,0)")]
    public decimal Id { get; set; }
    public DateTime? DocDate { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Adm_No { get; set; }
    public int? WardId { get; set; }
    public int? RoomId { get; set; }
    public string? Bed { get; set; }
    public decimal? Qty { get; set; }
    public short? OrgId { get; set; }
    public DateTime? Up_date { get; set; }
    public string? Sib { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Dp { get; set; }
    public decimal? Discount { get; set; }
    public string? IncludeInBill { get; set; }
    public int? UnderServiceId { get; set; }
    public decimal? Amount { get; set; }
    public decimal? NetAmount { get; set; }
}
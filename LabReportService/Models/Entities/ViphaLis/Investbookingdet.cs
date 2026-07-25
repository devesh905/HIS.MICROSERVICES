using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabReportService.Models.Entities.ViphaLis;

[Table("Invest_Booking_Det", Schema = "dbo")]
public class InvestBookingDet
{
    [Key]
    public long Id { get; set; }
    public string? UhidNo { get; set; }
    public string? BillNo { get; set; }
    public int? Sno { get; set; }

    /// <summary>L = Lab, R = Radiology</summary>
    public string? Typ { get; set; }
    public int? TestId { get; set; }
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
    public long? OrgId { get; set; }
    public short? FinYr { get; set; }
    public int? UserId { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? OpDocId { get; set; }
    public int? ValidateById { get; set; }
    public DateTime? ValidateAt { get; set; }
    public int? AuthorizedById { get; set; }
    public DateTime? AuthorizeAt { get; set; }
    public int? AuthDoctorId { get; set; }
    public decimal? SerRate { get; set; }
    public decimal? SerAmount { get; set; }
    public decimal? SerDis { get; set; }
    public string? IncludeInBill { get; set; }
    public string? Result_status { get; set; }
    public bool? IsDispatch { get; set; }
}
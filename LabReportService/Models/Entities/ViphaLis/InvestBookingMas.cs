using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabReportService.Models.Entities.ViphaLis;

[Table("Invest_Booking_Mas")]
public class InvestBookingMas
{
    [Key] public long Id { get; set; }
    public string? UhidNo { get; set; }
    public string? BillNo { get; set; }
    public DateTime? Bill_Date { get; set; }
    public string? PatientName { get; set; }
    public string? Gender { get; set; }
    public string? Title { get; set; }
    public short? AgeInYears { get; set; }
    public short? AgeInMonths { get; set; }
    public short? AgeInDays { get; set; }
    public string? Mobile_No { get; set; }
    public int? DocId { get; set; }
    public string? RefByName { get; set; }
    public string? Barcode { get; set; }
    public string? Adm_No { get; set; }
    public bool? IsOnlineReport { get; set; }
    public bool? IsCancel { get; set; }
    public string? BookingMode { get; set; }
    public string? Timein { get; set; }
    public long? OrgId { get; set; }      // bigint
    public short? FinYr { get; set; }
    public int? UserId { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastModifiedby { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
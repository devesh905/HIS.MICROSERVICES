namespace BillingService.Models.DTOs;

public class BillDetailItemDto
{
    public int? Sno { get; set; }
    public string? Typ { get; set; }        // S=Service, L=Lab, R=Radiology
    public string? SerName { get; set; }
    public int? Qty { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
    public decimal? DisPer { get; set; }
    public decimal? DisAmt { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? GstPer { get; set; }
    public decimal? GstAmt { get; set; }
    public string? Can_Status { get; set; }
}

public class BillDto
{
    public long? Id { get; set; }
    public string? BillNo { get; set; }
    public DateTime? Bill_Date { get; set; }
    public string? Timein { get; set; }
    public string? UhidNo { get; set; }
    public string? OpNo { get; set; }
    public string? OpNo1 { get; set; }
    public int? VisitNo { get; set; }
    public string? PayMode { get; set; }
    public string? PayMode2 { get; set; }
    public decimal? TotalAmt { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? Receipt_Amount { get; set; }
    public decimal? Balance { get; set; }
    public decimal? GSTAmount { get; set; }
    public decimal? Round_off { get; set; }
    public bool? IsCancel { get; set; }
    public string? CancelRemarks { get; set; }
    public string? Remarks { get; set; }
    public string? PatientName { get; set; }
    public string? AgeSex { get; set; }      // e.g. "32 Y / Male"
    public string? Mobile { get; set; }
    public string? ConsultantName { get; set; }
    public string? DepartmentName { get; set; }
    public string? SponsorName { get; set; }
    public string? BillSource { get; set; }
    public string? Address { get; set; }          // NEW
    public string? ReceivedByName { get; set; }    // NEW
    public List<BillDetailItemDto> Items { get; set; } = new();
}
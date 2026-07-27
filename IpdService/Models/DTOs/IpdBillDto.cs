namespace IpdService.Models.DTOs;

public class IpdBillSummaryDto
{
    public string? Adm_No { get; set; }
    public string? BillNo { get; set; }
    public string? PatientName { get; set; }
    public DateTime? Bill_Date { get; set; }
    public string? BillTime { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? DisAmt { get; set; }
    public decimal? ReceiptAmount { get; set; }
    public decimal? TotalNetAmount { get; set; }
    public decimal? GstAmount { get; set; }
    public decimal? UnderPackageAmt { get; set; }
    public string? T_status { get; set; }
    public DateTime? Adm_Date { get; set; }
    public DateTime? DisDate { get; set; }
    public string? HospitalKey { get; set; }
    public string? HospitalName { get; set; }
}

public class IpdBillDetailDto : IpdBillSummaryDto
{
    // Master extras
    public string? Remarks { get; set; }
    public string? Remarks1 { get; set; }
    public string? Can_Remarks { get; set; }
    public DateTime? CancelAt { get; set; }
    public string? IsImplantService { get; set; }
    public bool? ReOpenForReturn { get; set; }
    public bool? AccountPosting { get; set; }
    public decimal? UPAmt { get; set; }

    // Line items
    public List<IpdBillLineDto> LineItems { get; set; } = [];
}

public class IpdBillLineDto
{
    public string? Ser_Type { get; set; }
    public string? Ser_name { get; set; }
    public int? Qty { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
    public decimal? DisPer { get; set; }
    public decimal? DisAmt { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? GstAmt { get; set; }
    public decimal? UPAmt { get; set; }
}
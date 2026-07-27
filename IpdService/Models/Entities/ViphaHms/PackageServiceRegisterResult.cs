namespace IpdService.Models.Entities.ViphaHms;

public class PackageServiceRegisterResult
{
    public string? DocNo { get; set; }
    public string? UhidNo { get; set; }
    public string? Adm_No { get; set; }
    public string? Typ { get; set; }
    public DateTime? DocDate { get; set; }
    public string? PatientName { get; set; }
    public string? Paymode { get; set; }
    public int? DocId { get; set; }
    public string? DocName { get; set; }
    public int? SponsorID { get; set; }
    public string? SponsorName { get; set; }
    public int? MgrpId { get; set; }
    public string? MgrpName { get; set; }
    public int? SgrpId { get; set; }
    public string? SgrpName { get; set; }
    public int? SerId { get; set; }
    public string? SerName { get; set; }
    public int? Qty { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
    public decimal? DisAmt { get; set; }
    public decimal? NetAmt { get; set; }
    public int? UserId { get; set; }
    public int? RefById { get; set; }
    public string? RefBy { get; set; }
    public int? UnderServiceId { get; set; }
    public string? PackageName { get; set; }
    public string? CreatedBy { get; set; }
    public int? OutSourced { get; set; }
    public int? OSAccountId { get; set; }
    // OrgLogo intentionally skipped — binary, not needed in API
}
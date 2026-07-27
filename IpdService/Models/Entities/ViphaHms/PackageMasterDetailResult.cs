namespace IpdService.Models.Entities.ViphaHms;

public class PackageMasterDetailResult
{
    public short Sno { get; set; }
    public string? Typ { get; set; }
    public int SerId { get; set; }
    public string? Ser_Name { get; set; }
    public decimal ActualRate { get; set; }
    public decimal PackageRate { get; set; }
    public decimal ShareAmt { get; set; }
}
namespace IpdService.Models.DTOs;

public class IpdPackageDetailsDto
{
    public long OrgId { get; set; }
    public int Id { get; set; }
    public string? BillNo { get; set; }
    public int SerId { get; set; }
    public string? SerName { get; set; }
    public string? Typ { get; set; }
}
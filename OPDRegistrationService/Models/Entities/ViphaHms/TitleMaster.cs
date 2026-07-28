namespace OPDRegistrationService.Models.Entities.Hms;

public class TitleMaster
{
    public int Id { get; set; }
    public string? Title_Name { get; set; }
    public string? Title_Status { get; set; }
    public string? Gender { get; set; }
    public bool? Def_status { get; set; }
    public int OrgId { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastModifiedby { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
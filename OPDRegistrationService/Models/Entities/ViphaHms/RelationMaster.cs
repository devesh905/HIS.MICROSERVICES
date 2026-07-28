namespace OPDRegistrationService.Models.Entities.Hms;

public class RelationMaster
{
    public int? Id { get; set; }
    public string? Descript { get; set; }
    public string? Gender { get; set; }
    public int? TitleId { get; set; }
    public bool? IsActive { get; set; }
    public int OrgId { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastModifiedby { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
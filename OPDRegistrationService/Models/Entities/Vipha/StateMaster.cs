using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPDRegistrationService.Models.Entities.Vipha;

[Table("StateMaster")]
public class StateMaster
{
    [Key] public int Id { get; set; }
    public int CountryId { get; set; }
    [Required] public string StateName { get; set; } = "";
    public int? GSTCode { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool? IsActive { get; set; }
}
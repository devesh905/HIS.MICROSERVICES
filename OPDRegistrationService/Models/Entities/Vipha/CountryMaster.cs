using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPDRegistrationService.Models.Entities.Vipha;

[Table("CountryMaster")]
public class CountryMaster
{
    [Key] public int Id { get; set; }
    public string? ShortName { get; set; }
    [Required] public string Name { get; set; } = "";
    public string? Code { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool? IsActive { get; set; }
}
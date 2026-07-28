using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPDRegistrationService.Models.Entities.Vipha;

[Table("CityMaster")]
public class CityMaster
{
    [Key] public int Id { get; set; }
    public int StateId { get; set; }
    public string? CityName { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool? IsActive { get; set; }
}
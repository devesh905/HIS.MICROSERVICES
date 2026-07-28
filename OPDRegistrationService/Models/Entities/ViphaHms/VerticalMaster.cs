using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPDRegistrationService.Models.Entities.Hms;

[Table("Vertical_Master", Schema = "dbo")]
public class VerticalMaster
{
    [Key] public int Id { get; set; }
    public string? Vertical_Name { get; set; }
    public int OrgId { get; set; }
    public bool? IsActive { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPDRegistrationService.Models.Entities.Hms;

[Table("Opd_Type")]
public class OpdType
{
    [Key]
    public int Id { get; set; }

    public string? Descript { get; set; }

    public bool? IsActive { get; set; }

    [Column("Def_status")]
    public bool? DefStatus { get; set; }

    public int? OrgId { get; set; }

    public string? SystemName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    [Column("LastModifiedby")]
    public int? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}
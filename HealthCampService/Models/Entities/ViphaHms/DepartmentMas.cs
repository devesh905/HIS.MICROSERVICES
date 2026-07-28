using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCampMicroservice.Models.Entities.Hms;

[Table("Department_Mas", Schema = "dbo")]
public class DepartmentMas
{
    [Key] public int Id { get; set; }
    public string? Dept_Name { get; set; }
    public long OrgId { get; set; }
    public bool? IsActive { get; set; }
}
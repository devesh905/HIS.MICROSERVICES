using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCampMicroservice.Models.Entities.Lis;

[Table("Test_Mas")]
public class TestMas
{
    [Key] public int Id { get; set; }
    public string? Test_Name { get; set; }
    public string? Test_Code { get; set; }
    public string? ReportFormat { get; set; }  // "T" tabular / "H" html ?
    public string? Res_Footer { get; set; }    // ntext
    public string? Typ { get; set; }          // "L" = lab
    public bool? Print_In_Res { get; set; }
    public bool? IsNablLogo { get; set; }
}
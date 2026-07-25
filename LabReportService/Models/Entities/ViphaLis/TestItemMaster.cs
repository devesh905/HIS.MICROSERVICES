using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabReportService.Models.Entities.ViphaLis;

[Table("TestItem_Master")]
public class TestItemMaster
{
    [Key] public int Id { get; set; }
    public string? TestItem_Name { get; set; }
    public string? Uom { get; set; }
    public string? Method { get; set; }
    public string? Ref_Range_Intext { get; set; }  // e.g. "< 0.15", "Negative"
    public bool? IsNumeric_Res { get; set; }
    public int? Decimal_Place { get; set; }
    public string? Male_From_Range { get; set; }
    public string? Male_To_Range { get; set; }
    public string? Female_From_Range { get; set; }
    public string? Female_To_Range { get; set; }
    public string? Child_From_Range { get; set; }
    public string? Child_To_Range { get; set; }
    public string? Gen_From_Range { get; set; }
    public string? Gen_To_Range { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabReportService.Models.Entities.ViphaLis;

[Table("Result_Det")]
public class ResultDet
{
    [Key] public long Id { get; set; }
    public long? BId { get; set; }
    public string? Doc_No { get; set; }
    public DateTime? Doc_Date { get; set; }
    public short? Sno { get; set; }
    public int? TestId { get; set; }
    public int? TestItemId { get; set; }
    public string? Result { get; set; }   // "13.3", "NEGATIVE", etc.
    public string? Unit { get; set; }
    public string? Method { get; set; }
    public string? RangeFrom { get; set; }
    public string? RangeTo { get; set; }
    public string? RowColor { get; set; } // "OutOfRange" / "NormalRange"
    public string? PreResult1 { get; set; }
    public string? PreResult2 { get; set; }
}
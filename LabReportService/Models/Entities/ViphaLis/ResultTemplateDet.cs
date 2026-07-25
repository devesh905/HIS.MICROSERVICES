using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabReportService.Models.Entities.ViphaLis;

[Table("Result_TemplateDet")]
public class ResultTemplateDet
{
    [Key] public int Id { get; set; }
    public long? BId { get; set; }
    public int? TestId { get; set; }
    public int? TemplateId { get; set; }
    public string? TemplateDesc { get; set; }  // raw HTML
    public string? Doc_No { get; set; }
    public DateTime? Doc_Date { get; set; }
}
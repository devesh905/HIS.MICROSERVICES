using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabReportService.Models.Entities.ViphaLis;

[Table("Result_Mas")]
public class ResultMas
{
    [Key] public long Id { get; set; }
    public long? BId { get; set; }        // FK → InvestBookingMas.Id
    public string? Doc_No { get; set; }   // e.g. RES/1/22
    public DateTime? Doc_Date { get; set; }
    public string? PatientName { get; set; }
    public string? OpdNo { get; set; }
    public string? AdmNo { get; set; }
    public string? Barcode { get; set; }
    public int? Diagnosis { get; set; }
    public string? PrintRemarks { get; set; }
    public string? Remarks { get; set; }
}
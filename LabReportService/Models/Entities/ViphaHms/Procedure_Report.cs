using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabReportService.Models.Entities.ViphaHms;

[Table("Procedure_Report")]
public class ProcedureReport
{
    [Key]
    public long Id { get; set; }
    public string? Report_No { get; set; }
    public DateTime? Report_Date { get; set; }
    public string? Report_Time { get; set; }
    public string? Report_Mode { get; set; }
    public string? Ref_Doc_No { get; set; }
    public DateTime? Ref_Doc_DateTime { get; set; }
    public string? UHIDNo { get; set; }
    public string? Adm_No { get; set; }
    public int? ServiceId { get; set; }
    public string? ServiceTitle { get; set; }
    public int? TemplateId { get; set; }
    public string? Rep_Result { get; set; }   // ntext → string
    public string? Impression { get; set; }
    public int? DocId { get; set; }
    public int? RepDocId { get; set; }
    public string? T_Status { get; set; }
    public int? VerfiedBy { get; set; }
    public int? OrgId { get; set; }
    public int? UserId { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastModifiedby { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? RoomNo { get; set; }
    public string? AckDocNo { get; set; }
    public int? ValidateById { get; set; }
    public DateTime? ValidateAt { get; set; }
    public int? AuthorizedById { get; set; }
    public DateTime? AuthorizeAt { get; set; }
    public int? DiagnosisId { get; set; }
    public int? AuthDoctorId { get; set; }
    public string? Remarks { get; set; }
    public int? Ac_Jr_Id { get; set; }
    public string? Instruction { get; set; }
    public string? Justification { get; set; }
    public string? Notes { get; set; }
    public string? ClinicalNotes { get; set; }
    public string? HandOverTo { get; set; }
    public string? HandOverToMobileNo { get; set; }
    public int? HandOverById { get; set; }
    public DateTime? HandOverAt { get; set; }
    public string? HandOverSystemName { get; set; }
    public string? HandOverRptTyp { get; set; }
    public bool? IsCritical { get; set; }
    public string? LastImpression { get; set; }
}
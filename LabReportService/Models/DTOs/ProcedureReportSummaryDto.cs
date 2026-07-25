namespace LabReportService.Models.DTOs;

public class ProcedureReportSummaryDto
{
    public long Id { get; set; }
    public string? ReportNo { get; set; }
    public DateTime? ReportDate { get; set; }
    public string? ReportTime { get; set; }
    public string? ServiceTitle { get; set; }
    public string? Impression { get; set; }
    public string? Remarks { get; set; }
    public string? UhidNo { get; set; }
    public string? RoomNo { get; set; }
    public string? HandOverTo { get; set; }
    public bool? IsCritical { get; set; }
}

public class ProcedureReportDetailDto
{
    public long Id { get; set; }
    public string? ReportNo { get; set; }
    public DateTime? ReportDate { get; set; }
    public string? ReportTime { get; set; }
    public string? ServiceTitle { get; set; }
    public string? Impression { get; set; }
    public string? LastImpression { get; set; }
    public string? RepResult { get; set; }  // HTML body
    public string? Remarks { get; set; }
    public string? ClinicalNotes { get; set; }
    public string? Instruction { get; set; }
    public string? Justification { get; set; }
    public string? Notes { get; set; }
    public string? UhidNo { get; set; }
    public string? RoomNo { get; set; }
    public string? HandOverTo { get; set; }
    public string? HandOverToMobileNo { get; set; }
    public DateTime? HandOverAt { get; set; }
    public bool? IsCritical { get; set; }
    public string? ReportMode { get; set; }

    public string? AckDocNo { get; set; }
    public DateTime? RefDocDateTime { get; set; }
    public string? PatientName { get; set; }
    public string? PatientTitle { get; set; }
    public string? Gender { get; set; }
    public int? AgeInYears { get; set; }
    public string? Address { get; set; }
    public string? RefDoctorName { get; set; }
    public string? RefDoctorQual { get; set; }
    public string? AuthDoctorName { get; set; }
    public string? AuthDoctorQual { get; set; }
    public string? RepDoctorName { get; set; }

    public string? RefDocNo { get; set; }
}
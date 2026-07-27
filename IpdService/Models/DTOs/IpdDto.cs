namespace IpdService.Models.DTOs;
public class IpdSummaryDto
{
    public string? UhidNo { get; set; }
    public string? Adm_No { get; set; }
    public string? OpNo { get; set; }
    public DateTime? Adm_Date { get; set; }
    public string? Timein { get; set; }
    public string? Adm_Type { get; set; }       // Normal / Emergency
    public string? Patient_Type { get; set; }   // GENERAL / CASULTY etc.
    public string? IpdType { get; set; }
    public bool? IsEmergencyAdm { get; set; }
    public string? Pro_Diagnos { get; set; }    // Provisional diagnosis
    public string? Surgery_Name { get; set; }
    public string? Relieve_Status { get; set; } // blank = still admitted
    public DateTime? DOM { get; set; }          // Date of discharge
    public string? DoctorName { get; set; }
    public string? DoctorQualification { get; set; }
    public string? SponsorName { get; set; }
    public string? Cur_Bed { get; set; }
    public string? HospitalKey { get; set; }
    public string? HospitalName { get; set; }

    // Discharge info
    public string? DischargeDocNo { get; set; }
    public DateTime? DischargeDate { get; set; }
    public string? DischargeTime { get; set; }
    public string? DischargeMode { get; set; }
    public string? PatientCondition { get; set; }
    public string? DischargeStatus { get; set; }
    public bool IsDischarge { get; set; }
}

/// Full detail for a single admission.
public class IpdDetailDto : IpdSummaryDto
{
    public string? Adm_Mode { get; set; }
    public string? Ref_Type { get; set; }
    public string? MLCNO { get; set; }
    public bool? IsMlc { get; set; }
    public bool? Accidental_Status { get; set; }
    public bool? Package_Status { get; set; }
    public string? Policy_No { get; set; }
    public string? ClaimId { get; set; }
    public string? MStatus { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public short? AgeInYears { get; set; }
    public short? AgeInMonths { get; set; }
    public short? AgeInDays { get; set; }
    public string? Mother_Name { get; set; }
    public string? Remarks { get; set; }
    public string? AbhaNo { get; set; }
    public string? modepatient { get; set; }
    public string? KnowTo { get; set; }
}
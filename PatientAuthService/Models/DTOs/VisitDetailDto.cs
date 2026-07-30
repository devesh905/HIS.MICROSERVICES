namespace PatientAuthService.Models.DTOs;

public class VisitDetailDto
{
    public string? OpNo { get; set; }
    public string? UhidNo { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public DateTime? VisitDate { get; set; }    
    public string? RegistrationTime { get; set; }
    public string? OpdType { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorQualification { get; set; }
    public string? Department { get; set; }
    public string? SponsorName { get; set; }
    public decimal? TotalAmt { get; set; }
    public decimal? ConsultCharge { get; set; }
    public decimal? DisAmt { get; set; }
    public string? ReceiptMode { get; set; }
    public string? AppointmentNo { get; set; }
    public string? AppointmentTime { get; set; }
    public short? VisitNo { get; set; }         
    public string? VisitSource { get; set; }    //  "Registration" | "Consultancy"

    public string HospitalKey { get; set; } = "";   // "MRT" / "DDN"
    public string HospitalName { get; set; } = "";   // "Meerut" / "Dehradun"
}
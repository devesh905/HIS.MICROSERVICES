namespace PatientAuthService.Models.DTOs;

public class PatientReportDto
{
    // Identity
    public string? UhidNo { get; set; }
    public string? PatientName { get; set; }
    public string? Title { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public short? AgeInYears { get; set; }
    public string? BloodGroup { get; set; }

    // Contact
    public string? MobileNo { get; set; }
    public string? AltMobileNo { get; set; }
    public string? EmailId { get; set; }
    public string? Address { get; set; }
    public string? Town { get; set; }
    public string? District { get; set; }
    public string? PinCode { get; set; }
    public string? Nationality { get; set; }

    // Family
    public string? RelTitle { get; set; }
    public string? RelativeName { get; set; }
    public string? MotherName { get; set; }
    public string? Religion { get; set; }

    // ID Proof
    public string? IDProofType { get; set; }
    public string? IDProofNo { get; set; }
    public string? AdharNo { get; set; }

    // Visit History
    public List<VisitDetailDto> Visits { get; set; } = new();
    public int TotalVisits { get; set; }
    public DateTime? FirstVisit { get; set; }
    public DateTime? LastVisit { get; set; }

    public string HospitalKey { get; set; } = "";   // "MRT" / "DDN"
    public string HospitalName { get; set; } = "";   // "Meerut" / "Dehradun"
}
namespace OPDRegistrationService.Models.DTOs;

public class PatientDto
{
    public string? UhidNo { get; set; }
    public string? OpNo { get; set; }
    public string? Title { get; set; }
    public string? PatientName { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public short? AgeInYears { get; set; }  
    public string? BloodGroup { get; set; }
    public string? MobileNo { get; set; }
    public string? EmailId { get; set; }
    public string? Address { get; set; }
    public string? Town { get; set; }
    public string? Country { get; set; }    
    public string? District { get; set; }
    public int? State { get; set; }
    public int? City { get; set; }
    public string? OpdType { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public string? RelativeName { get; set; }
    public string? RelTitle { get; set; }
    public string? Nationality { get; set; }
    public string? AdharNo { get; set; }
    public string HospitalKey { get; set; } = "";   // "MRT" / "DDN"
    public string HospitalName { get; set; } = "";   // "Meerut" / "Dehradun"
}

public class AuthResponse
{
    public string Token { get; set; } = "";
    public PatientDto Patient { get; set; } = new();
}
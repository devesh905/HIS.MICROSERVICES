namespace EmployeeAuthService.Models.DTOs;

public record SendEmployeeOtpRequest(string UserName);

public record VerifyEmployeeOtpRequest(string UserName, string Otp);

public class EmployeeDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmpCode { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Designation { get; set; }
    public int? DeptId { get; set; }
    public bool IsSysAdmin { get; set; }
    public bool IsSysSubAdmin { get; set; }
    public DateTime? DateOfJoining { get; set; }
    public string? HospitalKey { get; set; }    
    public string? HospitalName { get; set; }   
}

public record SelectEmployeeHospitalRequest(string UserName, string HospitalKey);
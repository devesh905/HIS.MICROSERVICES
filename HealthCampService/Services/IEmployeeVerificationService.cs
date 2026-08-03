
namespace HealthCampService.Services;

public interface IEmployeeVerificationService
{
    Task<EmployeeVerificationResult> VerifyByMobileAsync(string mobileNo, CancellationToken ct = default);
}

public record EmployeeVerificationResult(
    bool IsEmployee,
    string? EmployeeCode,
    string? EmployeeName,
    string? DesignationName,
    string? StatusName);
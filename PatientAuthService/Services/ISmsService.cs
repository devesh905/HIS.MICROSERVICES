namespace PatientAuthService.Services;

public interface ISmsService
{
    Task SendAsync(string mobileNumber, string message);
}
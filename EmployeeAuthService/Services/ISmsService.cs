namespace EmployeeAuthService.Services;

public interface ISmsService
{
    Task SendAsync(string mobileNumber, string message);
}
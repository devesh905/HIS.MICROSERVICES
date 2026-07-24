namespace PatientAuthService.Services;

public interface IOtpService
{
    Task<string> GenerateAsync(string mobile);
    Task<bool> VerifyAsync(string mobile, string otp);
}
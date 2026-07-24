namespace PatientAuthService.Models.DTOs;

public record VerifyOtpRequest(string Mobile, string Otp);
using PatientAuthService.Models.Entities.ViphaHms;

namespace PatientAuthService.Services;

public interface IJwtService
{
    string GenerateToken(PatientRegistration patient);

    /// Preferred overload. Pass the hospital key so the JWT carries it
    /// as a claim — required by GET /api/patient/me.
    string GenerateToken(PatientRegistration patient, string hospitalKey);

    string GenerateGuestToken(string mobile);

}
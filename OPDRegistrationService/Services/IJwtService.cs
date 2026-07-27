using OPDRegistrationService.Models.Entities.Hms;

namespace OPDRegistrationService.Services;

public interface IJwtService
{
    string GenerateToken(PatientRegistration patient, string hospitalKey);
}
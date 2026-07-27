using OPDRegistrationService.Data;

namespace OPDRegistrationService.Services;

public interface IHospitalQueryService
{
    HmsDbContext CreateHmsContext(string connectionStringName);
}
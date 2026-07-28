using HealthCampService.Data;

namespace HealthCampService.Services;

public interface IHospitalQueryService
{
    HmsDbContext CreateHisContext(string connectionStringName);
    LisDbContext CreateLisContext(string connectionStringName);
    ViphaDbContext CreateViphaContext(string connectionStringName);
}

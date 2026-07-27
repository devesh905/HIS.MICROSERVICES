using IpdService.Data;

namespace IpdService.Services;

public interface IHospitalQueryService
{
    HmsDbContext CreateHisContext(string connectionStringName);
    LisDbContext CreateLisContext(string connectionStringName);
    ViphaDbContext CreateViphaContext(string connectionStringName);
}
using BillingService.Data;

namespace BillingService.Services;

public interface IHospitalQueryService
{
    HmsDbContext CreateHmsContext(string csName);
    LisDbContext CreateLisContext(string csName);
    ViphaDbContext CreateViphaContext(string csName);
}
using EmployeeAuthService.Data;

namespace EmployeeAuthService.Services;

public interface IHospitalQueryService
{

    ViphaDbContext CreateViphaContext(string csName);

    Task<List<HospitalResult<T>>> QueryAllViphaAsync<T>(
  Func<ViphaDbContext, HospitalRegistry.HospitalEntry, Task<T>> query);
}

public record HospitalResult<T>(string Key, string Name, T Result);
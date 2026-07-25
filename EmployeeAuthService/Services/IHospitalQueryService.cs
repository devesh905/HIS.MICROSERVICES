using EmployeeAuthService.Data;

namespace EmployeeAuthService.Services;

public interface IHospitalQueryService
{
    ViphaDbContext CreateHisContext(string connectionStringName);

    Task<List<HospitalResult<T>>> QueryAllAsync<T>(
        Func<ViphaDbContext, Task<T>> query);

    Task<List<HospitalResult<T>>> QueryAllAsync<T>(
        Func<ViphaDbContext, HospitalRegistry.HospitalEntry, Task<T>> query);

    ViphaDbContext CreateViphaContext(string csName);

    Task<List<HospitalResult<T>>> QueryAllViphaAsync<T>(
  Func<ViphaDbContext, HospitalRegistry.HospitalEntry, Task<T>> query);
}

public record HospitalResult<T>(string Key, string Name, T Result);
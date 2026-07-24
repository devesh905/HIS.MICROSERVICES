using PatientAuthService.Data;

namespace PatientAuthService.Services;

public interface IHospitalQueryService
{
    AuthDbContext CreateHisContext(string connectionStringName);

    Task<List<HospitalResult<T>>> QueryAllAsync<T>(
        Func<AuthDbContext, Task<T>> query);

    Task<List<HospitalResult<T>>> QueryAllAsync<T>(
        Func<AuthDbContext, HospitalRegistry.HospitalEntry, Task<T>> query);
}

public record HospitalResult<T>(string Key, string Name, T Result);
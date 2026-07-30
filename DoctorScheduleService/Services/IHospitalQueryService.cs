using DoctorScheduleService.Data;

namespace DoctorScheduleService.Services;

public interface IHospitalQueryService
{
    HmsDbContext CreateHmsContext(string connectionStringName);

    Task<List<HospitalResult<T>>> QueryAllAsync<T>(
        Func<HmsDbContext, Task<T>> query);

    Task<List<HospitalResult<T>>> QueryAllAsync<T>(
        Func<HmsDbContext, HospitalRegistry.HospitalEntry, Task<T>> query);
}

/// Wraps a query result with hospital identity.
public record HospitalResult<T>(
    string Key,
    string Name,
    T Result
);
using DoctorScheduleService.Data;

namespace DoctorScheduleService.Services;

public interface IHospitalQueryService
{
    /// Creates a HisDbContext pointing at the named connection string.
    /// Caller must dispose it (use 'await using').
    HisDbContext CreateHisContext(string connectionStringName);

    /// Creates a LisDbContext pointing at the named connection string.
    /// Caller must dispose it (use 'await using').
    LisDbContext CreateLisContext(string connectionStringName);

    ViphaDbContext CreateViphaContext(string csName);

    /// Runs the same query against ALL registered hospitals in parallel.
    /// Returns a list of (HospitalKey, HospitalName, TResult) — one entry per hospital.
    /// Hospitals where the query throws are skipped and logged (no crash).
    Task<List<HospitalResult<T>>> QueryAllAsync<T>(
        Func<HisDbContext, Task<T>> query);

    /// Same as QueryAllAsync but also passes the hospital entry so the query
    /// can use the hospital key/name if needed.
    Task<List<HospitalResult<T>>> QueryAllAsync<T>(
        Func<HisDbContext, HospitalRegistry.HospitalEntry, Task<T>> query);

    Task<List<HospitalResult<T>>> QueryAllViphaAsync<T>(
      Func<ViphaDbContext, HospitalRegistry.HospitalEntry, Task<T>> query);
}

/// Wraps a query result with hospital identity.
public record HospitalResult<T>(
    string Key,
    string Name,
    T Result
);
using HealthCampService.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthCampService.Services;

public static class HospitalRegistry
{
    public record HospitalEntry(string Key, string Name, string HisCs, string LisCs, string ViphaCs);

    public static readonly IReadOnlyList<HospitalEntry> All =
    [
        new("MRT", "Meerut",   "HMSDb_MRT", "LISDb_MRT", "ViphaDb_MRT"),
        new("DDN", "Dehradun", "HMSDb_Ddn", "LISDb_Ddn", "ViphaDb_Ddn"),
    ];

    public static HospitalEntry? Find(string key) =>
        All.FirstOrDefault(h => h.Key == key);
}

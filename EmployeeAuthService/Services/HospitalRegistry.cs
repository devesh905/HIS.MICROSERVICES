namespace EmployeeAuthService.Services;

public static class HospitalRegistry
{
    public record HospitalEntry(string Key, string Name, string ViphaCs);

    public static readonly IReadOnlyList<HospitalEntry> All =
    [
        new("MRT", "Meerut",   "ViphaDb_MRT"),
        new("DDN", "Dehradun", "ViphaDb_Ddn"),
    ];

    public static HospitalEntry? Find(string key) =>
        All.FirstOrDefault(h => h.Key == key);
}
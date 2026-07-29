namespace LabReportService.Services;

public static class HospitalRegistry
{
    public record HospitalEntry(string Key, string Name, string HmsCs, string LisCs);

    public static readonly IReadOnlyList<HospitalEntry> All =
    [
        new("MRT", "Meerut",   "HMSDb_MRT"  , "LISDb_MRT"),
        new("DDN", "Dehradun", "HMSDb_Ddn"  , "LISDb_Ddn"),
    ];

    public static HospitalEntry? Find(string key) =>
        All.FirstOrDefault(h => h.Key == key);
}
namespace LabReportService.Services;

public static class HospitalRegistry
{
    public record HospitalEntry(string Key, string Name, string HisCs, string LisCs);

    public static readonly IReadOnlyList<HospitalEntry> All =
    [
        new("MRT", "Meerut",   "HMSDb_MRT"  , "HISDb_MRT"),
        new("DDN", "Dehradun", "HMSDb_Ddn"  , "HISDb_Ddn"),
    ];

    public static HospitalEntry? Find(string key) =>
        All.FirstOrDefault(h => h.Key == key);
}
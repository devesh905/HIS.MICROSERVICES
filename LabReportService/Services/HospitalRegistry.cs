namespace LabReportService.Services;

public static class HospitalRegistry
{
    public record HospitalEntry(string Key, string Name, string HmsCs, string LisCs);

    public static readonly IReadOnlyList<HospitalEntry> All =
    [
        new("MRT", "Meerut",   "HmsDb_MRT"  , "LisDb_MRT"),
        new("DDN", "Dehradun", "HmsDb_Ddn"  , "LisDb_Ddn"),
    ];

    public static HospitalEntry? Find(string key) =>
        All.FirstOrDefault(h => h.Key == key);
}
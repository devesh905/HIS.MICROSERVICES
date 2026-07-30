namespace DoctorScheduleService.Services;
public static class HospitalRegistry
{
    public record HospitalEntry(
        string Key,     // short code used in JWT, localStorage, query params  e.g. "MRT"
        string Name,    // display name  e.g. "Meerut"
        string HmsCs  // connection-string key in appsettings.json for HMS DB
    );

    public static readonly IReadOnlyList<HospitalEntry> All =
    [
        new("MRT", "Meerut",    "HisDb"),
        new("DDN", "Dehradun",  "HisDb_Ddn"),
    ];

    public static HospitalEntry? Find(string key) =>
        All.FirstOrDefault(h => h.Key == key);
}
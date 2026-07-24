namespace BillingService;

public static class HospitalRegistry
{
    public record HospitalEntry(string Key, string Name, string HisCs, string LisCs, string ViphaCs);

    public static readonly IReadOnlyList<HospitalEntry> All =
    [
        new("MRT", "Meerut",   "HisDb",     "LisDb",     "ViphaDb"),
        new("DDN", "Dehradun", "HisDb_Ddn", "LisDb_Ddn", "ViphaDb_Ddn"),
    ];

    public static HospitalEntry? Find(string key) => All.FirstOrDefault(h => h.Key == key);
}
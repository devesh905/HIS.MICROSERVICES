using Microsoft.EntityFrameworkCore;
using LabReportService.Data;

namespace LabReportService.Services;

public class HospitalQueryService : IHospitalQueryService
{
    private readonly IConfiguration _config;
    public HospitalQueryService(IConfiguration config) => _config = config;

    public HmsDbContext CreateHmsContext (string csName)
    {
        var cs = _config.GetConnectionString(csName)
                 ?? throw new InvalidOperationException($"Connection string '{csName}' not found.");
        return new HmsDbContext(new DbContextOptionsBuilder<HmsDbContext>().UseSqlServer(cs).Options);
    }

    public LisDbContext CreateLisContext(string csName)
    {
        var cs = _config.GetConnectionString(csName)
                 ?? throw new InvalidOperationException($"Connection string '{csName}' not found.");
        return new LisDbContext(new DbContextOptionsBuilder<LisDbContext>().UseSqlServer(cs).Options);
    }
}
using Microsoft.EntityFrameworkCore;
using OPDRegistrationService.Data;

namespace OPDRegistrationService.Services;

public class HospitalQueryService : IHospitalQueryService
{
    private readonly IConfiguration _config;

    public HospitalQueryService(IConfiguration config) => _config = config;

    public HmsDbContext CreateHmsContext(string csName)
    {
        var cs = _config.GetConnectionString(csName)
                 ?? throw new InvalidOperationException($"Connection string '{csName}' not found.");
        var opts = new DbContextOptionsBuilder<HmsDbContext>().UseSqlServer(cs).Options;
        return new HmsDbContext(opts);
    }
}
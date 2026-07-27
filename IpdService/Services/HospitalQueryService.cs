using Microsoft.EntityFrameworkCore;
using IpdService.Data;

namespace IpdService.Services;

public class HospitalQueryService : IHospitalQueryService
{
    private readonly IConfiguration _config;

    public HospitalQueryService(IConfiguration config) => _config = config;

    public HmsDbContext CreateHisContext(string csName)
    {
        var cs = _config.GetConnectionString(csName)
                 ?? throw new InvalidOperationException($"Connection string '{csName}' not found.");
        var opts = new DbContextOptionsBuilder<HmsDbContext>().UseSqlServer(cs).Options;
        return new HmsDbContext(opts);
    }

    public LisDbContext CreateLisContext(string csName) { /* same pattern */ }
    public ViphaDbContext CreateViphaContext(string csName) { /* same pattern */ }
}
using LabReportService.Data;
using Microsoft.EntityFrameworkCore;

namespace LabReportService.Services;

public interface IHospitalQueryService
{
    LisDbContext CreateLisContext(string connectionString);
}

public class HospitalQueryService : IHospitalQueryService
{
    public LisDbContext CreateLisContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<LisDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new LisDbContext(options);
    }
}
using LabReportService.Data;

namespace LabReportService.Services;

public interface IHospitalQueryService
{
    HmsDbContext CreateHmsContext(string csName);
    LisDbContext CreateLisContext(string csName);
}
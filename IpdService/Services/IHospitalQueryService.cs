namespace IpdService.Services;

public interface IHospitalQueryService
{
    HisDbContext CreateHisContext(string connectionStringName);
    LisDbContext CreateLisContext(string connectionStringName);
    ViphaDbContext CreateViphaContext(string connectionStringName);
}
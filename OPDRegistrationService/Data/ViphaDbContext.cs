using OPDRegistrationService.Models.Entities;
using OPDRegistrationService.Models.Entities.Vipha;
using Microsoft.EntityFrameworkCore;

namespace OPDRegistrationService.Data;

public class ViphaDbContext : DbContext
{
    public ViphaDbContext(DbContextOptions<ViphaDbContext> options) : base(options) { }

    public DbSet<StateMaster> StateMasters { get; set; }
    public DbSet<CityMaster> CityMasters { get; set; }
    public DbSet<CountryMaster> CountryMasters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }
}
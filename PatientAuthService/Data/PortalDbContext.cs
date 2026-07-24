using Microsoft.EntityFrameworkCore;
using PatientAuthService.Models.Entities.PatientPortalDb;

namespace PatientAuthService.Data;

public class PortalDbContext : DbContext
{
    public PortalDbContext(DbContextOptions<PortalDbContext> options) : base(options) { }

    public DbSet<LoginActivityLog> LoginActivityLogs => Set<LoginActivityLog>();

}
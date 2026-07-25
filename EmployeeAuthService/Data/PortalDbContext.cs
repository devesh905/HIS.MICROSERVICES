using EmployeeAuthService.Models.Entities.Portal;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAuthService.Data;

public class PortalDbContext : DbContext
{
    public PortalDbContext(DbContextOptions<PortalDbContext> options) : base(options) { }

    public DbSet<LoginActivityLog> LoginActivityLogs => Set<LoginActivityLog>();
}
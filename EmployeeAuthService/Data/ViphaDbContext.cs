using EmployeeAuthService.Models.Entities.Vipha;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EmployeeAuthService.Data;

public class ViphaDbContext : DbContext
{
    public ViphaDbContext(DbContextOptions<ViphaDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .ToTable("Users", schema: "dbo"); 
    }
}
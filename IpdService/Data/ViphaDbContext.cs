using IpdService.Models.Entities.Vipha;
using Microsoft.EntityFrameworkCore;

namespace IpdService.Data;

public class ViphaDbContext : DbContext
{
    public ViphaDbContext(DbContextOptions<ViphaDbContext> options) : base(options) { }

    public DbSet<RetailMas> RetailMas => Set<RetailMas>();
    public DbSet<RetailReturnMas> RetailReturnMas => Set<RetailReturnMas>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RetailMas>()
            .ToTable("Retail_Mas", schema: "dbo");
        modelBuilder.Entity<RetailMas>()
            .HasIndex(r => r.IPDNo);
        modelBuilder.Entity<RetailMas>()
            .HasIndex(r => r.UHIDNo);

        modelBuilder.Entity<RetailReturnMas>()
            .ToTable("RetailReturn_Mas", schema: "dbo");
        modelBuilder.Entity<RetailReturnMas>()
            .HasIndex(r => r.IPDNo);
    }
}
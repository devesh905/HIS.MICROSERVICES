using BillingService.Models.Entities.ViphaLis;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Data;

public class LisDbContext : DbContext
{
    public LisDbContext(DbContextOptions<LisDbContext> options) : base(options) { }

    public DbSet<TestMas> TestMas => Set<TestMas>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestMas>()
            .ToTable("Test_Mas", schema: "dbo")
            .Property(t => t.Res_Footer)
            .HasColumnType("ntext");
    }
}
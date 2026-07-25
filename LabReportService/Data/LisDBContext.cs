using LabReportService.Models.Entities.ViphaLis;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace LabReportService.Data;

public class LisDbContext : DbContext
{
    public LisDbContext(DbContextOptions<LisDbContext> options) : base(options) { }

    public DbSet<InvestBookingMas> InvestBookingMas => Set<InvestBookingMas>();
    public DbSet<ResultMas> ResultMas => Set<ResultMas>();
    public DbSet<ResultDet> ResultDet => Set<ResultDet>();
    public DbSet<ResultTemplateDet> ResultTemplateDet => Set<ResultTemplateDet>();
    public DbSet<TestMas> TestMas => Set<TestMas>();
    public DbSet<TestItemMaster> TestItemMaster => Set<TestItemMaster>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InvestBookingMas>()
            .ToTable("Invest_Booking_Mas", schema: "dbo");
        modelBuilder.Entity<InvestBookingMas>()
            .HasIndex(i => i.UhidNo);

        modelBuilder.Entity<ResultMas>()
            .ToTable("Result_Mas", schema: "dbo");

        modelBuilder.Entity<ResultDet>()
            .ToTable("Result_Det", schema: "dbo");

        modelBuilder.Entity<ResultTemplateDet>()
            .ToTable("Result_TemplateDet", schema: "dbo")
            .Property(r => r.TemplateDesc)
            .HasColumnType("ntext");

        modelBuilder.Entity<TestMas>()
            .ToTable("Test_Mas", schema: "dbo")
            .Property(t => t.Res_Footer)
            .HasColumnType("ntext");

        modelBuilder.Entity<TestItemMaster>()
            .ToTable("TestItem_Master", schema: "dbo");
    }
}
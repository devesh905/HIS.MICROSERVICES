using HealthCampMicroservice.Models.Entities.Portal;
using Microsoft.EntityFrameworkCore;

namespace HealthCampMicroservice.Data;

public class PortalDbContext : DbContext
{
    public PortalDbContext(DbContextOptions<PortalDbContext> options) : base(options) { }

    public DbSet<HealthCampType> HealthCampTypes => Set<HealthCampType>();
    public DbSet<HealthCampSlot> HealthCampSlots => Set<HealthCampSlot>();
    public DbSet<HealthCampBooking> HealthCampBookings => Set<HealthCampBooking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HealthCampType>()
            .ToTable("HealthCampType");

        modelBuilder.Entity<HealthCampSlot>()
            .ToTable("HealthCampSlot");
        modelBuilder.Entity<HealthCampSlot>()
            .HasOne(s => s.CampType)
            .WithMany()
            .HasForeignKey(s => s.CampTypeId);

        modelBuilder.Entity<HealthCampBooking>()
            .ToTable("HealthCampBooking");
        modelBuilder.Entity<HealthCampBooking>()
            .HasOne(b => b.Slot)
            .WithMany()
            .HasForeignKey(b => b.SlotId);
        modelBuilder.Entity<HealthCampBooking>()
            .HasIndex(b => new { b.HospitalKey, b.BookingRefNo });
    }
}
using Microsoft.EntityFrameworkCore;
using DoctorScheduleService.Models.Entities.Portal;

namespace DoctorScheduleService.Data;

public class PortalDbContext : DbContext
{
    public PortalDbContext(DbContextOptions<PortalDbContext> options) : base(options) { }

    public DbSet<DoctorSchedule> DoctorSchedules { get; set; }

    public DbSet<HealthCampType> HealthCampTypes { get; set; }
    public DbSet<HealthCampSlot> HealthCampSlots { get; set; }
    public DbSet<LoginActivityLog> LoginActivityLogs { get; set; }

    public DbSet<HealthCampBooking> HealthCampBookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DoctorSchedule>(e =>
        {
            e.HasIndex(x => new { x.HospitalKey, x.ScheduleMonth, x.DayOfWeek, x.DepartmentId })
             .HasDatabaseName("IX_DoctorSchedule_Lookup");

            e.HasIndex(x => new { x.HospitalKey, x.DoctorId, x.ScheduleMonth })
             .HasDatabaseName("IX_DoctorSchedule_ByDoctor");
        });

        // HealthCampType
        modelBuilder.Entity<HealthCampType>(e =>
        {
            e.ToTable("HealthCampType");

            e.HasIndex(x => new { x.HospitalKey, x.Code })
             .IsUnique()
             .HasDatabaseName("UQ_HealthCampType_Code");
        });

        // HealthCampSlot
        modelBuilder.Entity<HealthCampSlot>(e =>
        {
            e.ToTable("HealthCampSlot");

            e.HasIndex(x => new { x.HospitalKey, x.CampTypeId, x.SlotDate, x.StartTime, x.EndTime })
             .IsUnique()
             .HasDatabaseName("UQ_HealthCampSlot_Window");

            e.HasIndex(x => new { x.HospitalKey, x.CampTypeId, x.SlotDate })
             .HasDatabaseName("IX_HealthCampSlot_Lookup");

            e.HasOne(x => x.CampType)
             .WithMany(x => x.Slots)
             .HasForeignKey(x => x.CampTypeId)
             .OnDelete(DeleteBehavior.Restrict);

            // Concurrency token protects BookedCount against race conditions
            // when two requests try to book the last seat simultaneously.
            e.Property(x => x.BookedCount).IsConcurrencyToken(false);
        });

        // HealthCampBooking
        modelBuilder.Entity<HealthCampBooking>(e =>
        {
            e.ToTable("HealthCampBooking");

            e.HasIndex(x => x.BookingRefNo)
             .IsUnique()
             .HasDatabaseName("UQ_HealthCampBooking_RefNo");

            e.HasIndex(x => new { x.SlotId, x.Status })
             .HasDatabaseName("IX_HealthCampBooking_Slot");

            e.HasIndex(x => new { x.HospitalKey, x.MobileNo, x.CampTypeId })
             .HasDatabaseName("IX_HealthCampBooking_Mobile");

            // Filtered unique index: one ACTIVE (Booked) booking per mobile per slot.
            // EF Core can't express filtered indexes purely via attributes,
            // so it's declared here with a raw SQL filter.
            e.HasIndex(x => new { x.HospitalKey, x.CampTypeId, x.SlotId, x.MobileNo })
             .IsUnique()
             .HasDatabaseName("UX_HealthCampBooking_NoDupe")
             .HasFilter("[Status] = 'Booked'");

            e.HasOne(x => x.Slot)
             .WithMany(x => x.Bookings)
             .HasForeignKey(x => x.SlotId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.CampType)
             .WithMany()
             .HasForeignKey(x => x.CampTypeId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
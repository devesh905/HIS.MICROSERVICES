using Microsoft.EntityFrameworkCore;
using DoctorScheduleService.Models.Entities.PatientPortal;

namespace DoctorScheduleService.Data;

public class PortalDbContext : DbContext
{
    public PortalDbContext(DbContextOptions<PortalDbContext> options) : base(options) { }

    public DbSet<DoctorSchedule> DoctorSchedules { get; set; }

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

    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorScheduleService.Models.Entities.PatientPortal;

[Table("DoctorSchedules")]
public class DoctorSchedule
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(10)]
    public string HospitalKey { get; set; } = "";   // MRT / DDN

    [Required]
    public int DoctorId { get; set; }               // Vipha DoctorMasters.Id (no FK — cross-DB)

    [Required]
    public int DepartmentId { get; set; }            // Vipha DepartmentMas.Id

    // 0 = Sunday ... 6 = Saturday (matches DateTime.DayOfWeek)
    [Required]
    public byte DayOfWeek { get; set; }

    [MaxLength(100)]
    public string? SlotLabel { get; set; }            // "8 AM to 12 Noon", "PVT OPD", etc.

    // Which month this schedule row belongs to, e.g. "2026-07"
    [Required, MaxLength(7)]
    public string ScheduleMonth { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
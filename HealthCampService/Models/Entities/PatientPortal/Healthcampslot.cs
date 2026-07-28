using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCampService.Models.Entities.PatientPortal;


// A bookable slot: specific date + time window + capacity for a given camp type.
// Event-based (not recurring like DoctorSchedule) - admin creates slots per camp.
[Table("HealthCampSlot")]
public class HealthCampSlot
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(10)]
    public string HospitalKey { get; set; } = string.Empty;

    [Required]
    public int CampTypeId { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateTime SlotDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    public int Capacity { get; set; }

    public int BookedCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;

    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }

    // Navigation
    [ForeignKey(nameof(CampTypeId))]
    public HealthCampType? CampType { get; set; }

    public ICollection<HealthCampBooking> Bookings { get; set; } = new List<HealthCampBooking>();

    // Convenience (not mapped) - useful in services/DTOs
    [NotMapped]
    public int AvailableCount => Capacity - BookedCount;

    [NotMapped]
    public bool IsFull => BookedCount >= Capacity;
}
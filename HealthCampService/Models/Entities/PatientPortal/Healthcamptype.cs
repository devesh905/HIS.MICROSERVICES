using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCampService.Models.Entities.PatientPortal;

// Master table for dynamic camp/test types (e.g. TMT, ECG, Master Health Checkup).
// New test types can be added anytime without code changes.
[Table("HealthCampType")]
public class HealthCampType
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(10)]
    public string HospitalKey { get; set; } = string.Empty; // 'MRT' / 'DDN'

    [Required, MaxLength(30)]
    public string Code { get; set; } = string.Empty; // 'TMT', 'ECG', 'MHC'

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty; // 'Free TMT Test'

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsEmployeeOnly { get; set; } = true;

    // Default Doctor/Department used when generating the OPD bill in Vipha
    // (HMS_Opd_Billing_Create @DocId/@DepId) for bookings of this camp type,
    // e.g. TMT -> Cardiology + assigned consultant. Same pattern as normal
    // OPD registration - just fixed per camp type instead of chosen by patient.
    public int? DefaultDoctorId { get; set; }
    public int? DefaultDepartmentId { get; set; }

    public bool IsActive { get; set; } = true;

    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;

    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }

    // Navigation
    public ICollection<HealthCampSlot> Slots { get; set; } = new List<HealthCampSlot>();
}
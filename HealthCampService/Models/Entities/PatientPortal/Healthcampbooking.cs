using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCampService.Models.Entities.PatientPortal;

public static class HealthCampBookingStatus
{
    public const string Booked = "Booked";
    public const string Cancelled = "Cancelled";
    public const string CheckedIn = "CheckedIn";
    public const string Completed = "Completed";
    public const string NoShow = "NoShow";
}

public static class HealthCampProcessingStatus
{
    public const string Pending = "Pending";     // waiting for visit-date job
    public const string Processed = "Processed"; // OpNo/BillNo generated in Vipha
    public const string Failed = "Failed";       // job ran but errored - needs manual look
}

// A patient/employee booking against a HealthCampSlot.
// This table lives ENTIRELY in PortalDb at booking time - no Vipha writes happen
// until the visit-date background job runs (see ProcessingStatus fields below).
[Table("HealthCampBooking")]
public class HealthCampBooking
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(10)]
    public string HospitalKey { get; set; } = string.Empty;

    [Required]
    public int SlotId { get; set; }

    [Required]
    public int CampTypeId { get; set; } // denormalized for reporting without join

    [Required, MaxLength(30)]
    public string BookingRefNo { get; set; } = string.Empty;

    // Identity: existing patient path
    public int? EmployeeId { get; set; }

    [MaxLength(30)]
    public string? UHID { get; set; } // set if booking against an existing UHID

    // Identity: new registration path
    public bool IsNewRegistration { get; set; } = false;

    /// Full OPD-style registration payload (JSON), mirroring buildPayload() from
    /// patient-opd-registration.js: title, name, DOB, address, contact, etc.
    /// Only populated when IsNewRegistration = true. Consumed by the visit-date
    /// job which calls HMS_PatientRegistration_Create with this data.
    public string? RegistrationPayloadJson { get; set; }

    //  Quick-access display/contact fields
    [Required, MaxLength(150)]
    public string PatientName { get; set; } = string.Empty;

    [Required, MaxLength(15)]
    public string MobileNo { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    public int? Age { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; } // free-text label, e.g. employee's own dept (not HMS DepId)

    // Doctor/Department for the eventual OPD billing
    public int? DoctorId { get; set; }   // DocId used in HMS_Opd_Billing_Create
    public int? DepartmentId { get; set; } // DepId used in HMS_Opd_Billing_Create

    // Booking lifecycle 
    [Required, MaxLength(20)]
    public string Status { get; set; } = HealthCampBookingStatus.Booked;

    public bool CardGenerated { get; set; } = false; // booking confirmation card, not OPD card
    public DateTime? CardSentOn { get; set; }

    [MaxLength(20)]
    public string? CardSentVia { get; set; } // SMS / WhatsApp / Email

    public DateTime? CancelledOn { get; set; }

    [MaxLength(300)]
    public string? CancelReason { get; set; }

    // Visit-date background job / Vipha integration
    [Required, MaxLength(20)]
    public string ProcessingStatus { get; set; } = HealthCampProcessingStatus.Pending;

    public DateTime? ProcessedOn { get; set; }

    [MaxLength(500)]
    public string? ProcessingError { get; set; }

    [MaxLength(30)]
    public string? ResultUhidNo { get; set; }

    [MaxLength(18)]
    public string? ResultOpNo { get; set; }

    [MaxLength(30)]
    public string? ResultBillNo { get; set; }

    //  Audit
    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;

    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }

    // Navigation
    [ForeignKey(nameof(SlotId))]
    public HealthCampSlot? Slot { get; set; }

    [ForeignKey(nameof(CampTypeId))]
    public HealthCampType? CampType { get; set; }
}
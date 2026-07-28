namespace HealthCampService.Models.DTOs;

// Camp Type
public class HealthCampTypeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEmployeeOnly { get; set; }

    // Default doctor/department for this camp type, if fixed
    // (e.g. TMT always goes under Cardiology / a specific consultant).
    // Frontend can prefill these; still overridable at booking time.
    public int? DefaultDoctorId { get; set; }
    public int? DefaultDepartmentId { get; set; }
}

// Slot availability (for calendar / slot picker UI)
public class SlotAvailabilityDto
{
    public int SlotId { get; set; }
    public DateTime SlotDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Capacity { get; set; }
    public int BookedCount { get; set; }
    public int AvailableCount { get; set; }
    public bool IsFull { get; set; }
    public bool IsPastTime { get; set; }
}

// New-registration payload (mirrors buildPayload() in patient-opd-registration.js)
// Used only when booking as a brand-new patient (no existing UHID selected).
// Stored as JSON on HealthCampBooking.RegistrationPayloadJson; consumed later
// by the visit-date job which feeds it into HMS_PatientRegistration_Create.
public class NewRegistrationPayload
{
    public string? Title { get; set; }
    public string PatientFirstName { get; set; } = string.Empty;
    public string PatientLastName { get; set; } = string.Empty;
    public string? RelTitle { get; set; }
    public string? RelativeName { get; set; }
    public string Gender { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public int AgeInYears { get; set; }
    public int AgeInMonths { get; set; }
    public int AgeInDays { get; set; }
    public string? BloodGroup { get; set; }
    public string Nationality { get; set; } = "Indian";
    public string MobileNo { get; set; } = string.Empty;
    public string? AltMobileNo { get; set; }
    public string? EmailId { get; set; }
    public string? AdharNo { get; set; }
    public int CountryId { get; set; } = 1;
    public int StateId { get; set; }
    public int CityId { get; set; }
    public string? Town { get; set; }
    public string? District { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? PinCode { get; set; }
}

// Create booking request
public class CreateBookingRequest
{
    public string HospitalKey { get; set; } = string.Empty;
    public int SlotId { get; set; }
    public int CampTypeId { get; set; }

    // Doctor/Department to use for the eventual OPD billing (HMS_Opd_Billing_Create).
    // Falls back to HealthCampType.DefaultDoctorId/DefaultDepartmentId if not provided.
    public int? DoctorId { get; set; }
    public int? DepartmentId { get; set; }

    // Path A: booking with an existing UHID 
    // Set IsNewRegistration = false and provide UHID (+ EmployeeId if known).
    // PatientName/MobileNo/etc. below are still required for quick-access display
    // even in this path (copied from the selected profile on the frontend).
    public bool IsNewRegistration { get; set; }

    public int? EmployeeId { get; set; }
    public string? UHID { get; set; }

    // Path B: new registration
    // Required only when IsNewRegistration = true.
    public NewRegistrationPayload? NewRegistration { get; set; }

    // Common quick-access fields (both paths)
    public string PatientName { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public int? Age { get; set; }
    public string? Department { get; set; } // free-text, e.g. employee's own dept
}

// Booking response
public class BookingResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public int? BookingId { get; set; }
    public string? BookingRefNo { get; set; }
    public DateTime? SlotDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}

// Admin: create slot(s) request
public class CreateSlotRequest
{
    public string HospitalKey { get; set; } = string.Empty;
    public int CampTypeId { get; set; }
    public DateTime SlotDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Capacity { get; set; }
}

// Booking list item (admin / reception view)
public class BookingListItemDto
{
    public int BookingId { get; set; }
    public string BookingRefNo { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public string? Department { get; set; }
    public DateTime SlotDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool CardGenerated { get; set; }

    public string ProcessingStatus { get; set; } = string.Empty;
    public string? ResultUhidNo { get; set; }
    public string? ResultOpNo { get; set; }
    public string? ResultBillNo { get; set; }
}
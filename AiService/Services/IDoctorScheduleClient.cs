namespace AiService.Services;

// Same pattern as HealthCampMicroservice.IDoctorScheduleClient — HTTP stand-in
// until DoctorScheduleService exists. Kept as a separate copy per-service
// (not Shared) since each service's DTO needs may diverge slightly; this one
// needs SlotLabel for the AI prompt text, HealthCamp's didn't.
// TODO: point BaseAddress at the real DoctorScheduleService once built.
public interface IDoctorScheduleClient
{
    Task<List<AvailableDoctorDto>> GetAvailableDoctorsAsync(string hospitalKey, int departmentId, DateTime date);
}

public record AvailableDoctorDto(int DoctorId, string? DoctorName, string? SlotLabel);
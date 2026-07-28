namespace HealthCampService.Services;

public interface IDoctorScheduleClient
{
    Task<List<AvailableDoctorDto>> GetAvailableDoctorsAsync(string hospitalKey, int departmentId, DateTime date);
}

public record AvailableDoctorDto(int DoctorId, string? DoctorName);
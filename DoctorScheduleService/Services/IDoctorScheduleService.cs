using DoctorScheduleService.Models.DTOs;

namespace DoctorScheduleService.Services;

public interface IDoctorScheduleService
{
    Task<List<AvailableDoctorDto>> GetAvailableDoctorsAsync(
        string hospitalKey, int departmentId, DateTime date);

    Task<List<DoctorScheduleDto>> GetScheduleForMonthAsync(
        string hospitalKey, string scheduleMonth);

    Task<int> AddScheduleAsync(DoctorScheduleCreateDto dto, int createdBy);

    Task<bool> DeleteScheduleAsync(int id);

    // for the admin management page
    Task<List<DoctorScheduleRowDto>> GetSchedulesAsync(
        string hospitalKey, string scheduleMonth,
        int? departmentId, int? doctorId, int? dayOfWeek);

    Task<bool> UpdateScheduleAsync(int id, DoctorScheduleUpdateDto dto);

    Task<(List<DoctorOptionDto> doctors, List<DepartmentOptionDto> departments)>
        GetDoctorAndDepartmentOptionsAsync(string hospitalKey);
}
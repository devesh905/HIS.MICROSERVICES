using System.Net.Http.Json;

namespace HealthCampService.Services;

public class DoctorScheduleClient : IDoctorScheduleClient
{
    private readonly HttpClient _http;
    private readonly ILogger<DoctorScheduleClient> _logger;

    public DoctorScheduleClient(HttpClient http, ILogger<DoctorScheduleClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<AvailableDoctorDto>> GetAvailableDoctorsAsync(
        string hospitalKey, int departmentId, DateTime date)
    {
        var url = $"api/doctor-schedules/available?hospitalKey={hospitalKey}&departmentId={departmentId}&date={date:yyyy-MM-dd}";

        try
        {
            var result = await _http.GetFromJsonAsync<List<AvailableDoctorDto>>(url);
            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DoctorScheduleService call failed for {Hospital}/{Dept}/{Date}",
                hospitalKey, departmentId, date);
            return [];
        }
    }
}
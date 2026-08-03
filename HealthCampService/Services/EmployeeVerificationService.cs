namespace HealthCampService.Services;

public class EmployeeVerificationService : IEmployeeVerificationService
{
    private readonly HttpClient _http;
    private readonly ILogger<EmployeeVerificationService> _logger;

    public EmployeeVerificationService(HttpClient http, ILogger<EmployeeVerificationService> logger)
    {
        _http = http; // base address configured in Program.cs: http://apiairtel.subharti.org
        _logger = logger;
    }

    public async Task<EmployeeVerificationResult> VerifyByMobileAsync(string mobileNo, CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.GetAsync(
                $"/api/v1/AppAttendance/GetEmployeeByRfCardNo?RfCardNo={mobileNo}&Flag=ByMobile", ct);

            if (!resp.IsSuccessStatusCode)
                return new EmployeeVerificationResult(false, null, null, null, null);

            var payload = await resp.Content.ReadFromJsonAsync<EmployeeApiResponse>(cancellationToken: ct);

            if (payload is not { Status: true, Data: not null })
                return new EmployeeVerificationResult(false, null, null, null, null);

            var isActive = string.Equals(payload.Data.StatusName, "Active", StringComparison.OrdinalIgnoreCase);

            return new EmployeeVerificationResult(
                IsEmployee: isActive,
                EmployeeCode: payload.Data.EmployeeCode,
                EmployeeName: payload.Data.EmployeeName,
                DesignationName: payload.Data.DesignationName,
                StatusName: payload.Data.StatusName);
        }
        catch (Exception ex)
        {
            // Fail closed for booking eligibility, but don't crash the caller
            _logger.LogWarning(ex, "Employee verification failed for mobile {Mobile}", mobileNo);
            return new EmployeeVerificationResult(false, null, null, null, null);
        }
    }

    private class EmployeeApiResponse
    {
        public bool Status { get; set; }
        public string? ErrorMessage { get; set; }
        public EmployeeData? Data { get; set; }
    }

    private class EmployeeData
    {
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? DesignationName { get; set; }
        public string? StatusName { get; set; }
    }
}
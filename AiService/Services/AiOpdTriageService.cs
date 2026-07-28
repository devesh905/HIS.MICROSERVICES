using AiService.Models.DTOs;
using AiService.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AiService.Services;

public interface IAiOpdTriageService
{
    Task<string> TriageAsync(AiOpdTriageRequest req);
}

public class AiOpdTriageService : IAiOpdTriageService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<AiOpdTriageService> _logger;
    private readonly IDoctorScheduleClient _scheduleClient;

    public AiOpdTriageService(
        HttpClient http,
        IConfiguration config,
        ILogger<AiOpdTriageService> logger,
        IDoctorScheduleClient scheduleClient)
    {
        _http = http;
        _config = config;
        _logger = logger;
        _scheduleClient = scheduleClient;
    }

    public async Task<string> TriageAsync(AiOpdTriageRequest req)
    {
        var apiKey = _config["Groq:ApiKey"];
        var model = _config["Groq:Model"] ?? "llama-3.3-70b-versatile";
        var url = _config["Groq:BaseUrl"] ?? "https://api.groq.com/openai/v1/chat/completions";

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Groq API key is not configured.");

        var visitDate = req.VisitDate ?? DateTime.Today;

        var deptAvailability = new Dictionary<int, List<AvailableDoctorDto>>();
        foreach (var dept in req.Departments.Take(50))
        {
            var available = await _scheduleClient.GetAvailableDoctorsAsync(req.HospitalKey, dept.Id, visitDate);
            deptAvailability[dept.Id] = available;
        }

        var hasAnyAvailability = deptAvailability.Values.Any(v => v.Count > 0);

        var deptList = string.Join("\n", req.Departments.Take(50).Select(d => $"{d.Id}:{d.Name}"));

        var doctorList = hasAnyAvailability
            ? string.Join("\n", deptAvailability
                .SelectMany(kv => kv.Value.Select(doc =>
                    $"{doc.DoctorId}:{doc.DoctorName}(dept:{kv.Key}, slot:{doc.SlotLabel})"))
                .Distinct())
            : "NONE — no doctor has a scheduled OPD slot in any department on this date.";

        const string jsonTemplate = """
{
  "suggestedDeptId": 12,
  "suggestedDeptName": "Orthopaedics",
  "suggestedDoctorId": 45,
  "suggestedDoctorName": "Dr. Rajesh Kumar",
  "reason": "One sentence plain-language explanation for the patient",
  "alternativeDepts": [
    { "deptId": 7, "deptName": "General Medicine", "reason": "If pain is not bone-related" }
  ],
  "noDoctorAvailable": false
}
""";

        var prompt = $"""
You are a helpful OPD triage assistant at an Indian multi-specialty hospital.
A patient has described their symptoms or problem below.

The visit date is: {visitDate:yyyy-MM-dd} ({visitDate:dddd})

IMPORTANT: Only suggest a doctor who is listed in AVAILABLE DOCTORS below.
These are the ONLY doctors with a scheduled OPD on this specific date.
Do NOT invent or assume any doctor outside this list.

PATIENT'S PROBLEM:
"{req.SymptomText}"

AVAILABLE DEPARTMENTS:
{deptList}

AVAILABLE DOCTORS (scheduled for {visitDate:yyyy-MM-dd} only):
{doctorList}

Return ONLY a valid JSON object. No markdown, no explanation, no preamble.
Use exactly this structure:

{jsonTemplate}

RULES:
- suggestedDeptId must be a valid Id from AVAILABLE DEPARTMENTS
- suggestedDoctorId MUST be picked from AVAILABLE DOCTORS only, matched to the correct department
- If AVAILABLE DOCTORS is "NONE", or no doctor exists for the matching department on this date,
  set suggestedDoctorId to 0, suggestedDoctorName to "", set noDoctorAvailable to true,
  and use "reason" to politely tell the patient no doctor is available for their issue on this date,
  and suggest they pick another date or visit General OPD if urgent.
- Otherwise set noDoctorAvailable to false
- alternativeDepts can be empty array if no good alternative exists
- reason must be in simple language the patient can understand (not medical jargon)
- If symptom is unclear, suggest General Medicine / General OPD (only if a doctor is available there)
- Respond in the same language the patient used (Hindi or English)
""";

        var body = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = "You are a hospital OPD triage assistant. Always respond with valid JSON only — no markdown, no explanation." },
                new { role = "user",   content = prompt }
            },
            max_tokens = 300,
            temperature = 0.2
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Groq triage error: {Status} | {Body}", response.StatusCode, raw);
            throw new Exception($"Groq API returned {response.StatusCode}: {raw}");
        }

        using var doc = JsonDocument.Parse(raw);
        return doc.RootElement
                  .GetProperty("choices")[0]
                  .GetProperty("message")
                  .GetProperty("content")
                  .GetString() ?? "{}";
    }
}
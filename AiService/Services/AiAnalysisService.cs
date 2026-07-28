using AiService.Models.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AiService.Services;

public class AiAnalysisService : IAiAnalysisService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<AiAnalysisService> _logger;

    public AiAnalysisService(
        HttpClient http,
        IConfiguration config,
        ILogger<AiAnalysisService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<string> AnalyzeBillAsync(AiBillAnalysisRequest req)
    {
        var apiKey = _config["Groq:ApiKey"];
        var model = _config["Groq:Model"] ?? "llama-3.3-70b-versatile";
        var url = _config["Groq:BaseUrl"] ?? "https://api.groq.com/openai/v1/chat/completions";

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Groq API key is not configured.");

        var servicesList = req.ExistingServices.Any()
            ? string.Join("\n", req.ExistingServices.Select(s =>
                $"  - {s.ServiceName} x{s.Quantity} = Rs.{s.Amount}"))
            : "  (No services billed yet)";

        const string jsonTemplate = """
{
  "observations": "2-3 sentence summary of the bill",
  "suggestions": [
    {
      "serviceName": "Example Service Name",
      "category": "Services",
      "frequency": "Daily",
      "estimatedAmount": 500,
      "reason": "Brief medical justification",
      "tpaClaimable": true
    }
  ],
  "tpaWarnings": ["Item that may cause TPA rejection"]
}
""";

        var dischargeNote = req.IsDischarge
            ? "YES — Patient is already discharged. Only suggest services that were missed or forgotten BEFORE discharge. Do NOT suggest ongoing daily charges."
            : "No, still admitted — suggest daily and periodic charges as appropriate.";

        var surgeryNote = string.IsNullOrWhiteSpace(req.SurgeryName) ? "None" : req.SurgeryName;
        var sponsorNote = string.IsNullOrWhiteSpace(req.SponsorName) ? "None / Self-pay" : req.SponsorName;

        var prompt = $"""
You are a senior hospital billing advisor for a multi-specialty hospital in India.
Analyze this IPD patient's bill and suggest ADDITIONAL billable services that are
medically justified, ethically sound, and not already present in the bill.

PATIENT DETAILS:
- Patient Name   : {req.PatientName}
- Age/Gender     : {req.AgeSex}
- Admission No   : {req.AdmNo}
- Admission Days : {req.AdmissionDays} days
- Doctor         : {req.DoctorName}
- Ward / Bed     : {req.WardBed}
- Department     : {req.Department}
- Diagnosis      : {req.Diagnosis}
- Surgery        : {surgeryNote}
- Sponsor / TPA  : {sponsorNote}
- Patient Type   : {req.PatientType}
- Admission Type : {req.AdmType}
- Is Discharged  : {dischargeNote}

FINANCIAL SUMMARY:
- Total Charges  : Rs.{req.TotalChargesSoFar:N2}
- Amount Received: Rs.{req.AmountReceived:N2}
- Balance Due    : Rs.{req.Balance:N2}

SERVICES ALREADY BILLED:
{servicesList}

YOUR TASK:
Return ONLY a valid JSON object — no explanation, no markdown fences, no preamble, no trailing text.
Use exactly this structure:

{jsonTemplate}

RULES:
- "category" must be exactly one of: Accommodation, Services, Laboratory, Pharmacy
- "estimatedAmount" must be a plain number in INR (no string, no currency symbol)
- "tpaClaimable" must be true or false (boolean)
- "frequency" examples: Daily, One-time, Per procedure, Weekly
- Suggest ONLY services NOT already in the billed list above
- Be specific to this patient's diagnosis, department, and admission days
- Do NOT inflate the bill — only medically legitimate charges
- If patient is discharged, only suggest missed/forgotten charges
- If diagnosis is unknown, infer from the services already billed
""";

        var body = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = "You are a hospital billing assistant for Indian multi-specialty hospitals. You always respond with valid JSON only — no markdown, no explanation." },
                new { role = "user",   content = prompt }
            },
            max_tokens = 2048,
            temperature = 0.3
        };

        var json = JsonSerializer.Serialize(body);

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Groq API error: {Status} | {Body}", response.StatusCode, raw);
            throw new Exception($"Groq API returned {response.StatusCode}: {raw}");
        }

        using var doc = JsonDocument.Parse(raw);
        var aiText = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

        return aiText ?? "No suggestion returned.";
    }
}
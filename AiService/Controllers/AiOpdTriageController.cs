using AiService.Models.DTOs;
using AiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiOpdTriageController : ControllerBase
{
    private readonly IAiOpdTriageService _ai;
    private readonly ILogger<AiOpdTriageController> _logger;

    public AiOpdTriageController(IAiOpdTriageService ai, ILogger<AiOpdTriageController> logger) 
    {
        _ai = ai;
        _logger = logger;
    }

    [HttpPost("suggest")]
    public async Task<IActionResult> Suggest([FromBody] AiOpdTriageRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SymptomText))
            return BadRequest(new { message = "Please describe your symptoms." });

        if (!req.Departments.Any())
            return BadRequest(new { message = "Department list is required." });

        if (string.IsNullOrWhiteSpace(req.HospitalKey))                    
            return BadRequest(new { message = "hospitalKey is required." });

        try
        {
            var raw = await _ai.TriageAsync(req);

            // validate AI returned parseable JSON before sending to client
            try
            {
                using var parsed = JsonDocument.Parse(raw);
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "AI triage returned non-JSON: {Raw}", raw);
                return StatusCode(502, new { message = "AI returned an unexpected response. Please try again." });
            }

            return Ok(new { suggestion = raw });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "AI triage config error");
            return StatusCode(500, new { message = "AI service not configured." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI triage failed: {Msg}", ex.Message);
            return StatusCode(500, new { message = "AI suggestion failed. Please try again shortly." });
        }
    }
}
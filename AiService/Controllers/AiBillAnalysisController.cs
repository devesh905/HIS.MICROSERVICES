using AiService.Models.DTOs;
using AiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "employee")]
public class AiBillAnalysisController : ControllerBase
{
    private readonly IAiAnalysisService _ai;
    private readonly ILogger<AiBillAnalysisController> _logger;

    public AiBillAnalysisController(
        IAiAnalysisService ai,
        ILogger<AiBillAnalysisController> logger)
    {
        _ai = ai;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] AiBillAnalysisRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.AdmNo))
            return BadRequest(new { message = "AdmNo is required." });

        try
        {
            var result = await _ai.AnalyzeBillAsync(req);
            return Ok(new { suggestion = result });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "AI config error for {AdmNo}", req.AdmNo);
            return StatusCode(500, new { message = "AI service is not configured. Please contact admin." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI analysis failed for {AdmNo}: {Msg}", req.AdmNo, ex.Message);
            return StatusCode(500, new { message = $"AI analysis failed: {ex.Message}" });
        }
    }

    [HttpGet("ping")]
    public IActionResult Ping([FromServices] IConfiguration config)
    {
        var key = config["Groq:ApiKey"];
        return Ok(new { hasKey = !string.IsNullOrEmpty(key), keyStart = key?[..8] });
    }
}
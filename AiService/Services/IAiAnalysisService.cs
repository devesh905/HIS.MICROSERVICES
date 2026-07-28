using AiService.Models.DTOs;

namespace AiService.Services;

public interface IAiAnalysisService
{
    Task<string> AnalyzeBillAsync(AiBillAnalysisRequest request);
}
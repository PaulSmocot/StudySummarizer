using Microsoft.AspNetCore.Mvc;
using StudySummarizer.Service.Dtos;
using StudySummarizer.Service.Interfaces;

namespace StudySummarizer.API.Controllers;

[ApiController]
[Route("api/documents/{documentId:guid}")]
public class SummariesController : ControllerBase
{
    private readonly ISummaryService _summaries;

    public SummariesController(ISummaryService summaries)
    {
        _summaries = summaries;
    }

    [HttpPost("summarize")]
    public async Task<ActionResult<SummaryResponse>> Summarize(Guid documentId, SummarizeRequest request)
    {
        return Ok(await _summaries.SummarizeAsync(documentId, request));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<SummaryResponse>> GetSummary(Guid documentId)
    {
        var summary = await _summaries.GetSummaryAsync(documentId);
        return summary is null
            ? NotFound($"No summary exists for document {documentId}. Run POST .../summarize first.")
            : Ok(summary);
    }

    [HttpPatch("summary")]
    public async Task<ActionResult<SummaryResponse>> UpdateSummary(Guid documentId, SummarizeRequest request)
    {
        var summary = await _summaries.UpdateSummaryAsync(documentId, request);
        return summary is null
            ? NotFound("No summary to update. Run POST .../summarize first.")
            : Ok(summary);
    }
}

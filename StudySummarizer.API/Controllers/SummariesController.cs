using Microsoft.AspNetCore.Mvc;
using StudySummarizer.Service.Dtos;
using StudySummarizer.Service.Interfaces;

namespace StudySummarizer.API.Controllers;

[ApiController]
[Route("api/documents/{documentId:guid}")]
public class SummariesController : ControllerBase
{
    private readonly ISummaryService _summaryService;

    public SummariesController(ISummaryService summaryService)
    {
        _summaryService = summaryService;
    }

    [HttpPost("summarize")]
    public async Task<ActionResult<SummaryResponse>> Summarize(Guid documentId, SummarizeRequest request)
    {
        return Ok(await _summaryService.SummarizeAsync(documentId, request));
    }

    [HttpGet("summaries")]
    public async Task<ActionResult<IEnumerable<SummaryResponse>>> GetSummaries(Guid documentId)
    {
        var summaries = await _summaryService.GetSummariesAsync(documentId);
        return summaries is null
            ? NotFound($"Document {documentId} was not found.")
            : Ok(summaries);
    }
}

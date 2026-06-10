using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudySummarizer.Data;
using StudySummarizer.Models;
using StudySummarizer.Services;

namespace StudySummarizer.Controllers;

[ApiController]
[Route("api/documents/{documentId:guid}")]
public class SummariesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITextExtractor _extractor;
    private readonly ISummarizer _summarizer;

    public SummariesController(AppDbContext db, ITextExtractor extractor, ISummarizer summarizer)
    {
        _db = db;
        _extractor = extractor;
        _summarizer = summarizer;
    }

    [HttpPost("summarize")]
    public async Task<ActionResult<SummaryResponse>> Summarize(Guid documentId, SummarizeRequest request)
    {
        var document = await _db.Documents
            .Include(d => d.Summary)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document is null)
            return NotFound($"Document {documentId} was not found.");

        try
        {
            document.Status = DocumentStatus.Processing;

            if (string.IsNullOrEmpty(document.ExtractedText))
                document.ExtractedText = _extractor.Extract(document.Content, document.FileName, document.ContentType);

            var summaryText = await _summarizer.SummarizeAsync(document.ExtractedText, request.Length, request.Focus);

            if (document.Summary is not null)
                _db.Summaries.Remove(document.Summary);

            var summary = new Summary
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                Content = summaryText,
                Length = request.Length,
                Focus = request.Focus,
                CreatedAt = DateTime.UtcNow
            };
            _db.Summaries.Add(summary);

            document.Status = DocumentStatus.Summarized;
            await _db.SaveChangesAsync();

            return Ok(ToResponse(summary));
        }
        catch (Exception ex)
        {
            document.Status = DocumentStatus.Failed;
            await _db.SaveChangesAsync();
            return StatusCode(500, $"Summarization failed: {ex.Message}");
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<SummaryResponse>> GetSummary(Guid documentId)
    {
        var summary = await _db.Summaries.FirstOrDefaultAsync(s => s.DocumentId == documentId);
        if (summary is null)
            return NotFound($"No summary exists for document {documentId}. Run POST .../summarize first.");

        return Ok(ToResponse(summary));
    }

    [HttpPatch("summary")]
    public async Task<ActionResult<SummaryResponse>> UpdateSummary(Guid documentId, SummarizeRequest request)
    {
        var document = await _db.Documents
            .Include(d => d.Summary)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document is null)
            return NotFound($"Document {documentId} was not found.");
        if (document.Summary is null)
            return NotFound("No summary to update. Run POST .../summarize first.");
        if (string.IsNullOrEmpty(document.ExtractedText))
            return BadRequest("Document has no extracted text.");

        document.Summary.Content = await _summarizer.SummarizeAsync(document.ExtractedText, request.Length, request.Focus);
        document.Summary.Length = request.Length;
        document.Summary.Focus = request.Focus;
        document.Summary.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(document.Summary));
    }

    private static SummaryResponse ToResponse(Summary s) => new(
        s.Id, s.DocumentId, s.Content, s.Length, s.Focus, s.CreatedAt, s.UpdatedAt
    );
}

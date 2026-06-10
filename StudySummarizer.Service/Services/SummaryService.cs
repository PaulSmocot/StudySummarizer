using Microsoft.EntityFrameworkCore;
using StudySummarizer.Domain.Entities;
using StudySummarizer.Infrastructure.Data;
using StudySummarizer.Service.Dtos;
using StudySummarizer.Service.Exceptions;
using StudySummarizer.Service.Interfaces;

namespace StudySummarizer.Service.Services;

public class SummaryService : ISummaryService
{
    private readonly AppDbContext _db;
    private readonly ITextExtractor _extractor;
    private readonly ISummarizer _summarizer;

    public SummaryService(AppDbContext db, ITextExtractor extractor, ISummarizer summarizer)
    {
        _db = db;
        _extractor = extractor;
        _summarizer = summarizer;
    }

    public async Task<SummaryResponse> SummarizeAsync(Guid documentId, SummarizeRequest request)
    {
        var document = await _db.Documents
            .Include(d => d.Summary)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document is null)
            throw new NotFoundException($"Document {documentId} was not found.");

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

            return ToResponse(summary);
        }
        catch
        {
            document.Status = DocumentStatus.Failed;
            await _db.SaveChangesAsync();
            throw;
        }
    }

    public async Task<SummaryResponse?> GetSummaryAsync(Guid documentId)
    {
        var summary = await _db.Summaries.FirstOrDefaultAsync(s => s.DocumentId == documentId);
        return summary is null ? null : ToResponse(summary);
    }

    public async Task<SummaryResponse?> UpdateSummaryAsync(Guid documentId, SummarizeRequest request)
    {
        var document = await _db.Documents
            .Include(d => d.Summary)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document is null || document.Summary is null)
            return null;

        if (string.IsNullOrEmpty(document.ExtractedText))
            throw new ValidationException("Document has no extracted text.");

        document.Summary.Content = await _summarizer.SummarizeAsync(document.ExtractedText, request.Length, request.Focus);
        document.Summary.Length = request.Length;
        document.Summary.Focus = request.Focus;
        document.Summary.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToResponse(document.Summary);
    }

    private static SummaryResponse ToResponse(Summary s) => new(
        s.Id, s.DocumentId, s.Content, s.Length, s.Focus, s.CreatedAt, s.UpdatedAt
    );
}

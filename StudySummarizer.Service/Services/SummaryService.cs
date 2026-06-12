using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudySummarizer.Domain.Entities;
using StudySummarizer.Infrastructure.Data;
using StudySummarizer.Service.Dtos;
using StudySummarizer.Service.Exceptions;
using StudySummarizer.Service.Interfaces;
using ValidationException = StudySummarizer.Service.Exceptions.ValidationException;

namespace StudySummarizer.Service.Services;

public class SummaryService : ISummaryService
{
    private readonly AppDbContext _db;
    private readonly ITextExtractor _extractor;
    private readonly ISummarizer _summarizer;
    private readonly IValidator<SummarizeRequest> _requestValidator;

    public SummaryService(
        AppDbContext db,
        ITextExtractor extractor,
        ISummarizer summarizer,
        IValidator<SummarizeRequest> requestValidator)
    {
        _db = db;
        _extractor = extractor;
        _summarizer = summarizer;
        _requestValidator = requestValidator;
    }

    public async Task<SummaryResponse> SummarizeAsync(Guid documentId, SummarizeRequest request)
    {
        var validation = await _requestValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new ValidationException(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));

        var document = await _db.Documents
            .Include(d => d.Summaries)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document is null)
            throw new NotFoundException($"Document {documentId} was not found.");

        try
        {
            document.Status = DocumentStatus.Processing;

            if (string.IsNullOrEmpty(document.ExtractedText))
                document.ExtractedText = _extractor.Extract(document.Content, document.FileName, document.ContentType);

            var summaryText = await _summarizer.SummarizeAsync(document.ExtractedText, request.Length, request.Focus);

            var existing = document.Summaries
                .FirstOrDefault(s => s.Length == request.Length && s.Focus == request.Focus);

            Summary summary;
            if (existing is not null)
            {
                existing.Content = summaryText;
                existing.UpdatedAt = DateTime.UtcNow;
                summary = existing;
            }
            else
            {
                summary = new Summary
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    Content = summaryText,
                    Length = request.Length,
                    Focus = request.Focus,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Summaries.Add(summary);
            }

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

    public async Task<IEnumerable<SummaryResponse>?> GetSummariesAsync(Guid documentId)
    {
        var document = await _db.Documents
            .Include(d => d.Summaries)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document is null)
            return null;

        return document.Summaries
            .OrderByDescending(s => s.CreatedAt)
            .Select(ToResponse);
    }

    private static SummaryResponse ToResponse(Summary s) => new(
        s.Id, s.DocumentId, s.Content, s.Length, s.Focus, s.CreatedAt, s.UpdatedAt
    );
}

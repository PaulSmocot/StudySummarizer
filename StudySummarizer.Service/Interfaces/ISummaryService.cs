using StudySummarizer.Service.Dtos;

namespace StudySummarizer.Service.Interfaces;

public interface ISummaryService
{
    Task<SummaryResponse> SummarizeAsync(Guid documentId, SummarizeRequest request);

    Task<SummaryResponse?> GetSummaryAsync(Guid documentId);

    Task<SummaryResponse?> UpdateSummaryAsync(Guid documentId, SummarizeRequest request);
}

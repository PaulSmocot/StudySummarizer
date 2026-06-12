using StudySummarizer.Service.Dtos;

namespace StudySummarizer.Service.Interfaces;

public interface ISummaryService
{
    Task<SummaryResponse> SummarizeAsync(Guid documentId, SummarizeRequest request);

    Task<IEnumerable<SummaryResponse>?> GetSummariesAsync(Guid documentId);
}

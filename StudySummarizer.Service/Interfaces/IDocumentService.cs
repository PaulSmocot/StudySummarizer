using StudySummarizer.Service.Dtos;

namespace StudySummarizer.Service.Interfaces;

public interface IDocumentService
{
    Task<DocumentResponse> UploadAsync(UploadFileInput input);

    Task<IEnumerable<DocumentResponse>> GetAllAsync();

    Task<DocumentResponse?> GetByIdAsync(Guid id);

    Task<DocumentFileResult?> GetFileAsync(Guid id);

    Task<bool> DeleteAsync(Guid id);
}

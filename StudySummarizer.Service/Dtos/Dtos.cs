using StudySummarizer.Domain.Entities;

namespace StudySummarizer.Service.Dtos;

public record DocumentResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    DocumentStatus Status,
    DateTime UploadedAt,
    bool HasSummary
);

public record SummaryResponse(
    Guid Id,
    Guid DocumentId,
    string Content,
    string Length,
    string? Focus,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record SummarizeRequest(
    string Length = "medium",
    string? Focus = null
);

public record UploadFileInput(
    string FileName,
    string ContentType,
    long SizeBytes,
    byte[] Content
);

public record DocumentFileResult(
    byte[] Content,
    string ContentType,
    string FileName
);

namespace StudySummarizer.Models;

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

namespace StudySummarizer.Domain.Entities;

public enum DocumentStatus
{
    Uploaded,
    Processing,
    Summarized,
    Failed
}

public class Document : BaseEntity
{
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public byte[] Content { get; set; } = [];

    public string? ExtractedText { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Uploaded;

    public ICollection<Summary> Summaries { get; set; } = [];
}

namespace StudySummarizer.Models;

public enum DocumentStatus
{
    Uploaded,
    Processing,
    Summarized,
    Failed
}

public class Document
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public byte[] Content { get; set; } = [];

    public string? ExtractedText { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Uploaded;

    public DateTime UploadedAt { get; set; }

    public Summary? Summary { get; set; }
}

namespace StudySummarizer.Models;

public class Summary
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public string Length { get; set; } = "medium";
    public string? Focus { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Document? Document { get; set; }
}

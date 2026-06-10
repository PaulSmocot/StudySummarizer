namespace StudySummarizer.Services;

public interface ISummarizer
{
    Task<string> SummarizeAsync(string text, string length, string? focus);
}

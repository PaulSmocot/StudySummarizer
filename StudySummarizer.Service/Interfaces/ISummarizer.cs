namespace StudySummarizer.Service.Interfaces;

public interface ISummarizer
{
    Task<string> SummarizeAsync(string text, string length, string? focus);
}

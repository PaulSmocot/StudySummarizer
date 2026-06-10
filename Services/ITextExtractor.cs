namespace StudySummarizer.Services;

public interface ITextExtractor
{
    string Extract(byte[] content, string fileName, string contentType);
}

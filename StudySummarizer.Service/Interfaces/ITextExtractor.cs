namespace StudySummarizer.Service.Interfaces;

public interface ITextExtractor
{
    string Extract(byte[] content, string fileName, string contentType);
}

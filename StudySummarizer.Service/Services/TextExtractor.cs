using System.Text;
using DocumentFormat.OpenXml.Packaging;
using StudySummarizer.Service.Interfaces;
using UglyToad.PdfPig;

namespace StudySummarizer.Service.Services;

public class TextExtractor : ITextExtractor
{
    public string Extract(byte[] content, string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".txt" => ExtractTxt(content),
            ".pdf" => ExtractPdf(content),
            ".docx" => ExtractDocx(content),
            _ => throw new NotSupportedException($"Unsupported type: {extension}")
        };
    }

    private static string ExtractTxt(byte[] content) =>
        Encoding.UTF8.GetString(content);

    private static string ExtractPdf(byte[] content)
    {
        var sb = new StringBuilder();
        using var pdf = PdfDocument.Open(content);
        foreach (var page in pdf.GetPages())
            sb.AppendLine(page.Text);
        return sb.ToString();
    }

    private static string ExtractDocx(byte[] content)
    {
        using var stream = new MemoryStream(content);
        using var doc = WordprocessingDocument.Open(stream, false);
        return doc.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
    }
}

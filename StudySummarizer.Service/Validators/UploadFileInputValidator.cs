using FluentValidation;
using StudySummarizer.Service.Dtos;

namespace StudySummarizer.Service.Validators;

public class UploadFileInputValidator : AbstractValidator<UploadFileInput>
{
    private const long MaxFileSize = 20 * 1024 * 1024;

    private static readonly string[] AllowedExtensions = [".pdf", ".docx", ".txt"];

    public UploadFileInputValidator()
    {
        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("No file sent or file is empty.")
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage($"File too large. Maximum {MaxFileSize / 1024 / 1024} MB.");

        RuleFor(x => x.FileName)
            .Must(fileName => AllowedExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant()))
            .WithMessage($"Invalid type. Accepted: {string.Join(", ", AllowedExtensions)}");
    }
}

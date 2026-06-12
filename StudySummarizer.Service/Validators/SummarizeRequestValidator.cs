using FluentValidation;
using StudySummarizer.Service.Dtos;

namespace StudySummarizer.Service.Validators;

public class SummarizeRequestValidator : AbstractValidator<SummarizeRequest>
{
    private static readonly string[] AllowedLengths = ["short", "medium", "long"];

    public SummarizeRequestValidator()
    {
        RuleFor(x => x.Length)
            .NotEmpty().WithMessage("Length is required.")
            .Must(length => AllowedLengths.Contains(length.ToLowerInvariant()))
            .WithMessage($"Length must be one of: {string.Join(", ", AllowedLengths)}.");

        RuleFor(x => x.Focus)
            .MaximumLength(200).WithMessage("Focus must be at most 200 characters.");
    }
}

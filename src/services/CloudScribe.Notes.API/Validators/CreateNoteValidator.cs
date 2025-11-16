using CloudScribe.Contracts.Notes;
using FluentValidation;

namespace CloudScribe.Notes.API.Validators;

public sealed class CreateNoteValidator : AbstractValidator<CreateNoteRequest>
{
    public CreateNoteValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(5000);

    }
}
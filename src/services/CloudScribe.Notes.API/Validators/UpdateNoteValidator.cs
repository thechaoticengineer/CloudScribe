using CloudScribe.Contracts.Notes;
using FluentValidation;

namespace CloudScribe.Notes.API.Validators;

public sealed class UpdateNoteValidator : AbstractValidator<UpdateNoteRequest>
{
    public UpdateNoteValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(5000);
    }
}
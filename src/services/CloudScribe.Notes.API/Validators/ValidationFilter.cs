using FluentValidation;
using FluentValidation.Results;

namespace CloudScribe.Notes.API.Validators;

public class ValidationFilter<T> : IEndpointFilter where T : class?
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var argToValidate = context.Arguments.OfType<T>().FirstOrDefault();

        if (argToValidate is null)
        {
            return Results.BadRequest("Cannot find object to validate.");
        }

        ValidationResult validationResult = await _validator.ValidateAsync(argToValidate);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }
}
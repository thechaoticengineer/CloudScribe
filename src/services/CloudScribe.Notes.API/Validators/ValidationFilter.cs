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
        var argToValidate = context.GetArgument<T>(0);

        if (argToValidate is null)
        {
            return Results.BadRequest("Nie można odnaleźć obiektu do walidacji.");
        }

        ValidationResult validationResult = await _validator.ValidateAsync(argToValidate);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }
}
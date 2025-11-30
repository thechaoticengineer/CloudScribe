using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloudScribe.SharedKernel;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.IsSuccess 
            ? TypedResults.Ok(result.Value) 
            : MapFailure(result.Error!);
    }

    public static IResult ToHttpResult(this Result result)
    {
        return result.IsSuccess 
            ? TypedResults.NoContent() 
            : MapFailure(result.Error!);
    }
    
    public static IResult ToCreated<T>(this Result<T> result, Func<T, string> uriBuilder)
    {
        if (result.IsSuccess)
        {
            var location = uriBuilder(result.Value!);
            return Results.Created(location, result.Value);
        }

        return MapFailure(result.Error!);
    }

    private static IResult MapFailure(Error error)
    {
        var problem = CreateProblemDetails(
            GetTitle(error.Type), 
            GetStatusCode(error.Type), 
            error);

        return error.Type switch
        {
            ErrorType.Validation => TypedResults.BadRequest(problem),
            ErrorType.NotFound   => TypedResults.NotFound(problem),
            ErrorType.Conflict   => TypedResults.Conflict(problem),
            _ => TypedResults.Problem(problem)
        };
    }

    private static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound   => StatusCodes.Status404NotFound,
        ErrorType.Conflict   => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTitle(ErrorType type) => type switch
    {
        ErrorType.Validation => "Bad Request",
        ErrorType.NotFound   => "Not Found",
        ErrorType.Conflict   => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        _ => "Internal Server Error"
    };

    private static ProblemDetails CreateProblemDetails(string title, int status, Error error)
    {
        return new ProblemDetails
        {
            Title = title,
            Status = status,
            Detail = error.Message,
            Extensions = { { "code", error.Code } }
        };
    }
}
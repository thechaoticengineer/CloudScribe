using Microsoft.AspNetCore.Http;

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
        return error.Type switch
        {
            ErrorType.NotFound => TypedResults.NotFound(CreateProblem(error)),
            ErrorType.Validation => TypedResults.BadRequest(CreateProblem(error)),
            ErrorType.Conflict => TypedResults.Conflict(CreateProblem(error)),
            _ => TypedResults.InternalServerError(CreateProblem(error))
        };
    }

    private static object CreateProblem(Error error) => new 
    { 
        Status = error.Code, 
        Detail = error.Message 
    };
}
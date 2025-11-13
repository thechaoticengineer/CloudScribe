namespace CloudScribe.Notes.API.Api.Endpoints;

public static class NotesEndpoints
{
    public static IEndpointRouteBuilder MapNotesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notes")
            .WithTags("Notes")
            .WithOpenApi();

        return app;
    }
    
}
using CloudScribe.Notes.API.Domain;
using CloudScribe.Notes.API.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using SharedKernel;

namespace CloudScribe.Notes.API.Api.Endpoints;

public static class NotesEndpoints
{
    public static IEndpointRouteBuilder MapNotesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notes")
            .WithTags("Notes")
            .WithOpenApi();

        group.MapGet("", async (NotesService service, int pageNumber = 1, int pageSize = 10) =>
            Results.Ok(await service.GetAll(pageNumber, pageSize)))
            .Produces<PagedResult<Note>>();

        group.MapGet("{id:guid}", async Task<Results<Ok<Domain.Note>, NotFound>> (Guid id, NotesService service) =>
        {
            var note = await service.GetByIdAsync(id);
            return note is not null
                ? TypedResults.Ok(note)
                : TypedResults.NotFound();
        })
        .Produces<Note>();

        group.MapPost("", async (CreateNoteRequest request, NotesService service) =>
        {
            var note = await service.Create(request.Title, request.Content);
            return Results.Created($"/api/notes/{note.Id}", note);
        })
        .Produces<Note>(StatusCodes.Status201Created) ;

        group.MapPut("{id:guid}", async Task<Results<Ok<Domain.Note>, NotFound>> (Guid id, UpdateNoteRequest request, NotesService service) =>
        {
            var note = await service.Update(id, request.Title, request.Content);
            return note is not null
                ? TypedResults.Ok(note)
                : TypedResults.NotFound();
        }).Produces<Note>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, NotesService service) =>
        {
            var deleted = await service.Delete(id);
            return deleted
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        }).Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

public record CreateNoteRequest(string Title, string Content);
public record UpdateNoteRequest(string Title, string Content);
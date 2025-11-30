using CloudScribe.Contracts.Notes;
using CloudScribe.Notes.API.Domain;
using CloudScribe.Notes.API.Services;
using CloudScribe.Notes.API.Validators;
using CloudScribe.SharedKernel;

namespace CloudScribe.Notes.API.Api.Endpoints;

public static class NotesEndpoints
{
    public static IEndpointRouteBuilder MapNotesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notes")
            .WithTags("Notes")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("",
                async (NotesService service, int pageNumber = 1, int pageSize = 10) =>
                {
                    var result = await service.GetAll(pageNumber, pageSize);
                    return result.ToHttpResult();
                })
            .Produces<PagedResult<Note>>()
            .WithDescription("Get all notes ordered by created date descending");

        group.MapGet("{id:guid}", async (Guid id, NotesService service) =>
            {
                var note = await service.GetById(id);
                return note.ToHttpResult();
            })
            .Produces<Note>()
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Get a note by id");

        group.MapPost("", async (CreateNoteRequest request, NotesService service) =>
            {
                var result = await service.Create(request.Title, request.Content);
                return result.ToCreated(note => $"/api/notes/{note.Id}");
            })
            .Produces<Note>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .AddEndpointFilter<ValidationFilter<CreateNoteRequest>>()
            .WithDescription("Create a new note");

        group.MapPut("{id:guid}",
                async (Guid id, UpdateNoteRequest request,
                    NotesService service) =>
                {
                    var result = await service.Update(id, request.Title, request.Content);
                    return result.ToHttpResult();
                })
            .Produces<Note>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .AddEndpointFilter<ValidationFilter<UpdateNoteRequest>>()
            .WithDescription("Update an existing note");

        group.MapDelete("{id:guid}", async (Guid id, NotesService service) =>
            {
                var result = await service.Delete(id);
                return result.ToHttpResult();
            }).Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Delete an existing note");

        return app;
    }
}
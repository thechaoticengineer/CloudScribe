using CloudScribe.Notes.API.Domain;
using CloudScribe.Notes.API.Infrastructure.Auth;
using CloudScribe.Notes.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CloudScribe.SharedKernel;

namespace CloudScribe.Notes.API.Services;

internal class NotesService(CloudScribeDbContext db, ICurrentUser currentUser)
{

    public async Task<Result<PagedResult<Note>>> GetAll(int pageNumber = 1, int pageSize = 10)
    {
        var query = db.Notes.Where(n => n.OwnerId == currentUser.Id);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(n => n.CreatedUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();


        return new PagedResult<Note>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Result<Note>> GetById(Guid id)
    {
        var note = await db.Notes.FindAsync(id);
        if (note is null)
            return Result<Note>.Failure(Error.NotFound("Notes.NotFound", $"Note {id} doesn't exist"));

        if (note.OwnerId != currentUser.Id)
            return Result<Note>.Failure(Error.Forbidden("Notes.Forbidden", "You don't have access to this note"));

        return note;
    }

    public async Task<Result<Note>> Create(string title, string content)
    {
        var note = Note.Create(title, content, currentUser.Id);
        db.Notes.Add(note);
        await db.SaveChangesAsync();
        return note;
    }

    public async Task<Result<Note>> Update(Guid id, string title, string content)
    {
        var note = await db.Notes.FindAsync(id);
        if (note is null)
            return Result<Note>.Failure(Error.NotFound("Notes.NotFound", $"Note {id} doesn't exist"));

        if (note.OwnerId != currentUser.Id)
            return Result<Note>.Failure(Error.Forbidden("Notes.Forbidden", "You don't have access to this note"));

        note.Update(title, content);
        await db.SaveChangesAsync();
        return note;
    }

    public async Task<Result> Delete(Guid id)
    {
        var note = await db.Notes.FindAsync(id);
        if (note is null)
            return Result.Failure(Error.NotFound("Notes.NotFound", $"Note {id} doesn't exist"));

        if (note.OwnerId != currentUser.Id)
            return Result.Failure(Error.Forbidden("Notes.Forbidden", "You don't have access to this note"));

        db.Notes.Remove(note);
        await db.SaveChangesAsync();
        return Result.Success();
    }
}
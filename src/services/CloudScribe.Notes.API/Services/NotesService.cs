using CloudScribe.Notes.API.Domain;
using CloudScribe.Notes.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CloudScribe.SharedKernel;

namespace CloudScribe.Notes.API.Services;

internal class NotesService
{
    private readonly CloudScribeDbContext _db;

    public NotesService(CloudScribeDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<Note>> GetAll(int pageNumber = 1, int pageSize = 10)
    {
        var totalCount = await _db.Notes.CountAsync();

        var items = await _db.Notes
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

    public async Task<Note?> GetByIdAsync(Guid id)
    {
        return await _db.Notes.FindAsync(id);
    }

    public async Task<Note> Create(string title, string content)
    {
        var note = Note.Create(title, content);
        _db.Notes.Add(note);
        await _db.SaveChangesAsync();
        return note;
    }

    public async Task<Note?> Update(Guid id, string title, string content)
    {
        var note = await _db.Notes.FindAsync(id);
        if (note is null)
            return null;

        note.Update(title, content);
        await _db.SaveChangesAsync();
        return note;
    }

    public async Task<bool> Delete(Guid id)
    {
        var note = await _db.Notes.FindAsync(id);
        if (note is null)
            return false;

        _db.Notes.Remove(note);
        await _db.SaveChangesAsync();
        return true;
    }
}
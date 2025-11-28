using CloudScribe.Blazor.Services.Auth;
using CloudScribe.Contracts.Notes;
using CloudScribe.SharedKernel;

namespace CloudScribe.Blazor.Services;

public class NotesClient(IHttpClientFactory factory, TokenService tokenService) : BaseClient(factory, tokenService, "API")
{
    public async Task<PagedResult<NoteDto>?> GetNotesAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await Get<PagedResult<NoteDto>>($"/api/notes?pageNumber={pageNumber}&pageSize={pageSize}");
    }

    public async Task CreateNoteAsync(CreateNoteRequest request)
    {
        await Post("/api/notes", request);
    }

    public async Task UpdateNoteAsync(Guid id, UpdateNoteRequest request)
    {
        await Put($"/api/notes/{id}", request);
    }

    public async Task<bool> DeleteNoteAsync(Guid id)
    {
        return await Delete($"/api/notes/{id}");
    }
}
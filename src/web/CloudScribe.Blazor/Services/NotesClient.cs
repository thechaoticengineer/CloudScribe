using CloudScribe.Contracts.Notes;
using CloudScribe.SharedKernel;

namespace CloudScribe.Blazor.Services;

public class NotesClient(HttpClient http)
{
    public async Task<PagedResult<NoteDto>?> GetNotesAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await http.GetFromJsonAsync<PagedResult<NoteDto>>($"/api/notes?pageNumber={pageNumber}&pageSize={pageSize}");
    }

    public async Task CreateNoteAsync(CreateNoteRequest request)
    {
        await http.PostAsJsonAsync("/api/notes", request);
    }

    public async Task UpdateNoteAsync(Guid id, UpdateNoteRequest request)
    {
        await http.PutAsJsonAsync($"/api/notes/{id}", request);
    }

    public Task<HttpResponseMessage> DeleteNoteAsync(Guid id)
    {
        return http.DeleteAsync($"/api/notes/{id}");
    }
}
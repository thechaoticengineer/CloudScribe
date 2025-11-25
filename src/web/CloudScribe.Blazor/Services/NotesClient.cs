using System.Net.Http.Headers;
using CloudScribe.Contracts.Notes;
using CloudScribe.SharedKernel;
using Microsoft.AspNetCore.Components.Authorization;

namespace CloudScribe.Blazor.Services;

public class NotesClient(HttpClient http, AuthenticationStateProvider authStateProvider)
{
    // todo: for test purposes only, remove later
    private async Task SetAuthorizationHeaderAsync()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var tokenClaim = user.FindFirst("access_token");

        if (tokenClaim != null)
        {
            http.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", tokenClaim.Value);
        }
    }
    public async Task<PagedResult<NoteDto>?> GetNotesAsync(int pageNumber = 1, int pageSize = 10)
    {
        await SetAuthorizationHeaderAsync();
        return await http.GetFromJsonAsync<PagedResult<NoteDto>>($"/api/notes?pageNumber={pageNumber}&pageSize={pageSize}");
    }

    public async Task CreateNoteAsync(CreateNoteRequest request)
    {
        await SetAuthorizationHeaderAsync();
        await http.PostAsJsonAsync("/api/notes", request);
    }

    public async Task UpdateNoteAsync(Guid id, UpdateNoteRequest request)
    {
        await SetAuthorizationHeaderAsync();
        await http.PutAsJsonAsync($"/api/notes/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteNoteAsync(Guid id)
    {
        await SetAuthorizationHeaderAsync();
        return await http.DeleteAsync($"/api/notes/{id}");
    }
}
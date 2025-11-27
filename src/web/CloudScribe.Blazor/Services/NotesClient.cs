using System.Net.Http.Headers;
using CloudScribe.Blazor.Services.Auth;
using CloudScribe.Contracts.Notes;
using CloudScribe.SharedKernel;
using Microsoft.AspNetCore.Components.Authorization;

namespace CloudScribe.Blazor.Services;

public class NotesClient(IHttpClientFactory factory, TokenService tokenService)
{
    private async Task<HttpClient> CreateClientAsync()
    {
        
        var client = factory.CreateClient("API");
        
        var token = await tokenService.GetAccessTokenAsync();
        
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }
    public async Task<PagedResult<NoteDto>?> GetNotesAsync(int pageNumber = 1, int pageSize = 10)
    {
        var client = await CreateClientAsync();
        return await client.GetFromJsonAsync<PagedResult<NoteDto>>($"/api/notes?pageNumber={pageNumber}&pageSize={pageSize}");
    }

    public async Task CreateNoteAsync(CreateNoteRequest request)
    {
        var client = await CreateClientAsync();
        await client.PostAsJsonAsync("/api/notes", request);
    }

    public async Task UpdateNoteAsync(Guid id, UpdateNoteRequest request)
    {
        var client = await CreateClientAsync();
        await client.PutAsJsonAsync($"/api/notes/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteNoteAsync(Guid id)
    {
        var client = await CreateClientAsync();
        return await client.DeleteAsync($"/api/notes/{id}");
    }
}
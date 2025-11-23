using CloudScribe.Contracts.Notes;
using CloudScribe.SharedKernel;
using Microsoft.AspNetCore.Components;

namespace CloudScribe.Blazor.Components.Pages;

public partial class Notes(HttpClient http) : ComponentBase
{
    private NoteFormModel Request { get; set; } = new();
    private PagedResult<NoteDto>? _notes;

    private async Task OnSubmit()
    {
        try
        {
            await http.PostAsJsonAsync("/api/notes", new CreateNoteRequest(Request.Title, Request.Content));
            await LoadNotes();
            Request = new();
        }
        catch (Exception)
        {
        }
        
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await LoadNotes();
        }
        catch (Exception)
        {
        }
    }
    
    private async Task LoadNotes()
    {
        _notes = await http.GetFromJsonAsync<PagedResult<NoteDto>>("/api/notes?pageNumber=1&pageSize=10");
    }

    private async Task DeleteNote(Guid noteId)
    {
        var response = await http.DeleteAsync($"/api/notes/{noteId}");
        if (response.IsSuccessStatusCode)
        {
            await LoadNotes();
        }
        else
        {
            // show snackbar later. 
        }
    }
}
using System.ComponentModel.DataAnnotations;
using CloudScribe.Contracts.Notes;
using CloudScribe.SharedKernel;
using Microsoft.AspNetCore.Components;

namespace CloudScribe.Blazor.Components.Pages;

public partial class Notes(HttpClient http) : ComponentBase
{
    private CreateNote Request { get; set; } = new();
    private PagedResult<NoteDto>? _notes;

    private async Task OnSubmit()
    {
        try
        {
            await http.PostAsJsonAsync("/api/notes", new CreateNoteRequest(Request.Title, Request.Content));
            await LoadNotes();
            Request = new();
        }
        catch (Exception e)
        {
        }
        
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await LoadNotes();
        }
        catch (Exception ex)
        {
        }
    }
    
    private async Task LoadNotes()
    {
        _notes = await http.GetFromJsonAsync<PagedResult<NoteDto>>("/api/notes?pageNumber=1&pageSize=10");
    }
}

public class CreateNote
{
    [Required(ErrorMessage = "Title is required.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required.")]
    public string Content { get; set; } = string.Empty;
}

public class NoteDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedUtc { get; init;  }
    public DateTime ModifiedUtc { get; init; }
}
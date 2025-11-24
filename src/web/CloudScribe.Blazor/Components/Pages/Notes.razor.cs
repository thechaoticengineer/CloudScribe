using CloudScribe.Blazor.Components.Dialogs;
using CloudScribe.Blazor.Services;
using CloudScribe.Contracts.Notes;
using CloudScribe.SharedKernel;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CloudScribe.Blazor.Components.Pages;

public partial class Notes(NotesClient notesClient, IDialogService dialogService, ISnackbar snackbar) : ComponentBase
{
    private NoteFormModel Request { get; set; } = new();
    private PagedResult<NoteDto>? _notes;
    

    private async Task OnSubmit()
    {
        try
        {
            await notesClient.CreateNoteAsync(new CreateNoteRequest(Request.Title, Request.Content));
            await LoadNotes();
            Request = new();
        }
        catch (Exception)
        {
            snackbar.Add("Error creating note", Severity.Error);
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
            snackbar.Add("Error loading notes", Severity.Error);
        }
    }
    
    private async Task LoadNotes()
    {
        _notes = await notesClient.GetNotesAsync();
    }

    private async Task DeleteNote(Guid noteId)
    {
        var response = await notesClient.DeleteNoteAsync(noteId);
        if (response.IsSuccessStatusCode)
        {
            await LoadNotes();
        }
        else
        {
            snackbar.Add("Error!", Severity.Error);
        }
    }
    
    private async Task OpenDialog(NoteDto? noteToEdit)
    {
        var formModel = new NoteFormModel();
        
        if (noteToEdit != null)
        {
            formModel.Title = noteToEdit.Title;
            formModel.Content = noteToEdit.Content;
        }
        
        var parameters = new DialogParameters<NoteDialog>
        {
            { x => x.Model, formModel }
        };
        
        var options = new DialogOptions 
        { 
            CloseOnEscapeKey = true, 
            MaxWidth = MaxWidth.Small, 
            FullWidth = true,
            CloseButton = true
        };
        
        var title = noteToEdit == null ? "Create new note" : "Edit note";
        
        var dialog = await dialogService.ShowAsync<NoteDialog>(title, parameters, options);
        var result = await dialog.Result;
        
        if (result is not null && !result.Canceled)
        {
            var data = (NoteFormModel)result.Data!;

            try 
            {
                if (noteToEdit != null)
                {
                    var updateRequest = new UpdateNoteRequest(data.Title, data.Content);
                    await notesClient.UpdateNoteAsync(noteToEdit.Id, updateRequest);
                    snackbar.Add("Notatka zaktualizowana", Severity.Success);
                }
                else
                {
                    var createRequest = new CreateNoteRequest(data.Title, data.Content);
                    await notesClient.CreateNoteAsync(createRequest);
                    snackbar.Add("Notatka utworzona", Severity.Success);
                }
                await LoadNotes();
            }
            catch (Exception)
            {
                snackbar.Add("Wystąpił błąd podczas zapisywania", Severity.Error);
            }
        }
    }
}
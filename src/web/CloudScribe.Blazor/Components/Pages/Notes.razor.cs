using CloudScribe.Blazor.Components.Dialogs;
using CloudScribe.Blazor.Services;
using CloudScribe.Contracts.Notes;
using CloudScribe.SharedKernel;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CloudScribe.Blazor.Components.Pages;

public partial class Notes(
    NotesClient notesClient,
    IDialogService dialogService,
    ISnackbar snackbar,
    ILogger<Notes> logger) : ComponentBase
{
    private PagedResult<NoteDto>? _notes;
    

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await LoadNotes();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to load notes. Status: {StatusCode}", ex.StatusCode);
            snackbar.Add("Error loading notes", Severity.Error);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while loading notes");
            snackbar.Add("Error loading notes", Severity.Error);
        }
    }
    
    private async Task LoadNotes()
    {
        _notes = await notesClient.GetNotesAsync();
    }

    private async Task DeleteNote(Guid noteId)
    {
        var isSuccess = await notesClient.DeleteNoteAsync(noteId);
        if (isSuccess)
        {
            await LoadNotes();
        }
        else
        {
            snackbar.Add("Error while deleting!", Severity.Error);
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
                    snackbar.Add("Note updated", Severity.Success);
                }
                else
                {
                    var createRequest = new CreateNoteRequest(data.Title, data.Content);
                    await notesClient.CreateNoteAsync(createRequest);
                    snackbar.Add("Note created", Severity.Success);
                }
                await LoadNotes();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to save note. Status: {StatusCode}", ex.StatusCode);
                snackbar.Add("Error occurred while saving", Severity.Error);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while saving note");
                snackbar.Add("Error occurred while saving", Severity.Error);
            }
        }
    }
}
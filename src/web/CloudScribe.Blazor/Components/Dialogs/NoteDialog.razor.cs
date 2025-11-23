using CloudScribe.Blazor.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace CloudScribe.Blazor.Components.Dialogs;

public partial class NoteDialog : ComponentBase
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public NoteFormModel Model { get; set; } = new();
    private EditContext _editContext = null!;

    private void Cancel() => MudDialog.Cancel();

    private void Submit()
    {
        if (_editContext.Validate())
        {
            MudDialog.Close(DialogResult.Ok(Model));
        }
    }
}
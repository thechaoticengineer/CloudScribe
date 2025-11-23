using System.ComponentModel.DataAnnotations;

namespace CloudScribe.Blazor.Components.Pages;

public class NoteFormModel
{
    [Required(ErrorMessage = "Title is required.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required.")]
    public string Content { get; set; } = string.Empty;
}
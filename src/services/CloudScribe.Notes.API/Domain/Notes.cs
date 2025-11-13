namespace CloudScribe.Notes.API.Domain;

public sealed class Notes
{
    Guid Id { get; set; }
    string Title { get; set; } = string.Empty;
    string Content { get; set; } = string.Empty;
    DateTime CreatedUtc { get; set; }
}
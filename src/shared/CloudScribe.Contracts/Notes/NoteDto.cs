namespace CloudScribe.Contracts.Notes;

public class NoteDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedUtc { get; init;  }
    public DateTime ModifiedUtc { get; init; }
}
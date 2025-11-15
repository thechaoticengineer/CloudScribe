using JetBrains.Annotations;

namespace CloudScribe.Notes.API.Domain;

public sealed class Note
{
    public Guid Id { get; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedUtc { get; }
    public DateTime ModifiedUtc { get; private set; }
    // UserID will be added later
    
    [UsedImplicitly]
    private Note() { }

    private Note(string title, string content)
    {
        Id = Guid.CreateVersion7();
        Title = title;
        Content = content;
        CreatedUtc = DateTime.UtcNow;
        ModifiedUtc = DateTime.UtcNow;
    }
    
    public static Note Create(string title, string content)
    {
        return new Note(title, content);
    }

    public Note Update(string title, string content)
    {
        Title = title;
        Content = content;
        ModifiedUtc = DateTime.UtcNow;
        return this;
    }
}
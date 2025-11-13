namespace CloudScribe.Notes.API.Domain;

public sealed class Notes
{
    public Guid Id { get; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedUtc { get; }
    public DateTime ModifiedUtc { get; private set; }
    
    private Notes() { }

    private Notes(string title, string content)
    {
        Id = Guid.CreateVersion7();
        Title = title;
        Content = content;
        CreatedUtc = DateTime.UtcNow;
        ModifiedUtc = DateTime.UtcNow;
    }
    
    public static Notes Create(string title, string content)
    {
        return new Notes(title, content);
    }

    public Notes Update(string title, string content)
    {
        Title = title;
        Content = content;
        ModifiedUtc = DateTime.UtcNow;
        return this;
    }
}
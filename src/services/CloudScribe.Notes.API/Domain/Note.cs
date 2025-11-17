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
        ValidateTitle(title);
        Title = title;
        ValidateContent(content);
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
        ValidateTitle(title);
        Title = title;
        ValidateContent(content);
        Content = content;
        ModifiedUtc = DateTime.UtcNow;
        return this;
    }
    
    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 200)
        {
            throw new DomainException("Title must be provided and cannot be longer than 200 characters");
        }
    }

    private static void ValidateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > 5000)
        {
            throw new DomainException("Content must be provided and cannot be longer than 5000 characters");
        }
    }
}
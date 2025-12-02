namespace CloudScribe.Notes.API.Infrastructure.Auth;

public interface ICurrentUser
{
    Guid Id { get; }
}
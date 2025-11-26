using System.Net.Http.Headers;
using CloudScribe.Notes.API.Domain;
using CloudScribe.Notes.API.Infrastructure.Data;
using CloudScribe.Notes.API.Tests.Helpers;

namespace CloudScribe.Notes.API.Tests.IntegrationTests;

public abstract class BaseIntegrationTest
{
    private NotesApiFactory _factory = null!;
    private IServiceScope _scope = null!;
    protected HttpClient Client { get; private set; } = null!;
    private CloudScribeDbContext DbContext { get; set; } = null!;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        _factory = new NotesApiFactory();
        await _factory.StartContainerAsync();
        Client = _factory.CreateClient();
    }
    
    [OneTimeTearDown]
    public async Task GlobalTearDown()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
    }
    
    [SetUp]
    public async Task Setup()
    {
        _scope = _factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<CloudScribeDbContext>();
        await DbContext.Notes.ExecuteDeleteAsync();
    }
    
    [TearDown]
    public void TearDown()
    {
        DbContext.Dispose();
        _scope.Dispose();
    }

    protected async Task<Note> CreateNoteAndSave(string title, string content)
    {
        var note = Note.Create(title, content);
        DbContext.Notes.Add(note);
        await DbContext.SaveChangesAsync();
        return note;
    }
    
    protected void AuthenticateAsync(string userId = "test-user-id")
    {
        var token = TestAuthHandler.GenerateJwtToken(userId);
        Client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuthentication()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }
}
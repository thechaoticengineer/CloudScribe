using CloudScribe.Notes.API.Domain;
using CloudScribe.Notes.API.Infrastructure.Data;
using CloudScribe.Notes.API.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace CloudScribe.Notes.API.Tests.UnitTests;

[TestFixture]
public class NoteServiceTest
{
    private CloudScribeDbContext _dbContext = null!;
    private NotesService _service = null!;
    
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CloudScribeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CloudScribeDbContext(options);
        _service = new NotesService(_dbContext);
    }
    
    [TestCase("test", "test")]
    [TestCase("test multi words", "test multi words")]
    public async Task AddNote_ShouldReturnNoteWithTitleAndContent(string title, string content)
    {
        var result = await _service.Create(title, content);
        
        result.ShouldSatisfyAllConditions(() => result.ShouldNotBeNull(),
            () => result.Title.ShouldBe(title),
            () => result.Content.ShouldBe(content));
    }

    [Test]
    public async Task AddNote_ShouldCreateWhenMaxTitleLengthIsReached()
    {
        var result = await _service.Create("a".PadRight(Note.MaxTitleLength, 'a'),"test");
        
        result.ShouldSatisfyAllConditions(() => result.ShouldNotBeNull());
    }
    
    [Test]
    public async Task AddNote_ShouldCreateWhenMaxContentLengthIsReached()
    {
        var result = await _service.Create("test", "a".PadRight(Note.MaxContentLength, 'a'));
    }
    
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void AddNote_ShouldReturnErrorWhenTitleIsEmpty(string? title)
    {
        Should.Throw<DomainException>(async () => await _service.Create(title!, "test"));
    }
    
    [Test]
    public void AddNote_ShouldReturnErrorWhenTitleIsTooLong() => 
        Should.Throw<DomainException>(async () => await _service.Create("a".PadRight(Note.MaxTitleLength + 1, 'a'),
            "test"));
    
    [Test]
    public void AddNote_ShouldReturnErrorWhenContentIsTooLong() => 
        Should.Throw<DomainException>(async () => await _service.Create("test", "a".PadRight(Note.MaxContentLength + 1,
            'a')));
    
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void AddNote_ShouldReturnErrorWhenContentIsEmpty(string? content) =>
        Should.Throw<DomainException>(async () => await _service.Create("test", content!));
    
    
    
    
    
    [TearDown]
    public async Task TearDown()
    {
        await _dbContext.DisposeAsync();
    }
    
}
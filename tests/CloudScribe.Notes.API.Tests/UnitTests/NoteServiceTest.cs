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
    private Guid _noteReadId;
    private Guid _noteDeleteId;
    
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CloudScribeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CloudScribeDbContext(options);
        var note = Note.Create("test", "test");
        _noteReadId = note.Id;
        _dbContext.Notes.Add(note);
        var noteToDelete = Note.Create("test delete", "test delete");
        _noteDeleteId = noteToDelete.Id;
        _dbContext.SaveChanges();
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
        var createdNote = await _service.Create("test", "a".PadRight(Note.MaxContentLength, 'a'));
        createdNote.ShouldSatisfyAllConditions(
            () => createdNote.ShouldNotBeNull(),
            () => createdNote.Content.Length.ShouldBe(Note.MaxContentLength));
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

    public async Task DeleteNote_ShouldDeleteNoteAndReturnTrue()
    {
        var result = await _service.Delete(_noteDeleteId);
        
        result.ShouldSatisfyAllConditions(
            () => result.ShouldBeTrue(),
            () => _dbContext.Notes.Find(_noteDeleteId).ShouldBeNull());
    }
    
    [Test]
    public async Task DeleteNote_ShouldReturnFalseWhenNoteNotFound()
    {
        var result = await _service.Delete(Guid.NewGuid());
        
        result.ShouldBeFalse();
    }

    [Test]
    public async Task GetNoteById_ShouldReturnNoteWhenFound()
    {
        var note = await _service.GetById(_noteReadId);

        note.ShouldNotBeNull();
        note!.Id.ShouldBe(_noteReadId);
    }
    
    [Test]
    public async Task GetNoteById_ShouldReturnNullWhenNotFound()
    {
        var note = await _service.GetById(Guid.NewGuid());
        
        note.ShouldBeNull();
    }

    [Test]
    public async Task GetAllNotes_ShouldReturnAllNotes()
    {
        var notes = await _service.GetAll();
        notes.ShouldNotBeNull();
        notes.Items.Count().ShouldBeGreaterThan(0);
    }
    
    [TestCase]
    [TearDown]
    public async Task TearDown()
    {
        await _dbContext.DisposeAsync();
    }
    
}
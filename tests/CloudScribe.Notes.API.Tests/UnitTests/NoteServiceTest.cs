using CloudScribe.Notes.API.Domain;
using CloudScribe.Notes.API.Infrastructure.Auth;
using CloudScribe.Notes.API.Infrastructure.Data;
using CloudScribe.Notes.API.Services;
using CloudScribe.Notes.API.Tests.Helpers;
using CloudScribe.SharedKernel;
using NSubstitute;

namespace CloudScribe.Notes.API.Tests.UnitTests;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.Children)]
public class NoteServiceTest
{
    private CloudScribeDbContext _dbContext;
    private NotesService _service = null!;
    private ICurrentUser _currentUser = null!;
    private Guid _testUserId;
    private Guid _otherUserId;
    private Guid _noteReadId;
    private Guid _noteDeleteId;
    private Guid _otherUserNoteId;
    private Guid _noteToEditId;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CloudScribeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CloudScribeDbContext(options);
        _testUserId = Guid.Parse(TestConst.TestUserId);
        _otherUserId = Guid.Parse(TestConst.OtherUserId);

        _currentUser = Substitute.For<ICurrentUser>();
        _currentUser.Id.Returns(_testUserId);

        var note = Note.Create("test", "test", _testUserId);
        _noteReadId = note.Id;
        _dbContext.Notes.Add(note);
        var noteToEdit = Note.Create("test", "test", _testUserId);
        _noteToEditId = noteToEdit.Id;
        _dbContext.Notes.Add(noteToEdit);
        var noteToDelete = Note.Create("test delete", "test delete", _testUserId);
        _noteDeleteId = noteToDelete.Id;
        _dbContext.Notes.Add(noteToDelete);

        var otherUserNote = Note.Create("other user note", "other user content", _otherUserId);
        _otherUserNoteId = otherUserNote.Id;
        _dbContext.Notes.Add(otherUserNote);

        _dbContext.SaveChanges();
        _service = new NotesService(_dbContext, _currentUser);
    }

    [TestCase("test", "test")]
    [TestCase("test multi words", "test multi words")]
    public async Task AddNote_ShouldReturnResultNoteWithTitleAndContent(string title, string content)
    {
        var result = await _service.Create(title, content);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Value!.Title.ShouldBe(title),
            () => result.Value!.Content.ShouldBe(content));
    }

    [Test]
    public async Task AddNote_ShouldCreateWhenMaxTitleLengthIsReached()
    {
        var result = await _service.Create("a".PadRight(Note.MaxTitleLength, 'a'), "test");

        result.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public async Task AddNote_ShouldCreateWhenMaxContentLengthIsReached()
    {
        var result = await _service.Create("test", "a".PadRight(Note.MaxContentLength, 'a'));
        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Value!.Content.Length.ShouldBe(Note.MaxContentLength));
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

    [Test]
    public async Task DeleteNote_ShouldDeleteNoteAndReturnTrue()
    {
        var result = await _service.Delete(_noteDeleteId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => _dbContext.Notes.Find(_noteDeleteId).ShouldBeNull());
    }

    [Test]
    public async Task DeleteNote_ShouldReturnResultWithNotFoundError()
    {
        var result = await _service.Delete(Guid.NewGuid());
        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.Error!.Type.ShouldBe(ErrorType.NotFound));
    }

    [Test]
    public async Task DeleteNote_ShouldReturnForbiddenError_WhenUserIsNotOwner()
    {
        var result = await _service.Delete(_otherUserNoteId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.Error!.Type.ShouldBe(ErrorType.Forbidden),
            () => result.Error!.Code.ShouldBe("Notes.Forbidden"));
    }

    [Test]
    public async Task GetNoteById_ShouldReturnNoteWhenFound()
    {
        var note = await _service.GetById(_noteReadId);

        note.ShouldSatisfyAllConditions(
            () => note.IsSuccess.ShouldBeTrue(),
            () => note.Value!.Id.ShouldBe(_noteReadId));
    }

    [Test]
    public async Task GetNoteById_ShouldReturnNotFoundError()
    {
        var result = await _service.GetById(Guid.NewGuid());

        result.ShouldSatisfyAllConditions(
        () => result.IsSuccess.ShouldBeFalse(),
        () => result.Error!.Type.ShouldBe(ErrorType.NotFound)
            );
    }

    [Test]
    public async Task GetNoteById_ShouldReturnForbiddenError_WhenUserIsNotOwner()
    {
        var result = await _service.GetById(_otherUserNoteId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.Error!.Type.ShouldBe(ErrorType.Forbidden),
            () => result.Error!.Code.ShouldBe("Notes.Forbidden"));
    }

    [Test]
    public async Task GetAllNotes_ShouldReturnAllNotes()
    {
        var notes = await _service.GetAll();

        notes.IsSuccess.ShouldBeTrue();
        notes.Value!.Items.Count().ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task UpdateNote_ShouldUpdateNoteSuccessfully()
    {
        var result = await _service.Update(_noteToEditId, "updated title", "updated content");

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Value!.Title.ShouldBe("updated title"),
            () => result.Value!.Content.ShouldBe("updated content"));
    }

    [Test]
    public async Task UpdateNote_ShouldReturnNotFoundError_WhenNoteDoesNotExist()
    {
        var result = await _service.Update(Guid.NewGuid(), "title", "content");

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.Error!.Type.ShouldBe(ErrorType.NotFound));
    }

    [Test]
    public async Task UpdateNote_ShouldReturnForbiddenError_WhenUserIsNotOwner()
    {
        var result = await _service.Update(_otherUserNoteId, "updated title", "updated content");

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.Error!.Type.ShouldBe(ErrorType.Forbidden),
            () => result.Error!.Code.ShouldBe("Notes.Forbidden"));
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dbContext.DisposeAsync();
    }
}
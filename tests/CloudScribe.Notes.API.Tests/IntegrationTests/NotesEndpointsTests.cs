using System.Net;
using System.Net.Http.Json;
using CloudScribe.Contracts.Notes;
using CloudScribe.Notes.API.Domain;
using CloudScribe.SharedKernel;
using Shouldly;

namespace CloudScribe.Notes.API.Tests.IntegrationTests;

[TestFixture]
[Parallelizable(ParallelScope.Fixtures)]
public class NotesEndpointsTests : BaseIntegrationTest
{

    [Test]
    public async Task GetAllNotes_ReturnsEmptyList_WhenNoNotesExist()
    {
        var response = await Client.GetAsync("/api/notes");
        
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Note>>();
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Test]
    public async Task CreateNote_ReturnsCreated_WithValidRequest()
    {
        var request = new CreateNoteRequest("Test Title", "Test Content");
        
        var response = await Client.PostAsJsonAsync("/api/notes", request);
        
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var note = await response.Content.ReadFromJsonAsync<NoteDto>();
        note.ShouldNotBeNull();
        note.Title.ShouldBe("Test Title");
        note.Content.ShouldBe("Test Content");
        note.Id.ShouldNotBe(Guid.Empty);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location!.ToString().ShouldContain($"/api/notes/{note.Id}");
    }

    [Test]
    public async Task CreateNote_ReturnsBadRequest_WhenTitleIsEmpty()
    {
        var request = new CreateNoteRequest("", "Test Content");
        
        var response = await Client.PostAsJsonAsync("/api/notes", request);
        
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateNote_ReturnsBadRequest_WhenTitleIsTooLong()
    {

        var request = new CreateNoteRequest(new string('a', 201), "Test Content");
        
        var response = await Client.PostAsJsonAsync("/api/notes", request);
        
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateNote_ReturnsBadRequest_WhenContentIsEmpty()
    {

        var request = new CreateNoteRequest("Test Title", "");


        var response = await Client.PostAsJsonAsync("/api/notes", request);


        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetNoteById_ReturnsNote_WhenNoteExists()
    {

        var createRequest = new CreateNoteRequest("Test Title", "Test Content");
        var createResponse = await Client.PostAsJsonAsync("/api/notes", createRequest);
        var createdNote = await createResponse.Content.ReadFromJsonAsync<NoteDto>();


        var response = await Client.GetAsync($"/api/notes/{createdNote!.Id}");


        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var note = await response.Content.ReadFromJsonAsync<NoteDto>();
        note.ShouldNotBeNull();
        note.Id.ShouldBe(createdNote.Id);
        note.Title.ShouldBe("Test Title");
        note.Content.ShouldBe("Test Content");
    }

    [Test]
    public async Task GetNoteById_ReturnsNotFound_WhenNoteDoesNotExist()
    {

        var nonExistentId = Guid.NewGuid();


        var response = await Client.GetAsync($"/api/notes/{nonExistentId}");


        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateNote_ReturnsOk_WithUpdatedNote()
    {

        var createRequest = new CreateNoteRequest("Original Title", "Original Content");
        var createResponse = await Client.PostAsJsonAsync("/api/notes", createRequest);
        var createdNote = await createResponse.Content.ReadFromJsonAsync<NoteDto>();

        var updateRequest = new UpdateNoteRequest("Updated Title", "Updated Content");


        var response = await Client.PutAsJsonAsync($"/api/notes/{createdNote!.Id}", updateRequest);


        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedNote = await response.Content.ReadFromJsonAsync<NoteDto>();
        updatedNote.ShouldNotBeNull();
        updatedNote.Id.ShouldBe(createdNote.Id);
        updatedNote.Title.ShouldBe("Updated Title");
        updatedNote.Content.ShouldBe("Updated Content");
        updatedNote.ModifiedUtc.ShouldBeGreaterThan(createdNote.CreatedUtc);
    }

    [Test]
    public async Task UpdateNote_ReturnsNotFound_WhenNoteDoesNotExist()
    {

        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateNoteRequest("Updated Title", "Updated Content");


        var response = await Client.PutAsJsonAsync($"/api/notes/{nonExistentId}", updateRequest);


        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateNote_ReturnsBadRequest_WhenTitleIsEmpty()
    {

        var createRequest = new CreateNoteRequest("Original Title", "Original Content");
        var createResponse = await Client.PostAsJsonAsync("/api/notes", createRequest);
        //var jsonString = await createResponse.Content.ReadAsStringAsync();
        var createdNote = await createResponse.Content.ReadFromJsonAsync<NoteDto>();

        var updateRequest = new UpdateNoteRequest("", "Updated Content");


        var response = await Client.PutAsJsonAsync($"/api/notes/{createdNote!.Id}", updateRequest);


        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task DeleteNote_ReturnsNoContent_WhenNoteExists()
    {

        var createRequest = new CreateNoteRequest("Test Title", "Test Content");
        var createResponse = await Client.PostAsJsonAsync("/api/notes", createRequest);
        var createdNote = await createResponse.Content.ReadFromJsonAsync<NoteDto>();


        var response = await Client.DeleteAsync($"/api/notes/{createdNote!.Id}");


        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify note is actually deleted
        var getResponse = await Client.GetAsync($"/api/notes/{createdNote.Id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteNote_ReturnsNotFound_WhenNoteDoesNotExist()
    {

        var nonExistentId = Guid.NewGuid();


        var response = await Client.DeleteAsync($"/api/notes/{nonExistentId}");


        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetAllNotes_ReturnsPaginatedResults()
    {
        for (int i = 1; i <= 5; i++)
        {
            var request = new CreateNoteRequest($"Note {i}", $"Content {i}");
            await Client.PostAsJsonAsync("/api/notes", request);
        }


        var response = await Client.GetAsync("/api/notes?pageNumber=1&pageSize=3");


        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<NoteDto>>();
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(3);
        result.TotalCount.ShouldBe(5);
        result.PageNumber.ShouldBe(1);
        result.PageSize.ShouldBe(3);
        result.TotalPages.ShouldBe(2);
    }

    [Test]
    public async Task GetAllNotes_ReturnsNotesOrderedByCreatedDateDescending()
    {
        var note1 = new CreateNoteRequest("First Note", "Content 1");
        var note2 = new CreateNoteRequest("Second Note", "Content 2");
        var note3 = new CreateNoteRequest("Third Note", "Content 3");

        await Client.PostAsJsonAsync("/api/notes", note1);
        await Task.Delay(100);
        await Client.PostAsJsonAsync("/api/notes", note2);
        await Task.Delay(100);
        await Client.PostAsJsonAsync("/api/notes", note3);
        
        var response = await Client.GetAsync("/api/notes");
        
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<NoteDto>>();
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBeGreaterThanOrEqualTo(3);
        
        var notes = result.Items.ToList();
        notes[0].Title.ShouldBe("Third Note");
        notes[1].Title.ShouldBe("Second Note");
        notes[2].Title.ShouldBe("First Note");
    }
}

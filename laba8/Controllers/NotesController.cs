using laba8.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using laba8.Models;
using System.Linq.Expressions;
using laba8.DataAccess;
namespace laba8.Controllers;

[ApiController]
[Route("[controller]")]
public class NotesController : ControllerBase
{
    private readonly NotesDbContext _dbContext;
    public NotesController(NotesDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoteRequest request, CancellationToken ct)
    {
        var note = new Note(request.Title, request.Description);
        await _dbContext.Notes.AddAsync(note, ct);
        await _dbContext.SaveChangesAsync(ct);

        return Ok();
    }

    private Expression<Func<Note, object>> GetSelectorKey(string sortItem)
    {
        return sortItem switch
        {
            "createdAt" => note => note.CreatedAt,
            "title" => note => note.Title,
            _ => note => note.Id
        };
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetNotesRequest request, CancellationToken ct)
    {
        var notesQuery = _dbContext.Notes
            .Where(n => string.IsNullOrWhiteSpace(request.Search) ||
            n.Title.ToLower().Contains(request.Search.ToLower()));
        Expression<Func<Note, object>> selectorKey = (request.SortItem?.ToLower()) switch
        {
            "date" => note => note.CreatedAt,
            "title" => note => note.Title,
            _ => note => note.Id
        };
        notesQuery = request.SortOrder == "desc"
        ? notesQuery.OrderByDescending(selectorKey)
        : notesQuery.OrderBy(selectorKey);
        var noteDtos = await notesQuery
                .Select(n => new NoteDto(n.Id, n.Title, n.Description, n.CreatedAt))
                .ToListAsync(cancellationToken: ct);
        return Ok(new GetNotesResponse(noteDtos));
    }
}
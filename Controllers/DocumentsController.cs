using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudySummarizer.Data;
using StudySummarizer.Models;

namespace StudySummarizer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private const long MaxFileSize = 20 * 1024 * 1024;

    private static readonly string[] AllowedExtensions = [".pdf", ".docx", ".txt"];

    private readonly AppDbContext _db;

    public DocumentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<DocumentResponse>> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file sent or file is empty.");

        if (file.Length > MaxFileSize)
            return BadRequest($"File too large. Maximum {MaxFileSize / 1024 / 1024} MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return BadRequest($"Invalid type. Accepted: {string.Join(", ", AllowedExtensions)}");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            Content = memoryStream.ToArray(),
            Status = DocumentStatus.Uploaded,
            UploadedAt = DateTime.UtcNow
        };

        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        var response = ToResponse(document);
        return CreatedAtAction(nameof(GetById), new { id = document.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetAll()
    {
        var documents = await _db.Documents
            .Include(d => d.Summary)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

        return Ok(documents.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentResponse>> GetById(Guid id)
    {
        var document = await _db.Documents
            .Include(d => d.Summary)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document is null)
            return NotFound($"Document {id} was not found.");

        return Ok(ToResponse(document));
    }

    [HttpGet("{id:guid}/file")]
    public async Task<IActionResult> Download(Guid id)
    {
        var document = await _db.Documents.FindAsync(id);
        if (document is null)
            return NotFound($"Document {id} was not found.");

        return File(document.Content, document.ContentType, document.FileName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var document = await _db.Documents.FindAsync(id);
        if (document is null)
            return NotFound($"Document {id} was not found.");

        _db.Documents.Remove(document);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static DocumentResponse ToResponse(Document d) => new(
        d.Id,
        d.FileName,
        d.ContentType,
        d.SizeBytes,
        d.Status,
        d.UploadedAt,
        d.Summary is not null
    );
}

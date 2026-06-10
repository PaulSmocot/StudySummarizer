using Microsoft.AspNetCore.Mvc;
using StudySummarizer.Service.Dtos;
using StudySummarizer.Service.Interfaces;

namespace StudySummarizer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documents;

    public DocumentsController(IDocumentService documents)
    {
        _documents = documents;
    }

    [HttpPost]
    public async Task<ActionResult<DocumentResponse>> Upload(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        if (file is not null)
            await file.CopyToAsync(memoryStream);

        var input = new UploadFileInput(
            file?.FileName ?? string.Empty,
            file?.ContentType ?? string.Empty,
            file?.Length ?? 0,
            memoryStream.ToArray()
        );

        var response = await _documents.UploadAsync(input);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetAll()
    {
        return Ok(await _documents.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentResponse>> GetById(Guid id)
    {
        var document = await _documents.GetByIdAsync(id);
        return document is null ? NotFound($"Document {id} was not found.") : Ok(document);
    }

    [HttpGet("{id:guid}/file")]
    public async Task<IActionResult> Download(Guid id)
    {
        var file = await _documents.GetFileAsync(id);
        return file is null
            ? NotFound($"Document {id} was not found.")
            : File(file.Content, file.ContentType, file.FileName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _documents.DeleteAsync(id);
        return deleted ? NoContent() : NotFound($"Document {id} was not found.");
    }
}

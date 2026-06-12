using Microsoft.AspNetCore.Mvc;
using StudySummarizer.Service.Dtos;
using StudySummarizer.Service.Interfaces;

namespace StudySummarizer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
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

        var response = await _documentService.UploadAsync(input);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetAll()
    {
        return Ok(await _documentService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentResponse>> GetById(Guid id)
    {
        var document = await _documentService.GetByIdAsync(id);
        return document is null ? NotFound($"Document {id} was not found.") : Ok(document);
    }

    [HttpGet("{id:guid}/file")]
    public async Task<IActionResult> Download(Guid id)
    {
        var file = await _documentService.GetFileAsync(id);
        return file is null
            ? NotFound($"Document {id} was not found.")
            : File(file.Content, file.ContentType, file.FileName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _documentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound($"Document {id} was not found.");
    }
}

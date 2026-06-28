using DataForge.Models;
using DataForge.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataForge.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransformController : ControllerBase
{
    private readonly IFileTransformService _service;
    private static readonly string[] AllowedExtensions = [".csv", ".json", ".xml"];

    public TransformController(IFileTransformService service)
    {
        _service = service;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Transform(
        IFormFile file,
        [FromForm] string? filter = null,
        [FromForm] string? selectColumns = null,
        [FromForm] string? renameColumns = null,
        [FromForm] string outputFormat = "json")
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest($"Unsupported file type '{ext}'. Use: {string.Join(", ", AllowedExtensions)}");

        var request = new TransformRequest
        {
            Filter = filter,
            SelectColumns = selectColumns?.Split(',').Select(c => c.Trim()).ToList(),
            RenameColumns = ParseRenameColumns(renameColumns),
            OutputFormat = outputFormat
        };

        using var stream = file.OpenReadStream();
        var result = await _service.TransformAsync(stream, file.FileName, request);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok", timestamp = DateTime.UtcNow });

    private static Dictionary<string, string>? ParseRenameColumns(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return raw.Split(',')
            .Select(p => p.Split(':'))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0].Trim(), p => p[1].Trim());
    }
}

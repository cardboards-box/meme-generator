using CardboardBox.Extensions.AspNetCore;
using ImageBox;
using MemeGenerator.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemeGenerator.Web.Controllers;

public class MediaController(
    ITemplateWatchService _templates,
    IMemeGeneratorService _memes,
    ILogger<MediaController> logger) : BaseController(logger)
{
    [HttpGet, Route("api/media/templates/{id}")]
    [ProducesDefaultResponseType(typeof(FileResult))]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = Extensions.CACHE_AGE_YEAR)]
    public IActionResult Example([FromRoute] Guid id)
    {
        var template = _templates.FindTemplate(id);
        if (template is null ||
            string.IsNullOrEmpty(template.ExamplePath) ||
            !System.IO.File.Exists(template.ExamplePath))
            return NotFound();

        var io = System.IO.File.OpenRead(template.ExamplePath);
        var mime = MimeTypes.GetMimeType(template.ExamplePath);
        return File(io, mime, Path.GetFileName(template.ExamplePath));
    }

    [HttpGet, Route("api/media/templates/{id}/script")]
    [ProducesDefaultResponseType(typeof(FileResult))]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = Extensions.CACHE_AGE_YEAR)]
    public IActionResult Script([FromRoute] Guid id)
    {
        var template = _templates.FindTemplate(id);
        if (template is null ||
            string.IsNullOrEmpty(template.FilePath) ||
            !System.IO.File.Exists(template.FilePath))
            return NotFound();
        var io = System.IO.File.OpenRead(template.FilePath);
        var mime = "text/html";
        return File(io, mime, Path.GetFileName(template.FilePath));
    }

    [HttpGet, Route("api/media/memes/{id}")]
    [ProducesDefaultResponseType(typeof(FileResult))]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = Extensions.CACHE_AGE_YEAR)]
    public IActionResult Meme([FromRoute] Guid id)
    {
        var path = _memes.GetPath(id);
        if (string.IsNullOrEmpty(path) ||
            !System.IO.File.Exists(path))
            return NotFound();

        var io = System.IO.File.OpenRead(path);
        var mime = MimeTypes.GetMimeType(path);
        return File(io, mime, Path.GetFileName(path));
    }
}

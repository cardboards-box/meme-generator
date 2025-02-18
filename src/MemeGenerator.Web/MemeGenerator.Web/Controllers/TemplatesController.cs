using CardboardBox.Extensions.AspNetCore;
using CardboardBox.Extensions.Requesting;
using MemeGenerator.Models;
using MemeGenerator.Services;
using MemeGenerator.Web.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemeGenerator.Web.Controllers;

public class TemplatesController(
    IApi _api,
    ILogger<TemplatesController> logger) : BaseController(logger)
{
    [HttpGet, Route("api/templates/all")]
    [ProducesArray<MemeTemplate>]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public Task<IActionResult> All() => Box(async () => (Boxed)(await _api.Templates.Get())!);

    [HttpGet, Route("api/templates/{id}")]
    [Produces<MemeTemplate>]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public Task<IActionResult> Fetch(Guid id) => Box(async () => (Boxed)(await _api.Templates.Fetch(id))!);

}

internal class TemplatesApiService(
    ITemplateWatchService _templates) : ITemplatesApiService
{
    public Task<Boxed<MemeTemplate>?> Fetch(Guid id)
    {
        var template = _templates.FindTemplate(id);

        Boxed<MemeTemplate>? result;
        if (template is null)
            result = Boxed.NotFound<MemeTemplate>("template");
        else
            result = Boxed.Ok(template.Template);

        return Task.FromResult(result)!;
    }

    public Task<BoxedArray<MemeTemplate>?> Get()
    {
        var templates = _templates.Templates;
        var result = Boxed.Ok(templates);
        return Task.FromResult(result)!;
    }

    public async Task<string?> Script(Guid id)
    {
        var template = _templates.FindTemplate(id);
        if (template is null ||
            string.IsNullOrEmpty(template.FilePath) ||
            !File.Exists(template.FilePath)) return null;

        return await File.ReadAllTextAsync(template.FilePath);
    }
}
using CardboardBox.Extensions.AspNetCore;
using CardboardBox.Extensions.Requesting;
using MemeGenerator.Models;
using MemeGenerator.Services;
using MemeGenerator.Web.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemeGenerator.Web.Controllers;

public class MemesController(
    IApi _api,
    ILogger<MemesController> logger) : BaseController(logger)
{
    [HttpGet, Route("api/memes")]
    [ProducesPaged<MemeResult>]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int size = 10) 
        => Box(async () => (Boxed)(await _api.Memes.Recent(page, size))!);

    [HttpGet, Route("api/memes/{id}")]
    [Produces<MemeResult>]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public Task<IActionResult> Get([FromRoute] Guid id) => Box(async () => (Boxed)(await _api.Memes.Get(id))!);

    [HttpPost, Route("api/memes")]
    [Produces<MemeResult>]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public Task<IActionResult> Post([FromBody] MemeRequest request) => Box(async () => (Boxed)(await _api.Memes.Post(request))!);
}

internal class MemesApiService(
    IMemeGeneratorService _memes) : IMemesApiService
{
    public Task<Boxed<MemeResult>?> Get(Guid id)
    {
        var result = _memes.FindResult(id);
        Boxed<MemeResult>? boxed;
        if (result is null)
            boxed = Boxed.NotFound<MemeResult>("result", id.ToString());
        else
            boxed = Boxed.Ok(result);
        return Task.FromResult(boxed)!;
    }

    public Task<Boxed<MemeResult>?> Post(MemeRequest request)
    {
        var result = _memes.Queue(request);
        return Task.FromResult(Boxed.Ok(result))!;
    }

    public Task<BoxedPaged<MemeResult>?> Recent(int page = 1, int size = 10)
    {
        BoxedPaged<MemeResult> BadResult(string reason)
        {
            return new BoxedPaged<MemeResult>([], 0, 0, System.Net.HttpStatusCode.BadRequest, Boxed.ERROR)
            {
                Errors = [reason]
            };
        }

        BoxedPaged<MemeResult> Do()
        {
            if (page <= 0) return BadResult("Page must be greater than 0");
            if (size <= 0) return BadResult("Size must be greater than 0");
            if (size > 50) return BadResult("Size cannot be greater than 50");

            var results = _memes.Recent(page, size).ToArray();
            var total = _memes.TotalCount;
            var pages = (int)Math.Ceiling(total / (double)size);
            return new BoxedPaged<MemeResult>(results, pages, total);
        }

        var result = Do();
        return Task.FromResult(result)!;
    }
}

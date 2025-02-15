using CardboardBox.Extensions.Requesting;
using CardboardBox.Http;
using MemeGenerator.Models;

namespace MemeGenerator.Web.Client.Services;

public interface ITemplatesApiService
{
    Task<BoxedArray<MemeTemplate>?> Get();

    Task<Boxed<MemeTemplate>?> Fetch(Guid id);
}

internal class TemplatesApiService(
    IClientApiService _api) : ITemplatesApiService
{
    public Task<Boxed<MemeTemplate>?> Fetch(Guid id)
    {
        return _api.Get<Boxed<MemeTemplate>>($"templates/{id}");
    }

    public Task<BoxedArray<MemeTemplate>?> Get()
    {
        return _api.Get<BoxedArray<MemeTemplate>>("templates/all");
    }
}

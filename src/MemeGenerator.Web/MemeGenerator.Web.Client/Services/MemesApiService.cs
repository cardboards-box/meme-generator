using CardboardBox.Extensions.Requesting;
using CardboardBox.Http;
using MemeGenerator.Models;

namespace MemeGenerator.Web.Client.Services;

public interface IMemesApiService
{
    Task<Boxed<MemeResult>?> Get(Guid id);

    Task<Boxed<MemeResult>?> Post(MemeRequest request);

    Task<BoxedPaged<MemeResult>?> Recent(int page = 1, int size = 10);
}

internal class MemesApiService(
    IClientApiService _api) : IMemesApiService
{
    public Task<Boxed<MemeResult>?> Get(Guid id)
    {
        return _api.Get<Boxed<MemeResult>>($"memes/{id}");
    }

    public Task<Boxed<MemeResult>?> Post(MemeRequest request)
    {
        return _api.Post<Boxed<MemeResult>, MemeRequest>("memes", request);
    }

    public Task<BoxedPaged<MemeResult>?> Recent(int page = 1, int size = 10)
    {
        return _api.Get<BoxedPaged<MemeResult>>($"memes?page={page}&size={size}");
    }
}

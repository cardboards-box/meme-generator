using CardboardBox.Http;
using CardboardBox.Json;
using Microsoft.AspNetCore.Components;

namespace MemeGenerator.Web.Client.Services;

public interface IClientApiService : IApiService { }

public class ClientApiService(
    IJsonService _json,
    IHttpClientFactory _factory,
    NavigationManager _navi) : ApiService(_factory, _json), IClientApiService
{
    public string ApiUrl()
    {
        return $"{_navi.BaseUri.TrimEnd('/')}/api/";
    }

    public override IHttpBuilder Create(string url, IJsonService json, string method)
    {
        var builder = base.Create(url, json, method);
        builder.ClientConfig(c => c.BaseAddress ??= new Uri(ApiUrl()));
        return builder;
    }
}

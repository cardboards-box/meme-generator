using CardboardBox.Json;

namespace MemeGenerator.Web.Client.Services;

public static class DiExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        return services
            .AddTransient<IApi, Api>();
    }

    public static IServiceCollection AddClientServices(this IServiceCollection services)
    {
        return services
            .AddSharedServices()
            .AddJson()
            .AddHttpClient()
            .AddTransient<IClientApiService, ClientApiService>()
            .AddTransient<ITemplatesApiService, TemplatesApiService>()
            .AddTransient<IMemesApiService, MemesApiService>();
    }
}

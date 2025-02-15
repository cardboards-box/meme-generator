using CardboardBox.Extensions.Scripting;
using ImageBox;

namespace MemeGenerator.Services;

public static class DiExtensions
{
    public static IServiceCollection AddMemeServices(this IServiceCollection services, IConfiguration config)
    {
        return services
            .AddSingleton<ITemplateWatchService, TemplateWatchService>()
            .AddSingleton<IMemeGeneratorService, MemeGeneratorService>()
            .AddTemplatingServices()
            .AddImageBox(config);
    }
}

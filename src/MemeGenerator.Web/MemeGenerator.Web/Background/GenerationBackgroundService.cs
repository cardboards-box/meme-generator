namespace MemeGenerator.Web.Background;

using Services;

public class GenerationBackgroundService(
    IMemeGeneratorService _memes) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _memes.Watch(stoppingToken);
    }
}

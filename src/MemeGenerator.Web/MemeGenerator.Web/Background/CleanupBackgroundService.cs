namespace MemeGenerator.Web.Background;

using Services;

public class CleanupBackgroundService(
    IMemeGeneratorService _memes) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _memes.Cleanup(stoppingToken);
    }
}

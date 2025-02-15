namespace MemeGenerator.Web.Background;

using Services;

public class TemplatesBackgroundService(
    ITemplateWatchService _templates) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _templates.Watch(stoppingToken);
    }
}

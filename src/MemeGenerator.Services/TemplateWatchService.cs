using CardboardBox.Extensions.Scripting.Tokening;

namespace MemeGenerator.Services;

using Models;

public interface ITemplateWatchService
{
    MemeTemplate[] Templates { get; }

    LoadedMemeTemplate? FindTemplate(Guid id);

    Task Watch(CancellationToken token);
}

public class TemplateWatchService(
    IConfiguration _config,
    ITokenService _token,
    ILogger<TemplateWatchService> _logger) : ITemplateWatchService
{
    private readonly ConcurrentDictionary<Guid, LoadedMemeTemplate> _templates = [];
    private readonly TokenParserConfig _tokenConfig = new("<!-- CONFIG:", "-->", "\\");
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    private bool _watching = false;

    public string TemplatesPath => _config["Memes:TemplatesDir"] 
        ?? throw new NullReferenceException("Memes:TemplatesDir is not set in the configuration");
    public string Filter => _config["Memes:TemplatesFilter"] ?? "*.ib";

    public MemeTemplate[] Templates => _templates.Select(t => t.Value.Template).ToArray();

    public LoadedMemeTemplate? FindTemplate(Guid id)
    {
        return _templates.TryGetValue(id, out var template) ? template : null;
    }

    public LoadedMemeTemplate? FindTemplate(string path)
    {
        return _templates.Values.FirstOrDefault(t => t.FilePath == path);
    }

    public async Task LoadFile(string filePath)
    {
        string[] possibleExtensions = ["png", "jpg", "jpeg", "gif"];
        try
        {
            var id = FindTemplate(filePath)?.Template.Id ?? Guid.NewGuid();

            var text = await File.ReadAllTextAsync(filePath);
            var config = _token.ParseTokens(text, _tokenConfig).FirstOrDefault();
            if (config is null || string.IsNullOrEmpty(config.Content))
            {
                _logger.LogWarning("Failed to find config in template file {filePath}", filePath);
                return;
            }

            var template = JsonSerializer.Deserialize<MemeTemplate>(config.Content, _options);
            if (template is null)
            {
                _logger.LogWarning("Failed to deserialize template from file {filePath}", filePath);
                return;
            }

            var dir = Path.GetDirectoryName(filePath) ?? string.Empty;
            var name = Path.GetFileNameWithoutExtension(filePath).Trim('.');
            var example = possibleExtensions
                .Select(ext => Path.Combine(dir, $"{name}.{ext}"))
                .FirstOrDefault(File.Exists);

            template.Id = id;
            var loaded = new LoadedMemeTemplate(template, filePath, example);
            _templates.AddOrUpdate(id, loaded, (_, _) => loaded);
            _logger.LogInformation("Loaded template {name} from {filePath}", template.Name, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load template file {filePath}", filePath);
        }
    }

    public async Task SetupWatcher(CancellationToken token)
    {
        if (_watching) return;

        _watching = true;

        using var watcher = new FileSystemWatcher();
        watcher.Changed += (_, e) => _ = LoadFile(e.FullPath);
        watcher.Path = TemplatesPath;
        watcher.Filter = Filter;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        await Task.Delay(-1, token);
    }

    public async Task Watch(CancellationToken token)
    {
        var paths = Directory.GetFiles(TemplatesPath, Filter, SearchOption.AllDirectories);
        foreach (var path in paths)
            await LoadFile(path);

        await SetupWatcher(token);
    }
}

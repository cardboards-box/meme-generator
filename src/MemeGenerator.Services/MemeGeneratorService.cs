using ImageBox;

namespace MemeGenerator.Services;

using Models;

/// <summary>
/// A service for generating memes
/// </summary>
public interface IMemeGeneratorService
{
    /// <summary>
    /// The number of items currently in the generation queue
    /// </summary>
    int QueueCount { get; }

    /// <summary>
    /// The total number of finished requests
    /// </summary>
    int TotalCount { get; }

    /// <summary>
    /// Gets a certain number of recent generations
    /// </summary>
    /// <param name="page">The page of generations</param>
    /// <param name="size">The size of a page</param>
    /// <returns>The recent generations</returns>
    IEnumerable<MemeResult> Recent(int page, int size);

    /// <summary>
    /// Finds a <see cref="MemeResult"/> by it's ID
    /// </summary>
    /// <param name="id">The ID of the result</param>
    /// <returns>The <see cref="MemeResult"/> if it exists</returns>
    MemeResult? FindResult(Guid id);

    /// <summary>
    /// Gets the path of a results file
    /// </summary>
    /// <param name="id">The ID of the results file</param>
    /// <returns>The path if it exists</returns>
    string? GetPath(Guid id);

    /// <summary>
    /// Enqueue a new meme request
    /// </summary>
    /// <param name="request">The request to enqueue</param>
    /// <returns>The result of the queue request</returns>
    /// <exception cref="NullReferenceException">Thrown if the meme generator is not running</exception>
    MemeResult Queue(MemeRequest request);

    /// <summary>
    /// [BACKGROUND TASK] Watch the generation queue for new memes
    /// </summary>
    /// <param name="token">When to stop watching the queue</param>
    Task Watch(CancellationToken token);

    /// <summary>
    /// [BACKGROUND TASK] Cleanup any expired files
    /// </summary>
    /// <param name="token">When to stop cleaning the files</param>
    Task Cleanup(CancellationToken token);
}

/// <summary>
/// The implementation of the <see cref="IMemeGeneratorService"/>
/// </summary>
/// <param name="_config">The configuration of the application</param>
/// <param name="_image">The image box generation service</param>
/// <param name="_templates">The service containing all of our templates</param>
/// <param name="_logger">The logger for the service</param>
internal class MemeGeneratorService(
    IConfiguration _config,
    IImageBoxService _image,
    ITemplateWatchService _templates,
    ILogger<MemeGeneratorService> _logger) : IMemeGeneratorService
{
    private int? _generators;
    private double? _expireMins;
    private string? _outputDir;
    private readonly ConcurrentDictionary<Guid, MemeResult> _finished = [];
    private readonly ConcurrentDictionary<Guid, MemeResult> _queued = [];
    private readonly ConcurrentQueue<MemeResult> _queue = [];

    /// <summary>
    /// The number of threads to create to generate memes on
    /// </summary>
    public int Generators => _generators ??= int.TryParse(_config["Memes:Generators"], out var num) ? num : 10;
    /// <summary>
    /// The number of minutes to save generated memes for
    /// </summary>
    public double ExpireMinutes => _expireMins ??= double.TryParse(_config["Memes:ExpireMinutes"], out var min) ? min : 15;
    /// <summary>
    /// The directory to write generated memes to
    /// </summary>
    public string OutputDirectory => _outputDir ??= _config["Memes:OutputDir"].ForceNull() ?? "./output";
    /// <summary>
    /// The number of items currently in the generation queue
    /// </summary>
    public int QueueCount => _queue.Count;
    /// <summary>
    /// The total number of finished requests
    /// </summary>
    public int TotalCount => _finished.Values.Count(t => t.State == StateType.Completed);

    /// <summary>
    /// Gets a certain number of recent generations
    /// </summary>
    /// <param name="page">The page of generations</param>
    /// <param name="size">The size of a page</param>
    /// <returns>The recent generations</returns>
    public IEnumerable<MemeResult> Recent(int page, int size)
    {
        return _finished.Values
            .Where(t => t.State == StateType.Completed)
            .OrderByDescending(t => t.TimeQueued)
            .Skip((page - 1) * size)
            .Take(size);
    }

    /// <summary>
    /// Finds a <see cref="MemeResult"/> by it's ID
    /// </summary>
    /// <param name="id">The ID of the result</param>
    /// <returns>The <see cref="MemeResult"/> if it exists</returns>
    public MemeResult? FindResult(Guid id)
    {
        return _queued.TryGetValue(id, out var result) 
            ? result 
            : _finished.TryGetValue(id, out result) 
                ? result 
                : null;
    }

    /// <summary>
    /// Moves the result from <see cref="_queued"/> to <see cref="_finished"/> and decrements the position of all other queued items
    /// </summary>
    /// <param name="result">The result to deal with</param>
    public void DecrementPosition(MemeResult result)
    {
        //Move the requests from the queue to the finished dictionaries
        _queued.TryRemove(result.Id, out _);
        _finished.TryAdd(result.Id, result);
        //Update the metadata on the current result
        result.State = StateType.Processing;
        result.Position = 0;
        result.TimeProcessStart = DateTime.UtcNow;
        //Decrement the position of all other queued items
        foreach (var item in _queued.Values)
            item.Position--;
    }

    public string GetWebRootPath()
    {
        if (!Directory.Exists(OutputDirectory))
        {
            Directory.CreateDirectory(OutputDirectory);
            _logger.LogInformation("Creating output directory: {dir}", OutputDirectory);
        }

        return OutputDirectory;
    }

    /// <summary>
    /// Gets the path of a results file
    /// </summary>
    /// <param name="id">The ID of the results file</param>
    /// <returns>The path if it exists</returns>
    public string? GetPath(Guid id)
    {
        var dir = GetWebRootPath();
        var result = FindResult(id);
        if (result is not null)
            return Path.Combine(dir, $"{result.Id}.{(result.Animated ? "gif" : "png")}");

        string[] checks = ["png", "gif"];
        return checks.Select(t => Path.Combine(dir, $"{id}.{t}"))
            .FirstOrDefault(File.Exists);
    }

    /// <summary>
    /// Starts the generation process for a meme result
    /// </summary>
    /// <param name="result">The meme to generate</param>
    /// <exception cref="NullReferenceException">Should never actually be thrown...</exception>
    public async Task Generate(MemeResult result)
    {
        //Update the meta data for the result
        DecrementPosition(result);

        try
        {
            _logger.LogInformation("Starting generation of meme: {id}", result.Id);
            //Get the template for the request
            var request = result.Request;
            var template = _templates.FindTemplate(request.TemplateId);
            if (template is null)
            {
                result.State = StateType.Failed;
                result.Error = "Template not found";
                _logger.LogWarning("Could not generate result: {id} - Template not found", result.Id);
                return;
            }
            //Load the image box context from the template
            var im = _image.Create(template.FilePath);
            //Load the image meta data from the template
            var ctx = await _image.LoadContext(im);
            //Update the meta data for the result
            result.Animated = ctx.Animate;
            result.Width = ctx.Size.Width;
            result.Height = ctx.Size.Height;
            //Get the output path
            var path = GetPath(result.Id) 
                ?? throw new NullReferenceException("Path could not be found - This shouldn't  happen");
            _logger.LogInformation("Starting render of request: {id}", result.Id);
            //Render the image to the output path
            await _image.RenderToFile(path, im, result.Request.ToVariables());
            result.State = StateType.Completed;
            _logger.LogInformation("Finished render of request: {id}", result.Id);
        }
        catch (Exception ex)
        {
            result.State = StateType.Failed;
            result.Error = ex.Message;
            _logger.LogError(ex, "Failed to generate meme {Id}", result.Id);
        }
        finally
        {
            var now = DateTime.UtcNow;
            result.TimeProcessFinished = now;
            result.TimeToLive = now.AddMinutes(ExpireMinutes);
            _logger.LogInformation("Finished generation of meme: {id}", result.Id);
        }
    }

    /// <summary>
    /// [BACKGROUND TASK] Watch the generation queue for new memes
    /// </summary>
    /// <param name="token">When to stop watching the queue</param>
    public async Task Watch(CancellationToken token)
    {
        //Create a cancellation token source to stop the generators
        using var tsc = new CancellationTokenSource();
        token.Register(tsc.Cancel);
        token.Register(_queue.Clear);

        _logger.LogInformation("Starting meme generators");
        //Create a semaphore to ensure we don't run too many generators at a time
        var throttler = new SemaphoreSlim(Generators);
        //How often we should check the queue for more items
        var readTimeout = TimeSpan.FromMilliseconds(250);
        //Keep attempting to read until the background service ends
        while (!tsc.Token.IsCancellationRequested)
        {
            //If the queue is empty, wait a bit before trying again
            if (_queue.IsEmpty)
            {
                await Task.Delay(readTimeout, tsc.Token);
                continue;
            }

            //Wait until we have a generation thread free
            await throttler.WaitAsync(tsc.Token);
            //Try to dequeue an item from the queue
            if (!_queue.TryDequeue(out var item))
            {
                //Couldn't dequeue, so release the semaphore
                throttler.Release();
                continue;
            }
            //Background thread the generation function
            _ = Task.Run(async () =>
            {
                //Trigger the image generation
                await Generate(item);
                //Release the semaphore instance
                throttler.Release();
            }, tsc.Token);
        }
        _logger.LogInformation("Closing meme generators");
    }

    /// <summary>
    /// Cleans up any files that have expired from the queue
    /// </summary>
    public void CleanupFromQueue()
    {
        var now = DateTime.UtcNow;
        foreach (var item in _finished.Values.ToArray())
        {
            if (item.TimeToLive is null || item.TimeToLive > now)
                continue;

            _finished.TryRemove(item.Id, out _);
            var path = GetPath(item.Id);
            if (string.IsNullOrEmpty(path)) continue;

            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file {path}", path);
            }
        }
    }

    /// <summary>
    /// Cleans up any orphaned files from the output directory
    /// </summary>
    /// <remarks>This is mostly for left-over files from previous restarts</remarks>
    public void CleanupOrphaned()
    {
        var files = Directory.GetFiles(GetWebRootPath(), "*.*", SearchOption.AllDirectories);
        foreach(var file in files)
        {
            var info = new FileInfo(file);
            var lastWrite = info.LastWriteTimeUtc;

            if (lastWrite.AddMinutes(ExpireMinutes) > DateTime.UtcNow)
                continue;

            try
            {
                File.Delete(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file {path}", file);
            }
        }
    }

    /// <summary>
    /// [BACKGROUND TASK] Cleanup any expired files
    /// </summary>
    /// <param name="token">When to stop cleaning the files</param>
    public async Task Cleanup(CancellationToken token)
    {
        //Get the timeout for the cleanup
        var timeout = TimeSpan.FromMinutes(ExpireMinutes);
        while (!token.IsCancellationRequested)
        {
            //Do the various clean ups
            CleanupFromQueue();
            CleanupOrphaned();
            //Wait for the timeout
            await Task.Delay(timeout, token);
        }
    }

    /// <summary>
    /// Enqueue a new meme request
    /// </summary>
    /// <param name="request">The request to enqueue</param>
    /// <returns>The result of the queue request</returns>
    /// <exception cref="NullReferenceException">Thrown if the meme generator is not running</exception>
    public MemeResult Queue(MemeRequest request)
    {
        var id = Guid.NewGuid();
        var result = new MemeResult(request, id, DateTime.UtcNow)
        {
            Position = QueueCount + 1,
            State = StateType.Queued,
            Error = null
        };
        _queued.TryAdd(id, result);
        _queue.Enqueue(result);
        return result;
    }
}

using System.Text.Json.Serialization;

namespace MemeGenerator.Models;

/// <summary>
/// The result of a <see cref="MemeRequest"/>
/// </summary>
/// <param name="Request">The meme request</param>
/// <param name="Id">The ID of the file with the result</param>
/// <param name="TimeQueued">The time the image was queued for processing</param>
public record class MemeResult(
    MemeRequest Request,
    Guid Id,
    DateTime TimeQueued)
{
    /// <summary>
    /// The state of the request
    /// </summary>
    public StateType State { get; set; } = StateType.Queued;

    /// <summary>
    /// How long the server will save the image
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? TimeToLive { get; set; }

    /// <summary>
    /// The error message if the request failed
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }

    /// <summary>
    /// The time the image started processing
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? TimeProcessStart { get; set; }

    /// <summary>
    /// The time the image finished processing
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? TimeProcessFinished { get; set; }

    /// <summary>
    /// The position in the queue
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Whether or not the resulting image is animated
    /// </summary>
    public bool Animated { get; set; } = false;

    /// <summary>
    /// The width in pixels
    /// </summary>
    public int Width { get; set; } = 0;

    /// <summary>
    /// The height in pixels
    /// </summary>
    public int Height { get; set; } = 0;
}

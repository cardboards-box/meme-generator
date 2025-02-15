namespace MemeGenerator.Models;

/// <summary>
/// Represents the state of a <see cref="MemeRequest"/>
/// </summary>
public enum StateType
{
    /// <summary>
    /// The request is queued
    /// </summary>
    Queued = 0,
    /// <summary>
    /// The meme is currently being generated
    /// </summary>
    Processing = 1,
    /// <summary>
    /// The meme has been generated
    /// </summary>
    Completed = 2,
    /// <summary>
    /// The meme generation failed
    /// </summary>
    Failed = 3
}

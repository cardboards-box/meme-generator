using System.Text.Json.Serialization;

namespace MemeGenerator.Models;

/// <summary>
/// A input requirement for a meme
/// </summary>
/// <param name="Type">The data type of the requirement</param>
/// <param name="Name">The name of the requirement</param>
/// <param name="Description">The description of the requirement</param>
/// <param name="Default">The default value of the requirement</param>
public record class MemeRequirement(
    DataType Type,
    string Name,
    string Description,
    string? Default)
{
    /// <summary>
    /// The value to use for the requirement
    /// </summary>
    [JsonIgnore]
    public string? Value { get; set; } = Default;
}

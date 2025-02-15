using System.Text.Json.Serialization;

namespace MemeGenerator.Models;

/// <summary>
/// Represents a meme template that can be used
/// </summary>
/// <param name="Name">The name of the template</param>
/// <param name="Description">The description of the template</param>
/// <param name="Inputs">All of the input parameters for the template</param>
public record class MemeTemplate(
    string Name,
    string Description,
    MemeRequirement[] Inputs)
{
    /// <summary>
    /// The ID of the template
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid? Id { get; set; }
}

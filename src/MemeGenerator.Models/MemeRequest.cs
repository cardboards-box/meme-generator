namespace MemeGenerator.Models;

/// <summary>
/// Represents a meme request to generate a meme
/// </summary>
/// <param name="TemplateId">The ID of the <see cref="MemeTemplate"/> to generate</param>
/// <param name="Inputs">The variables used to generate the meme</param>
public record class MemeRequest(
    Guid TemplateId,
    Dictionary<string, string?> Inputs)
{
    /// <summary>
    /// Converts the <see cref="Inputs"/> to a dictionary of variables
    /// </summary>
    /// <returns>The dictionary of variables</returns>
    public Dictionary<string, object?> ToVariables()
    {
        return Inputs.ToDictionary(x => x.Key, x => (object?)x.Value);
    }
}

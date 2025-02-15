namespace MemeGenerator.Models;

/// <summary>
/// Represents a template loaded in memory
/// </summary>
/// <param name="Template">The template that was loaded</param>
/// <param name="FilePath">The path to the template file </param>
/// <param name="ExamplePath">The path to the example image</param>
public record class LoadedMemeTemplate(
    MemeTemplate Template,
    string FilePath,
    string? ExamplePath);

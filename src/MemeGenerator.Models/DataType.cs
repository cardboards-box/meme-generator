namespace MemeGenerator.Models;

/// <summary>
/// Represents the data type that can be used in a meme requirement
/// </summary>
public enum DataType
{
    /// <summary>
    /// String / Text
    /// </summary>
    Text = 0,
    /// <summary>
    /// Whole number
    /// </summary>
    Integer = 1,
    /// <summary>
    /// Decimal number
    /// </summary>
    Decimal = 2,
    /// <summary>
    /// Date
    /// </summary>
    Date = 3,
    /// <summary>
    /// Time
    /// </summary>
    Time = 4,
    /// <summary>
    /// Date and time
    /// </summary>
    DateTime = 5,
    /// <summary>
    /// Image upload
    /// </summary>
    Image = 6,
}

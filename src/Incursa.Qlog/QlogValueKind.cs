namespace Incursa.Qlog;

/// <summary>
/// Identifies the JSON-compatible kind carried by a <see cref="QlogValue"/>.
/// </summary>
public enum QlogValueKind
{
    /// <summary>
    /// The value is `null` or omitted.
    /// </summary>
    Null,

    /// <summary>
    /// The value is a JSON Boolean.
    /// </summary>
    Boolean,

    /// <summary>
    /// The value is a JSON number.
    /// </summary>
    Number,

    /// <summary>
    /// The value is a JSON string.
    /// </summary>
    String,

    /// <summary>
    /// The value is a JSON object.
    /// </summary>
    Object,

    /// <summary>
    /// The value is a JSON array.
    /// </summary>
    Array,
}

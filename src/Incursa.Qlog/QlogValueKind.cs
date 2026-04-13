namespace Incursa.Qlog;

/// <summary>
/// Identifies the JSON-compatible kind carried by a <see cref="QlogValue"/>.
/// </summary>
public enum QlogValueKind
{
    Null,
    Boolean,
    Number,
    String,
    Object,
    Array,
}

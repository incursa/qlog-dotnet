using System.Text.Encodings.Web;
using System.Text.Json;

namespace Incursa.Qlog.Serialization.Json;

/// <summary>
/// Provides JSON writer settings shared by qlog JSON serializers.
/// </summary>
internal static class QlogJsonSerializationHelpers
{
    /// <summary>
    /// Creates the JSON writer options used by qlog serializers.
    /// </summary>
    /// <param name="indented">Indicates whether the output should be indented.</param>
    /// <returns>The writer options.</returns>
    public static JsonWriterOptions CreateWriterOptions(bool indented)
    {
        return new JsonWriterOptions
        {
            Indented = indented,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
    }
}

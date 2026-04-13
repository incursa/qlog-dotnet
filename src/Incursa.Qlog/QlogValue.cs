using System.Buffers;
using System.Text.Json;

namespace Incursa.Qlog;

/// <summary>
/// Carries JSON-compatible extension values without coupling the core model to a concrete serializer API.
/// </summary>
public readonly struct QlogValue : IEquatable<QlogValue>
{
    private readonly JsonElement element;
    private readonly bool hasValue;

    private QlogValue(JsonElement element)
    {
        this.element = element;
        hasValue = true;
    }

    /// <summary>
    /// Gets a qlog null value.
    /// </summary>
    public static QlogValue Null => default;

    /// <summary>
    /// Gets the value kind.
    /// </summary>
    public QlogValueKind Kind =>
        !hasValue
            ? QlogValueKind.Null
            : element.ValueKind switch
            {
                JsonValueKind.Null or JsonValueKind.Undefined => QlogValueKind.Null,
                JsonValueKind.True or JsonValueKind.False => QlogValueKind.Boolean,
                JsonValueKind.Number => QlogValueKind.Number,
                JsonValueKind.String => QlogValueKind.String,
                JsonValueKind.Object => QlogValueKind.Object,
                JsonValueKind.Array => QlogValueKind.Array,
                _ => throw new InvalidOperationException($"Unsupported JSON value kind '{element.ValueKind}'."),
            };

    /// <summary>
    /// Parses an arbitrary JSON value into a <see cref="QlogValue"/>.
    /// </summary>
    /// <param name="json">The JSON value text.</param>
    /// <returns>The parsed qlog value.</returns>
    public static QlogValue Parse(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using JsonDocument document = JsonDocument.Parse(json);
        return FromElement(document.RootElement);
    }

    /// <summary>
    /// Creates a qlog string value.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>The created qlog value.</returns>
    public static QlogValue FromString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return CreateValue(writer => writer.WriteStringValue(value));
    }

    /// <summary>
    /// Creates a qlog Boolean value.
    /// </summary>
    /// <param name="value">The Boolean value.</param>
    /// <returns>The created qlog value.</returns>
    public static QlogValue FromBoolean(bool value) => CreateValue(writer => writer.WriteBooleanValue(value));

    /// <summary>
    /// Creates a qlog integer number value.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <returns>The created qlog value.</returns>
    public static QlogValue FromNumber(long value) => CreateValue(writer => writer.WriteNumberValue(value));

    /// <summary>
    /// Creates a qlog floating-point number value.
    /// </summary>
    /// <param name="value">The floating-point value.</param>
    /// <returns>The created qlog value.</returns>
    public static QlogValue FromNumber(double value) => CreateValue(writer => writer.WriteNumberValue(value));

    /// <summary>
    /// Creates a qlog unsigned integer number value.
    /// </summary>
    /// <param name="value">The unsigned integer value.</param>
    /// <returns>The created qlog value.</returns>
    public static QlogValue FromNumber(ulong value) => CreateValue(writer => writer.WriteNumberValue(value));

    /// <summary>
    /// Creates a qlog object value from property pairs.
    /// </summary>
    /// <param name="properties">The properties to write in order.</param>
    /// <returns>The created qlog value.</returns>
    public static QlogValue FromObject(IEnumerable<KeyValuePair<string, QlogValue>> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        return CreateValue(writer =>
        {
            writer.WriteStartObject();
            foreach (KeyValuePair<string, QlogValue> property in properties)
            {
                ArgumentNullException.ThrowIfNull(property.Key);
                writer.WritePropertyName(property.Key);
                property.Value.WriteTo(writer);
            }

            writer.WriteEndObject();
        });
    }

    /// <summary>
    /// Creates a qlog array value from child values.
    /// </summary>
    /// <param name="values">The values to write in order.</param>
    /// <returns>The created qlog value.</returns>
    public static QlogValue FromArray(IEnumerable<QlogValue> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        return CreateValue(writer =>
        {
            writer.WriteStartArray();
            foreach (QlogValue value in values)
            {
                value.WriteTo(writer);
            }

            writer.WriteEndArray();
        });
    }

    /// <summary>
    /// Returns the JSON text for the carried value.
    /// </summary>
    /// <returns>The JSON representation.</returns>
    public string ToJson() => !hasValue ? "null" : element.GetRawText();

    /// <inheritdoc />
    public override string ToString() => ToJson();

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is QlogValue other && Equals(other);

    /// <inheritdoc />
    public bool Equals(QlogValue other) => string.Equals(ToJson(), other.ToJson(), StringComparison.Ordinal);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(ToJson());

    internal static QlogValue FromElement(JsonElement element) => new(element.Clone());

    internal static QlogValue Create(Action<Utf8JsonWriter> writeValue) => CreateValue(writeValue);

    private static QlogValue CreateValue(Action<Utf8JsonWriter> writeValue)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (Utf8JsonWriter writer = new(buffer))
        {
            writeValue(writer);
        }

        using JsonDocument document = JsonDocument.Parse(buffer.WrittenMemory);
        return FromElement(document.RootElement);
    }

    internal void WriteTo(Utf8JsonWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (!hasValue)
        {
            writer.WriteNullValue();
            return;
        }

        element.WriteTo(writer);
    }
}

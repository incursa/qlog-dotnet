using System.Text.Json;

namespace Incursa.Qlog;

internal static class QlogValueFactory
{
    public static QlogValue String(string value) => QlogValue.FromString(value);

    public static QlogValue Boolean(bool value) => QlogValue.FromBoolean(value);

    public static QlogValue Number(ushort value) => QlogValue.Create(writer => writer.WriteNumberValue(value));

    public static QlogValue Number(ulong value) => QlogValue.Create(writer => writer.WriteNumberValue(value));

    public static QlogValue Object(Action<Utf8JsonWriter> writeObject)
    {
        ArgumentNullException.ThrowIfNull(writeObject);

        return QlogValue.Create(writer =>
        {
            writer.WriteStartObject();
            writeObject(writer);
            writer.WriteEndObject();
        });
    }

    public static QlogValue Array<T>(IEnumerable<T> values, Func<T, QlogValue> convert)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(convert);

        return QlogValue.Create(writer =>
        {
            writer.WriteStartArray();
            foreach (T value in values)
            {
                convert(value).WriteTo(writer);
            }

            writer.WriteEndArray();
        });
    }
}

using System.Text;
using System.Text.Json;

namespace Incursa.Qlog.Serialization.Json;

/// <summary>
/// Serializes sequential qlog JSON Text Sequences artifacts for the draft sequential file slice.
/// </summary>
public static class QlogJsonTextSequenceSerializer
{
    private const byte RecordSeparator = 0x1E;

    /// <summary>
    /// Serializes a sequential qlog file to a JSON Text Sequences string.
    /// </summary>
    /// <param name="file">The file to serialize.</param>
    /// <param name="indented">Indicates whether the output should be indented.</param>
    /// <returns>The serialized JSON Text Sequences text.</returns>
    public static string Serialize(QlogFile file, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(file);

        using MemoryStream stream = new();
        Serialize(stream, file, indented);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Serializes a sequential qlog file to a UTF-8 JSON Text Sequences stream.
    /// </summary>
    /// <param name="stream">The destination stream.</param>
    /// <param name="file">The file to serialize.</param>
    /// <param name="indented">Indicates whether the output should be indented.</param>
    public static void Serialize(Stream stream, QlogFile file, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(file);

        QlogJsonSerializationHelpers.ValidateSequentialFile(file);

        WriteRecord(stream, writer => QlogJsonSerializationHelpers.WriteSequentialFileHeader(writer, file), indented);

        QlogTrace trace = (QlogTrace)file.Traces[0];
        foreach (QlogEvent qlogEvent in trace.Events)
        {
            WriteRecord(stream, writer => QlogJsonSerializationHelpers.WriteEvent(writer, qlogEvent), indented);
        }
    }

    private static void WriteRecord(Stream stream, Action<Utf8JsonWriter> writeRecord, bool indented)
    {
        stream.WriteByte(RecordSeparator);

        using (Utf8JsonWriter writer = new(stream, QlogJsonSerializationHelpers.CreateWriterOptions(indented)))
        {
            writeRecord(writer);
        }

        stream.WriteByte((byte)'\n');
    }
}

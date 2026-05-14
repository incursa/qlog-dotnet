using System.Text;
using Incursa.Qlog.Serialization.Json;

namespace Incursa.Qlog.Import;

/// <summary>
/// Hydrates qlog output back into the retained model for replay-oriented consumers.
/// </summary>
public static class QlogImportSerializer
{
    private const byte Utf8BomFirstByte = 0xEF;
    private const byte Utf8BomSecondByte = 0xBB;
    private const byte Utf8BomThirdByte = 0xBF;
    private const int Utf8BomFirstIndex = 0;
    private const int Utf8BomSecondIndex = 1;
    private const int Utf8BomThirdIndex = 2;
    private const byte Utf8Tab = 0x09;
    private const byte Utf8LineFeed = 0x0A;
    private const byte Utf8CarriageReturn = 0x0D;
    private const byte Utf8Space = 0x20;
    private const byte CborMapMajorTypeMask = 0b1110_0000;
    private const byte CborMapMajorType = 0b1010_0000;
    private const int Utf8BomLength = 3;
    private const char JsonRecordSeparator = '\u001E';
    private const char Utf8BomCharacter = '\uFEFF';

    /// <summary>
    /// Deserializes qlog output from a string and auto-detects the repository's contained JSON or sequential JSON Text
    /// Sequences format.
    /// </summary>
    /// <param name="content">The qlog text to hydrate.</param>
    /// <returns>The parsed qlog file model.</returns>
    public static QlogFile Deserialize(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return content.IndexOf(JsonRecordSeparator) >= 0
            ? QlogSequentialJsonTextSequenceReader.Deserialize(content)
            : QlogJsonSerializer.Deserialize(content);
    }

    /// <summary>
    /// Deserializes qlog output from a UTF-8 or binary stream and auto-detects the repository's contained JSON,
    /// sequential JSON Text Sequences, or contained CBOR format.
    /// </summary>
    /// <param name="stream">The UTF-8 qlog stream to hydrate.</param>
    /// <returns>The parsed qlog file model.</returns>
    public static QlogFile Deserialize(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        byte[] payload = ReadAllBytes(stream);
        return DeserializePayload(payload);
    }

    /// <summary>
    /// Deserializes contained qlog JSON output from a string.
    /// </summary>
    /// <param name="json">The contained qlog JSON text.</param>
    /// <returns>The parsed qlog file model.</returns>
    public static QlogFile DeserializeContainedJson(string json)
    {
        return QlogJsonSerializer.Deserialize(json);
    }

    /// <summary>
    /// Deserializes contained qlog JSON output from a stream.
    /// </summary>
    /// <param name="stream">The contained qlog JSON stream.</param>
    /// <returns>The parsed qlog file model.</returns>
    public static QlogFile DeserializeContainedJson(Stream stream)
    {
        return QlogJsonSerializer.Deserialize(stream);
    }

    /// <summary>
    /// Deserializes sequential qlog JSON Text Sequences output from a string.
    /// </summary>
    /// <param name="text">The sequential qlog text sequence.</param>
    /// <returns>The parsed qlog file model.</returns>
    public static QlogFile DeserializeSequentialJsonTextSequences(string text)
    {
        return QlogSequentialJsonTextSequenceReader.Deserialize(text);
    }

    /// <summary>
    /// Deserializes sequential qlog JSON Text Sequences output from a stream.
    /// </summary>
    /// <param name="stream">The sequential qlog JSON Text Sequences stream.</param>
    /// <returns>The parsed qlog file model.</returns>
    public static QlogFile DeserializeSequentialJsonTextSequences(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        return QlogSequentialJsonTextSequenceReader.Deserialize(DecodeUtf8Text(ReadAllBytes(stream)));
    }

    /// <summary>
    /// Deserializes contained qlog CBOR output from a stream.
    /// </summary>
    /// <param name="stream">The contained qlog CBOR stream.</param>
    /// <returns>The parsed qlog file model.</returns>
    public static QlogFile DeserializeContainedCbor(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        return QlogContainedCborReader.Deserialize(ReadAllBytes(stream));
    }

    private static QlogFile DeserializePayload(byte[] payload)
    {
        int index = SkipLeadingUtf8BomAndWhitespace(payload);
        if (index >= payload.Length)
        {
            throw new InvalidOperationException("Qlog input must not be empty.");
        }

        byte first = payload[index];
        if (first == JsonRecordSeparator)
        {
            return QlogSequentialJsonTextSequenceReader.Deserialize(DecodeUtf8Text(payload));
        }

        if (first == (byte)'{')
        {
            return QlogJsonSerializer.Deserialize(DecodeUtf8Text(payload));
        }

        if ((first & CborMapMajorTypeMask) == CborMapMajorType)
        {
            return QlogContainedCborReader.Deserialize(payload);
        }

        throw new InvalidOperationException("Unsupported qlog input format.");
    }

    private static byte[] ReadAllBytes(Stream stream)
    {
        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static string DecodeUtf8Text(byte[] payload)
    {
        string text = Encoding.UTF8.GetString(payload);
        return text.Length > 0 && text[0] == Utf8BomCharacter
            ? text[1..]
            : text;
    }

    private static int SkipLeadingUtf8BomAndWhitespace(byte[] payload)
    {
        int index = 0;
        if (
            payload.Length >= Utf8BomLength
            && payload[Utf8BomFirstIndex] == Utf8BomFirstByte
            && payload[Utf8BomSecondIndex] == Utf8BomSecondByte
            && payload[Utf8BomThirdIndex] == Utf8BomThirdByte)
        {
            index = Utf8BomLength;
        }

        while (index < payload.Length && IsUtf8WhitespaceByte(payload[index]))
        {
            index++;
        }

        return index;
    }

    private static bool IsUtf8WhitespaceByte(byte value) =>
        value is Utf8Tab or Utf8LineFeed or Utf8CarriageReturn or Utf8Space;
}

namespace Incursa.Qlog.Benchmarks;

internal static class QlogTransportBenchmarkData
{
    public const int RepresentativePayloadLength = 1200;
    private const int ChecksumMultiplier = 33;
    private const int PayloadByteMultiplier = 31;
    private const int PayloadByteOffset = 17;

    public static byte[] CreatePayload(int length)
    {
        byte[] payload = new byte[length];
        for (int index = 0; index < payload.Length; index++)
        {
            payload[index] = unchecked((byte)((index * PayloadByteMultiplier) + PayloadByteOffset));
        }

        return payload;
    }

    public static int MovePayload(byte[] source, byte[] destination, ulong offset)
    {
        source.AsSpan().CopyTo(destination);

        int checksum = 0;
        for (int index = 0; index < destination.Length; index++)
        {
            checksum = unchecked((checksum * ChecksumMultiplier) + destination[index]);
        }

        return checksum ^ unchecked((int)offset);
    }
}

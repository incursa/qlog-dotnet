namespace Incursa.Qlog.Serialization.Cbor;

/// <summary>
/// Provides the repository-selected qlog CBOR artifact values for the contained serializer slice.
/// </summary>
public static class QlogCborKnownValues
{
    /// <summary>
    /// Gets the media type used for contained qlog CBOR artifacts in this repository.
    /// </summary>
    public const string ContainedCborSerializationFormat = "application/cbor";

    /// <summary>
    /// Gets the canonical file extension used for contained qlog CBOR artifacts in this repository.
    /// </summary>
    public const string ContainedCborFileExtension = ".qlog.cbor";
}

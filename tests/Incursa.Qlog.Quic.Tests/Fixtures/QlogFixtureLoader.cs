using Incursa.Qlog;
using Incursa.Qlog.Import;

namespace Incursa.Qlog.Quic.Tests.Fixtures;

internal static class QlogFixtureLoader
{
    internal static QlogFile LoadQlog(params string[] pathSegments)
    {
        using Stream stream = File.OpenRead(GetFixturePath(pathSegments));
        return QlogImportSerializer.Deserialize(stream);
    }

    internal static QlogFile LoadContainedCborQlog(params string[] pathSegments)
    {
        using Stream stream = File.OpenRead(GetFixturePath(pathSegments));
        return QlogImportSerializer.DeserializeContainedCbor(stream);
    }

    internal static string GetFixturePath(params string[] pathSegments)
    {
        ArgumentNullException.ThrowIfNull(pathSegments);

        if (pathSegments.Length == 0)
        {
            throw new ArgumentException("At least one fixture path segment is required.", nameof(pathSegments));
        }

        return Path.Combine(AppContext.BaseDirectory, Path.Combine(pathSegments));
    }
}

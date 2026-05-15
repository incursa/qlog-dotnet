namespace Incursa.Qlog;

internal static class QlogCaptureOutputPathResolver
{
    public static string Resolve(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (Path.IsPathFullyQualified(filePath))
        {
            return filePath;
        }

        string? qlogDir = Environment.GetEnvironmentVariable("QLOGDIR");
        if (!string.IsNullOrWhiteSpace(qlogDir))
        {
            return Path.Combine(qlogDir, filePath);
        }

        string? qlogFile = Environment.GetEnvironmentVariable("QLOGFILE");
        if (!string.IsNullOrWhiteSpace(qlogFile))
        {
            return qlogFile;
        }

        return filePath;
    }
}

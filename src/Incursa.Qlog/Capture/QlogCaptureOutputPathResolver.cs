// Copyright (c) 2026 Incursa LLC.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

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

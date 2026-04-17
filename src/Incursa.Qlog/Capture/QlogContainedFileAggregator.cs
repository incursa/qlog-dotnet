namespace Incursa.Qlog;

internal static class QlogContainedFileAggregator
{
    public static QlogFile AppendCompletedSession(QlogFile? aggregatedFile, QlogFile completedSessionFile)
    {
        ArgumentNullException.ThrowIfNull(completedSessionFile);

        if (aggregatedFile is null)
        {
            return completedSessionFile;
        }

        foreach (QlogTraceComponent traceComponent in completedSessionFile.Traces)
        {
            aggregatedFile.Traces.Add(traceComponent);
        }

        // Keep the shared envelope deterministic: first completed session wins conflicts,
        // while later sessions only backfill metadata that is still absent.
        if (string.IsNullOrWhiteSpace(aggregatedFile.Title))
        {
            aggregatedFile.Title = completedSessionFile.Title;
        }

        if (string.IsNullOrWhiteSpace(aggregatedFile.Description))
        {
            aggregatedFile.Description = completedSessionFile.Description;
        }

        foreach (KeyValuePair<string, QlogValue> entry in completedSessionFile.ExtensionData)
        {
            if (!aggregatedFile.ExtensionData.ContainsKey(entry.Key))
            {
                aggregatedFile.ExtensionData[entry.Key] = entry.Value;
            }
        }

        return aggregatedFile;
    }
}

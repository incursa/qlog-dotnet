// Copyright (c) 2026 Incursa LLC.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Incursa.Qlog;

internal sealed class QlogCaptureSession
{
    private long nextSequence;
    private bool completed;
    private bool captureStarted;

    public QlogCaptureSession(
        string sessionId,
        QlogTrace traceHeader,
        string? fileTitle = null,
        string? fileDescription = null,
        IEnumerable<KeyValuePair<string, QlogValue>>? fileExtensionData = null)
    {
        Snapshot = QlogCaptureSessionSnapshot.Create(
            sessionId,
            traceHeader,
            fileTitle,
            fileDescription,
            fileExtensionData);
    }

    public string SessionId => Snapshot.SessionId;

    internal object Gate { get; } = new();

    internal QlogCaptureSessionSnapshot Snapshot { get; }

    internal void StartCapture()
    {
        ThrowIfCompleted();
        captureStarted = true;
    }

    internal long NextSequence()
    {
        nextSequence++;
        return nextSequence;
    }

    internal void ThrowIfCompleted()
    {
        if (completed)
        {
            throw new InvalidOperationException($"Capture session '{SessionId}' has already completed.");
        }
    }

    internal void ThrowIfCaptureNotStarted()
    {
        if (!captureStarted)
        {
            throw new InvalidOperationException(
                $"Capture session '{SessionId}' has not been explicitly started. Call StartCapture() before writing qlog data.");
        }
    }

    internal void MarkCompleted()
    {
        ThrowIfCompleted();
        completed = true;
    }
}

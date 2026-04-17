using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Incursa.Qlog;
using Incursa.Qlog.Serialization.Json;
using Xunit;

namespace Incursa.Qlog.Tests;

public sealed class REQ_QLOG_CAPTURE_0001
{
    [Fact]
    [Trait("Requirement", "REQ-QLOG-CAPTURE-S1-0006")]
    [Trait("Requirement", "REQ-QLOG-CAPTURE-S1-0007")]
    [Trait("CoverageType", "Negative")]
    public void PublicApiSurface_DoesNotExposeCaptureSessionOrSinkTypes()
    {
        string[] exportedTypeNames = typeof(QlogFile)
            .Assembly
            .GetExportedTypes()
            .Select(type => type.FullName ?? type.Name)
            .ToArray();

        Assert.Contains(typeof(QlogFile).FullName, exportedTypeNames);
        Assert.Contains(typeof(QlogTrace).FullName, exportedTypeNames);
        Assert.Contains(typeof(QlogEvent).FullName, exportedTypeNames);
        Assert.Contains(typeof(QlogJsonSerializer).FullName, exportedTypeNames);
        Assert.Contains(typeof(QlogJsonTextSequenceSerializer).FullName, exportedTypeNames);

        Assert.DoesNotContain(exportedTypeNames, name => name.Contains("Capture", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(exportedTypeNames, name => name.Contains("Session", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(exportedTypeNames, name => name.Contains("Sink", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(exportedTypeNames, name => name.Contains("Dispatcher", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-CAPTURE-S1-0001")]
    [Trait("Requirement", "REQ-QLOG-CAPTURE-S1-0002")]
    [Trait("Requirement", "REQ-QLOG-CAPTURE-S1-0003")]
    [Trait("CoverageType", "Positive")]
    public async Task Capture_DispatchesFrozenSnapshotsToSessionAndProcessObservers()
    {
        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateSession("session-and-process");
        RecordingObserver sessionObserver = new();
        RecordingObserver processObserver = new();

        await using QlogCaptureSubscription sessionSubscription = coordinator.RegisterSessionObserver(session, sessionObserver);
        await using QlogCaptureSubscription processSubscription = coordinator.RegisterProcessObserver(processObserver);

        QlogEvent qlogEvent = CreateEvent("example:first", 11);
        coordinator.Capture(session, qlogEvent);

        qlogEvent.Name = "example:mutated";
        qlogEvent.Data["packet_size"] = QlogValue.FromNumber(99);
        qlogEvent.ExtensionData["captured"] = QlogValue.FromString("after");

        await coordinator.CompleteSessionAsync(session);
        await coordinator.FlushAsync();

        Assert.Single(sessionObserver.Events);
        Assert.Single(processObserver.Events);

        Assert.Equal("example:first", sessionObserver.Events[0].Name);
        Assert.Equal(QlogValue.FromNumber(11), sessionObserver.Events[0].Data["packet_size"]);
        Assert.Equal(QlogValue.FromString("before"), sessionObserver.Events[0].ExtensionData["captured"]);

        Assert.Equal("example:first", processObserver.Events[0].Name);
        Assert.Equal(QlogValue.FromNumber(11), processObserver.Events[0].Data["packet_size"]);
        Assert.Equal("session-and-process", sessionObserver.CompletedSessions.Single());
        Assert.Equal("session-and-process", processObserver.CompletedSessions.Single());
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-CAPTURE-S1-0004")]
    [Trait("CoverageType", "Positive")]
    public async Task Capture_PreservesPerSessionOrderAcrossAsyncDispatch()
    {
        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateSession("ordered-session");
        RecordingObserver observer = new();

        await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(session, observer);

        coordinator.Capture(session, CreateEvent("example:first", 1));
        coordinator.Capture(session, CreateEvent("example:second", 2));
        coordinator.Capture(session, CreateEvent("example:third", 3));

        await coordinator.CompleteSessionAsync(session);

        Assert.Equal(
            new[] { "example:first", "example:second", "example:third" },
            observer.Events.Select(capturedEvent => capturedEvent.Name).ToArray());
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-CAPTURE-S1-0004")]
    [Trait("CoverageType", "Edge")]
    public async Task Capture_DoesNotBlockTheCallerOnObserverWork()
    {
        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateSession("non-blocking-session");
        BlockingObserver observer = new();

        await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(session, observer);

        Stopwatch stopwatch = Stopwatch.StartNew();
        coordinator.Capture(session, CreateEvent("example:first", 1));
        await observer.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        coordinator.Capture(session, CreateEvent("example:second", 2));
        stopwatch.Stop();

        Assert.True(stopwatch.Elapsed < TimeSpan.FromMilliseconds(250));

        observer.Release();
        await coordinator.CompleteSessionAsync(session);
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-CAPTURE-S1-0008")]
    [Trait("Requirement", "REQ-QLOG-CAPTURE-S1-0009")]
    [Trait("CoverageType", "Positive")]
    public void Capture_DefaultDispatchOptions_UseBoundedBackpressureAndScaleCapacity()
    {
        Assert.Equal(QlogCaptureBackpressureMode.BoundedDropNewest, QlogCaptureDispatchOptions.Default.BackpressureMode);
        Assert.Equal(1024, QlogCaptureDispatchOptions.ComputeRecommendedCapacity(128L * 1024 * 1024));
        Assert.Equal(4096, QlogCaptureDispatchOptions.ComputeRecommendedCapacity(8L * 1024 * 1024 * 1024));
        Assert.Equal(16384, QlogCaptureDispatchOptions.ComputeRecommendedCapacity(64L * 1024 * 1024 * 1024));
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-CAPTURE-S1-0008")]
    [Trait("CoverageType", "Edge")]
    public async Task Capture_DropsNewestEventsWhenTheBoundedObserverQueueIsFull()
    {
        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateSession("bounded-drop-session");
        BlockingObserver observer = new();
        QlogCaptureDispatchOptions options = new()
        {
            BackpressureMode = QlogCaptureBackpressureMode.BoundedDropNewest,
            BoundedCapacity = 1,
        };

        await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(session, observer, options);

        coordinator.Capture(session, CreateEvent("example:first", 1));
        await observer.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        coordinator.Capture(session, CreateEvent("example:second", 2));
        coordinator.Capture(session, CreateEvent("example:third", 3));

        observer.Release();
        await coordinator.CompleteSessionAsync(session);

        Assert.Equal(
            new[] { "example:first", "example:second" },
            observer.Events.Select(capturedEvent => capturedEvent.Name).ToArray());
    }

    [Fact]
    [Trait("Requirement", "REQ-QLOG-CAPTURE-S1-0010")]
    [Trait("CoverageType", "Positive")]
    public async Task Capture_AllowsExplicitUnboundedObserverQueues()
    {
        await using QlogCaptureCoordinator coordinator = new();
        QlogCaptureSession session = CreateSession("unbounded-session");
        BlockingObserver observer = new();
        QlogCaptureDispatchOptions options = new()
        {
            BackpressureMode = QlogCaptureBackpressureMode.Unbounded,
        };

        await using QlogCaptureSubscription subscription = coordinator.RegisterSessionObserver(session, observer, options);

        coordinator.Capture(session, CreateEvent("example:first", 1));
        await observer.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        coordinator.Capture(session, CreateEvent("example:second", 2));
        coordinator.Capture(session, CreateEvent("example:third", 3));

        observer.Release();
        await coordinator.CompleteSessionAsync(session);

        Assert.Equal(
            new[] { "example:first", "example:second", "example:third" },
            observer.Events.Select(capturedEvent => capturedEvent.Name).ToArray());
    }

    private static QlogCaptureSession CreateSession(string sessionId)
    {
        QlogTrace trace = new()
        {
            Title = "capture-trace",
            CommonFields = new QlogCommonFields
            {
                GroupId = "capture-group",
                TimeFormat = QlogKnownValues.RelativeToEpochTimeFormat,
            },
            VantagePoint = new QlogVantagePoint
            {
                Type = QlogKnownValues.ServerVantagePoint,
            },
        };
        trace.EventSchemas.Add(new Uri("urn:ietf:params:qlog:events:example"));

        return new QlogCaptureSession(sessionId, trace, fileTitle: "capture-file");
    }

    private static QlogEvent CreateEvent(string name, int packetSize)
    {
        QlogEvent qlogEvent = new()
        {
            Time = packetSize,
            Name = name,
            GroupId = "capture-group",
        };
        qlogEvent.Data["packet_size"] = QlogValue.FromNumber(packetSize);
        qlogEvent.ExtensionData["captured"] = QlogValue.FromString("before");
        return qlogEvent;
    }

    private sealed class RecordingObserver : IQlogCaptureObserver
    {
        public List<QlogCapturedEvent> Events { get; } = new();

        public List<string> CompletedSessions { get; } = new();

        public ValueTask OnCapturedAsync(QlogCapturedEvent capturedEvent, CancellationToken cancellationToken)
        {
            Events.Add(capturedEvent);
            return ValueTask.CompletedTask;
        }

        public ValueTask OnSessionCompletedAsync(QlogCaptureSessionSnapshot sessionSnapshot, CancellationToken cancellationToken)
        {
            CompletedSessions.Add(sessionSnapshot.SessionId);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class BlockingObserver : IQlogCaptureObserver
    {
        private readonly TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public List<QlogCapturedEvent> Events { get; } = new();

        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask OnCapturedAsync(QlogCapturedEvent capturedEvent, CancellationToken cancellationToken)
        {
            Events.Add(capturedEvent);
            Started.TrySetResult();
            await release.Task.WaitAsync(cancellationToken);
        }

        public ValueTask OnSessionCompletedAsync(QlogCaptureSessionSnapshot sessionSnapshot, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        public void Release()
        {
            release.TrySetResult();
        }
    }
}

namespace Incursa.Qlog;

internal enum QlogCaptureBackpressureMode
{
    BoundedDropNewest,
    Unbounded,
}

internal sealed class QlogCaptureDispatchOptions
{
    private const int MinimumRecommendedBoundedCapacity = 1024;
    private const int MaximumRecommendedBoundedCapacity = 16384;
    private const int EstimatedFrozenEventFootprintBytes = 4 * 1024;
    private const int DefaultQueueBudgetDivisor = 512;

    public static QlogCaptureDispatchOptions Default { get; } = new();

    public QlogCaptureBackpressureMode BackpressureMode { get; init; } = QlogCaptureBackpressureMode.BoundedDropNewest;

    public int? BoundedCapacity { get; init; }

    internal int ResolveBoundedCapacity()
    {
        if (BackpressureMode == QlogCaptureBackpressureMode.Unbounded)
        {
            throw new InvalidOperationException("A bounded capacity cannot be resolved for an unbounded capture queue.");
        }

        if (BoundedCapacity is int boundedCapacity)
        {
            if (boundedCapacity <= 0)
            {
                throw new InvalidOperationException("The bounded capture queue capacity must be greater than zero.");
            }

            return boundedCapacity;
        }

        return ComputeRecommendedCapacity();
    }

    internal static int ComputeRecommendedCapacity()
    {
        long totalAvailableMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        return ComputeRecommendedCapacity(totalAvailableMemoryBytes);
    }

    internal static int ComputeRecommendedCapacity(long totalAvailableMemoryBytes)
    {
        if (totalAvailableMemoryBytes <= 0)
        {
            return MinimumRecommendedBoundedCapacity;
        }

        long recommendedCapacity = totalAvailableMemoryBytes / (EstimatedFrozenEventFootprintBytes * (long)DefaultQueueBudgetDivisor);
        if (recommendedCapacity < MinimumRecommendedBoundedCapacity)
        {
            return MinimumRecommendedBoundedCapacity;
        }

        if (recommendedCapacity > MaximumRecommendedBoundedCapacity)
        {
            return MaximumRecommendedBoundedCapacity;
        }

        return (int)recommendedCapacity;
    }
}

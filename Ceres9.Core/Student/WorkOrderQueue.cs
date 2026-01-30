using Ceres9.Core.Domain;

namespace Ceres9.Core.Student;

/// <summary>
/// Stage 4: Bounded producer-consumer queue for work orders.
/// </summary>
/// <remarks>
/// Requirements:
/// - Bounded capacity: Enqueue should block when full (until space is available or cancellation).
/// - Consumers block when empty (until an item appears, completion is signaled, timeout, or cancellation).
/// - Each enqueued order is returned exactly once across all consumers.
///
/// Hint: BlockingCollection&lt;T&gt; (wrapping ConcurrentQueue&lt;T&gt;) matches this stage well.
/// </remarks>
public sealed class WorkOrderQueue(int capacity) : IDisposable
{
    public void Enqueue(WorkOrder order, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public bool TryDequeue(out WorkOrder? order, TimeSpan timeout, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public void Complete()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}


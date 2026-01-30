namespace Ceres9.Core.Student.Boss;

/// <summary>
/// Boss fight: Implement a bounded blocking queue using Monitor.Wait/Pulse(All).
/// </summary>
/// <remarks>
/// Requirements:
/// - Bounded by <c>capacity</c>.
/// - Enqueue blocks when full.
/// - Dequeue blocks when empty.
/// - Must be safe under concurrency and support cancellation.
///
/// Hint: Condition variables require a lock + while-loop rechecking the condition.
/// </remarks>
public sealed class MonitorBoundedQueue<T>(int capacity)
{
    public void Enqueue(T item, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public T Dequeue(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}


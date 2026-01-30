namespace Ceres9.Core.Student;

/// <summary>
/// Stage 2: Thread-safe numeric counter.
/// </summary>
/// <remarks>
/// Requirements:
/// - No lost updates under concurrency.
/// - Reads must observe a consistent value.
///
/// Hint: Interlocked operations are the intended tool.
/// </remarks>
public sealed class AtomicCounter(long initialValue = 0)
{
    private long counter = initialValue;
    public long Value
    {
        get { return  Volatile.Read(ref counter); }
    }

    public long Increment()
    {
       return Interlocked.Increment(ref counter);
        
    }

    public long Add(long delta)
    {
        return Interlocked.Add(ref counter, delta);
    }
}


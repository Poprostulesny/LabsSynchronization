using Ceres9.Core.Domain;

namespace Ceres9.Core.Student.Boss;

/// <summary>
/// Boss fight: Ultra-short critical sections with SpinLock.
/// </summary>
/// <remarks>
/// Requirements:
/// - Thread-safe under concurrency.
/// - Keep the critical section minimal (no allocations / I/O while holding the lock).
/// </remarks>
public sealed class SpinLockedStats
{
    public void AddSample(int value)
    {
        throw new NotImplementedException();
    }

    public StatsSnapshot Snapshot()
    {
        throw new NotImplementedException();
    }
}


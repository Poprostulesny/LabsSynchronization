namespace Ceres9.Core.Student;

/// <summary>
/// Stage 7: Synchronize workers in phases ("shifts").
/// </summary>
/// <remarks>
/// Requirements:
/// - There are <paramref name="participants"/> participants.
/// - Each participant calls <see cref="SignalAndWait"/> once per shift.
/// - <paramref name="afterShift"/> is invoked exactly once per shift, after all participants arrive.
///
/// Hint: Barrier is designed for multi-phase synchronization.
/// </remarks>
public sealed class ShiftCoordinator(int participants, Action<int> afterShift) : IDisposable
{
    public int CurrentShift
    {
        get { throw new NotImplementedException(); }
    }

    public void SignalAndWait(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}


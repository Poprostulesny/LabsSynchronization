namespace Ceres9.Core.Student;

/// <summary>
/// Stage 5: Pause/resume gate for workers.
/// </summary>
/// <remarks>
/// Requirements:
/// - When paused, workers calling <see cref="WaitIfPaused"/> must block.
/// - When resumed, all blocked workers proceed.
/// - Must support cancellation.
///
/// Hint: ManualResetEventSlim models an "open/closed gate".
/// </remarks>
public sealed class PauseGate : IDisposable
{
    public bool IsPaused
    {
        get { throw new NotImplementedException(); }
    }

    public void Pause()
    {
        throw new NotImplementedException();
    }

    public void Resume()
    {
        throw new NotImplementedException();
    }

    public void WaitIfPaused(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}


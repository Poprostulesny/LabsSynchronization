namespace Ceres9.Core.Student;

/// <summary>
/// Stage 1: Safe stop flag for hot loops.
/// </summary>
/// <remarks>
/// Goal: After <see cref="RequestStop"/> returns, other threads must reliably observe
/// <see cref="IsStopRequested"/> becoming true without using locks or sleeps.
///
/// Hint: Look at memory visibility topics (volatile / Interlocked / memory barriers).
/// </remarks>
public sealed class StopSignal
{
    private bool stopflag = false;
    public void RequestStop()
    {
        Volatile.Write(ref stopflag, true);
    }

    public void Reset()
    {
        Volatile.Write(ref stopflag, false);
    }

    public bool IsStopRequested
    {
        get { return Volatile.Read(ref stopflag); }
    }
}


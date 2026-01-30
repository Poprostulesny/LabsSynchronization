namespace Ceres9.Core.Student;

/// <summary>
/// Stage 5: "One ship through" signaling mechanism.
/// </summary>
/// <remarks>
/// Requirements:
/// - Each call to <see cref="AllowNextShip"/> releases exactly one waiter (or stores the signal).
/// - <see cref="WaitForClearance"/> blocks until released or canceled.
///
/// Hint: AutoResetEvent is designed for exactly-one release semantics.
/// </remarks>
public sealed class CustomsTurnstile : IDisposable
{
    public void AllowNextShip()
    {
        throw new NotImplementedException();
    }

    public void WaitForClearance(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}


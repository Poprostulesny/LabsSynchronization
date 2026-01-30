namespace Ceres9.Core.Student;

/// <summary>
/// Stage 7: Wait for all workers to exit during shutdown.
/// </summary>
/// <remarks>
/// Requirements:
/// - Constructed with the number of workers.
/// - Each worker calls <see cref="WorkerDone"/> exactly once on exit.
/// - Controller calls <see cref="WaitAll"/> and expects it to return true once all workers are done.
///
/// Hint: CountdownEvent matches this pattern naturally.
/// </remarks>
public sealed class ShutdownLatch(int workerCount) : IDisposable
{
    public void WorkerDone()
    {
        throw new NotImplementedException();
    }

    public bool WaitAll(TimeSpan timeout, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}


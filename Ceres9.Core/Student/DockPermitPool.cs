namespace Ceres9.Core.Student;

/// <summary>
/// Stage 3: Limits docking concurrency and assigns a concrete dock index.
/// </summary>
/// <remarks>
/// Requirements:
/// - At most <c>dockCount</c> permits may be active at once.
/// - Each active permit must have a unique <see cref="DockPermit.DockIndex"/> in [0..dockCount-1].
/// - Releasing a permit must unblock waiters even if the worker fails (use try/finally patterns).
///
/// Hint: SemaphoreSlim is a natural fit for throttling; a concurrent collection can track free indices.
/// </remarks>
public sealed class DockPermitPool(int dockCount) : IAsyncDisposable
{
    public Task<DockPermit> AcquireAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}

public sealed class DockPermit : IAsyncDisposable
{
    public int DockIndex
    {
        get { throw new NotImplementedException(); }
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}


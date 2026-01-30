namespace Ceres9.Core.Student.Boss;

/// <summary>
/// Boss fight: Prevent multiple app instances using a named Mutex (cross-process).
/// </summary>
public sealed class SingleInstanceGuard : IDisposable
{
    public static bool TryAcquire(string name, out SingleInstanceGuard? guard)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}


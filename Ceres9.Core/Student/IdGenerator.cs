namespace Ceres9.Core.Student;

/// <summary>
/// Stage 2: Thread-safe unique ID generator.
/// </summary>
/// <remarks>
/// Requirements:
/// - <see cref="NextId"/> must never return duplicate values, even under heavy concurrency.
/// - IDs must be strictly increasing (start at <paramref name="startId"/> + 1 by default).
///
/// Hint: Plain ++ is not atomic.
/// </remarks>
public sealed class IdGenerator(int startId = 0)
{   
    private int  counter = 0;
    private Lock locker = new Lock();
    public int NextId()
    {
        lock (locker)
        {
            counter++;
            return counter;
        }
    }
}


namespace Ceres9.Core.Student;

/// <summary>
/// Stage 6: Inventory that is read frequently and written occasionally.
/// </summary>
/// <remarks>
/// Requirements:
/// - Multiple readers may read concurrently.
/// - Writes are exclusive.
/// - <see cref="TryTake"/> must be atomic and must never allow negative quantity.
/// - <see cref="Snapshot"/> must be consistent (no half-applied updates).
///
/// Hint: ReaderWriterLockSlim targets "many readers, few writers".
/// </remarks>
public sealed class WarehouseInventory : IDisposable
{
    public int GetQuantity(string item)
    {
        throw new NotImplementedException();
    }

    public void Add(string item, int amount)
    {
        throw new NotImplementedException();
    }

    public bool TryTake(string item, int amount)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyDictionary<string, int> Snapshot()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}


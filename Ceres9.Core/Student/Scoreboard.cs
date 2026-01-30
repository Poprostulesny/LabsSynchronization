using Ceres9.Core.Domain;

namespace Ceres9.Core.Student;

/// <summary>
/// Stage 4: Thread-safe scoreboard updated by many workers.
/// </summary>
/// <remarks>
/// Requirements:
/// - <see cref="RecordDelivery"/> is called concurrently.
/// - Stats must never "lose" deliveries (all increments must be accounted for).
/// - <see cref="Top"/> should return the best companies by Credits (ties can be any order).
///
/// Hint: ConcurrentDictionary with AddOrUpdate is worth investigating.
/// </remarks>
public sealed class Scoreboard
{
    public void RecordDelivery(string company, int crates, int credits)
    {
        throw new NotImplementedException();
    }

    public CompanyStats Get(string company)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<CompanyStats> Top(int count)
    {
        throw new NotImplementedException();
    }
}


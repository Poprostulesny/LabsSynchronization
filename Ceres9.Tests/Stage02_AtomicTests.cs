using System.Collections.Concurrent;
using Ceres9.Core.Student;

namespace Ceres9.Tests;

[TestClass]
public sealed class Stage02_AtomicTests
{
    [TestMethod]
    public void IdGenerator_NextId_MustBeUnique()
    {
        var gen = new IdGenerator();
        const int total = 50_000;
        var bag = new ConcurrentBag<int>();

        Parallel.For(0, total, _ => bag.Add(gen.NextId()));

        var unique = bag.Distinct().Count();
        Assert.AreEqual(total, unique, "IDs must be unique under concurrency.");

        var min = bag.Min();
        var max = bag.Max();
        Assert.AreEqual(total, max - min + 1, "IDs should be contiguous increasing starting at > 0.");
    }

    [TestMethod]
    public void AtomicCounter_MustNotLoseUpdates()
    {
        const int tasks = 32;
        const int perTask = 20_000;
        var counter = new AtomicCounter();

        Parallel.For(0, tasks, _ =>
        {
            for (int i = 0; i < perTask; i++)
            {
                counter.Increment();
            }
            counter.Add(1); // one extra to mix API usage
        });

        long expected = tasks * perTask + tasks * 1;
        Assert.AreEqual(expected, counter.Value, "Counter lost updates under concurrency.");
    }
}

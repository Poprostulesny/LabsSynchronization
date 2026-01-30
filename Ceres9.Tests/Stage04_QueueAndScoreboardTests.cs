using System.Collections.Concurrent;
using Ceres9.Core.Domain;
using Ceres9.Core.Student;

namespace Ceres9.Tests;

[TestClass]
public sealed class Stage04_QueueAndScoreboardTests
{
    [TestMethod]
    public void WorkOrderQueue_MustNotDropOrDuplicateOrders()
    {
        const int capacity = 10;
        const int producers = 4;
        const int consumers = 6;
        const int ordersPerProducer = 200;
        var queue = new WorkOrderQueue(capacity);
        var seen = new ConcurrentDictionary<long, byte>();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // producers
        var producerTasks = Enumerable.Range(0, producers).Select(p => Task.Run(() =>
        {
            for (int i = 0; i < ordersPerProducer; i++)
            {
                long id = p * 1_000_000 + i;
                queue.Enqueue(new WorkOrder(id, i, "ACME", "Ore", 1, 1, DateTimeOffset.UtcNow), cts.Token);
            }
        }, cts.Token)).ToArray();

        // consumers
        int consumed = 0;
        var consumerTasks = Enumerable.Range(0, consumers).Select(_ => Task.Run(() =>
        {
            while (true)
            {
                if (!queue.TryDequeue(out var order, TimeSpan.FromMilliseconds(200), cts.Token))
                {
                    return; // completed and drained
                }

                Assert.IsNotNull(order);
                bool added = seen.TryAdd(order!.OrderId, 1);
                Assert.IsTrue(added, $"Duplicate order id processed: {order.OrderId}");
                Interlocked.Increment(ref consumed);
            }
        }, cts.Token)).ToArray();

        Task.WaitAll(producerTasks);
        queue.Complete();
        Task.WaitAll(consumerTasks);

        int expected = producers * ordersPerProducer;
        Assert.AreEqual(expected, consumed, "Not all orders were processed.");
        Assert.AreEqual(expected, seen.Count, "Lost or duplicate orders detected.");
    }

    [TestMethod]
    public void Scoreboard_MustAggregateCorrectly()
    {
        var scoreboard = new Scoreboard();
        var companies = new[] { "ACME", "Weyland", "Tyrell", "Umbrella" };
        var totals = new ConcurrentDictionary<string, (int ships, int crates, int credits)>();
        foreach (var c in companies)
        {
            totals[c] = (0, 0, 0);
        }

        const int tasks = 16;
        const int opsPerTask = 1000;
        var threadLocalRandom = new ThreadLocal<Random>(() => new Random());

        Parallel.For(0, tasks, _ =>
        {
            var local = threadLocalRandom.Value!;
            for (int i = 0; i < opsPerTask; i++)
            {
                var company = companies[local.Next(companies.Length)];
                int crates = local.Next(1, 5);
                int credits = crates * 10;
                scoreboard.RecordDelivery(company, crates, credits);

                totals.AddOrUpdate(company,
                    key => (1, crates, credits),
                    (key, old) => (old.ships + 1, old.crates + crates, old.credits + credits));
            }
        });

        foreach (var company in companies)
        {
            var expected = totals[company];
            var actual = scoreboard.Get(company);
            Assert.AreEqual(expected.ships, actual.Ships, $"{company}: ships mismatch");
            Assert.AreEqual(expected.crates, actual.Crates, $"{company}: crates mismatch");
            Assert.AreEqual(expected.credits, actual.Credits, $"{company}: credits mismatch");
        }

        var top = scoreboard.Top(2);
        Assert.IsTrue(top.Count <= 2);
        // top should be sorted by credits descending
        for (int i = 1; i < top.Count; i++)
        {
            Assert.IsTrue(top[i - 1].Credits >= top[i].Credits, "Top is not sorted by credits.");
        }
    }
}

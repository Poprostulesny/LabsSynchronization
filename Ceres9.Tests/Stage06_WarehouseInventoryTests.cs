using Ceres9.Core.Student;

namespace Ceres9.Tests;

[TestClass]
public sealed class Stage06_WarehouseInventoryTests
{
    [TestMethod]
    public void Inventory_TryTake_MustNeverGoNegative()
    {
        using var inv = new WarehouseInventory();
        const string item = "Helium-3";
        const int writers = 8;
        const int readers = 16;
        const int ops = 10_000;
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var writerTasks = Enumerable.Range(0, writers).Select(_ => Task.Run(() =>
        {
            var rnd = new Random();
            for (int i = 0; i < ops; i++)
            {
                if (rnd.NextDouble() < 0.6)
                {
                    inv.Add(item, 3);
                }
                else
                {
                    inv.TryTake(item, 2);
                }
            }
        }, cts.Token)).ToArray();

        var readerTasks = Enumerable.Range(0, readers).Select(i => Task.Run(() =>
        {
            for (int j = 0; j < ops; j++)
            {
                _ = inv.GetQuantity(item);
                _ = inv.Snapshot();
            }
        }, cts.Token)).ToArray();

        Task.WaitAll(writerTasks.Concat(readerTasks).ToArray());

        // final invariant
        var finalQty = inv.GetQuantity(item);
        Assert.IsTrue(finalQty >= 0, "Inventory went negative.");

        var snapshot = inv.Snapshot();
        if (snapshot.TryGetValue(item, out var snapQty))
        {
            Assert.IsTrue(snapQty >= 0, "Snapshot shows negative inventory.");
        }
    }
}

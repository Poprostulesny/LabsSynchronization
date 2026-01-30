using System.Collections.Concurrent;
using Ceres9.Core.Student;

namespace Ceres9.Tests;

[TestClass]
public sealed class Stage03_DockPermitPoolTests
{
    [TestMethod]
    public async Task AcquireAsync_MustRespectDockCount_AndAssignUniqueIndices()
    {
        const int dockCount = 3;
        const int totalAttempts = 60;
        var pool = new DockPermitPool(dockCount);
        var active = new ConcurrentDictionary<int, byte>();
        int maxActive = 0;

        var tasks = Enumerable.Range(0, totalAttempts).Select(async _ =>
        {
            await Task.Yield();
            await using var permit = await pool.AcquireAsync(CancellationToken.None);

            // track active indices
            bool added = active.TryAdd(permit.DockIndex, 1);
            Assert.IsTrue(added, $"Duplicate dock index active: {permit.DockIndex}");

            int currentActive = active.Count;
            int snapshot;
            do
            {
                snapshot = maxActive;
                if (currentActive <= snapshot) break;
            } while (Interlocked.CompareExchange(ref maxActive, currentActive, snapshot) != snapshot);

            // simulate work
            await Task.Delay(10);

            active.TryRemove(permit.DockIndex, out byte _);
        }).ToArray();

        await Task.WhenAll(tasks);

        Assert.IsTrue(maxActive <= dockCount, $"Active permits exceeded dockCount: {maxActive} > {dockCount}");
    }

    [TestMethod]
    public async Task AcquireAsync_Respects_Cancellation()
    {
        var pool = new DockPermitPool(1);
        await using var first = await pool.AcquireAsync(CancellationToken.None);

        using var cts = new CancellationTokenSource(50);
        bool cancelled = false;
        try
        {
            await pool.AcquireAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
        }

        Assert.IsTrue(cancelled, "AcquireAsync should observe cancellation.");
    }
}

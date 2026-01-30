using Ceres9.Core.Student;

namespace Ceres9.Tests;

[TestClass]
public sealed class Stage07_ShiftAndShutdownTests
{
    [TestMethod]
    public void ShiftCoordinator_AfterShift_MustRunExactlyOncePerShift()
    {
        const int participants = 4;
        const int shifts = 5;
        int callbacks = 0;

        using var coordinator = new ShiftCoordinator(participants, shiftNum =>
        {
            Interlocked.Increment(ref callbacks);
        });

        var tasks = Enumerable.Range(0, participants).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < shifts; i++)
            {
                coordinator.SignalAndWait(CancellationToken.None);
            }
        })).ToArray();

        Task.WaitAll(tasks);

        Assert.AreEqual(shifts, callbacks, "afterShift should run once per shift.");
        Assert.AreEqual(shifts, coordinator.CurrentShift, "CurrentShift should match completed shifts.");
    }

    [TestMethod]
    public void ShutdownLatch_WaitsForAllWorkers()
    {
        const int workers = 4;
        using var latch = new ShutdownLatch(workers);

        var tasks = Enumerable.Range(0, workers).Select(i => Task.Run(async () =>
        {
            await Task.Delay(50 + i * 10);
            latch.WorkerDone();
        })).ToArray();

        bool finished = latch.WaitAll(TimeSpan.FromSeconds(2), CancellationToken.None);
        Assert.IsTrue(finished, "Latch did not observe all workers in time.");

        Task.WaitAll(tasks);
    }
}

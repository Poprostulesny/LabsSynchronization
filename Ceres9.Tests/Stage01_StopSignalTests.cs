using System.Diagnostics;
using Ceres9.Core.Student;

namespace Ceres9.Tests;

[TestClass]
public sealed class Stage01_StopSignalTests
{
    [TestMethod]
    public void RequestStop_MustBecomeVisible_ToWorkersQuickly()
    {
        const int workers = 32;
        const int iterations = 5; // repeat to catch flaky visibility

        for (int run = 0; run < iterations; run++)
        {
            var signal = new StopSignal();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            var tasks = Enumerable.Range(0, workers).Select(_ => Task.Run(() =>
            {
                // tight loop, no sleeps
                while (!signal.IsStopRequested)
                {
                    // spin and occasionally do a volatile write pattern
                }
            }, cts.Token)).ToArray();

            // give workers time to start
            Thread.Sleep(10);

            var sw = Stopwatch.StartNew();
            signal.RequestStop();

            bool completed = Task.WaitAll(tasks, TimeSpan.FromMilliseconds(500));
            Assert.IsTrue(completed, $"Workers did not observe stop within 500 ms (run {run}).");
            sw.Stop();
            Assert.IsTrue(sw.ElapsedMilliseconds < 500, $"Stop propagation too slow: {sw.ElapsedMilliseconds} ms (run {run}).");
        }
    }

    [TestMethod]
    public void Reset_Allows_Reusing_StopSignal()
    {
        var signal = new StopSignal();
        signal.RequestStop();
        Assert.IsTrue(signal.IsStopRequested);
        signal.Reset();
        Assert.IsFalse(signal.IsStopRequested);
    }
}

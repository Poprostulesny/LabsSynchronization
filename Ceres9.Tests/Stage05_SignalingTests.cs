using System.Collections.Concurrent;
using Ceres9.Core.Student;

namespace Ceres9.Tests;

[TestClass]
public sealed class Stage05_SignalingTests
{
    [TestMethod]
    public void PauseGate_MustBlockUntilResumed()
    {
        using var gate = new PauseGate();
        gate.Pause();

        int progressed = 0;
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        var worker = Task.Run(() =>
        {
            gate.WaitIfPaused(cts.Token);
            Interlocked.Increment(ref progressed);
        }, cts.Token);

        // give time to ensure it's blocked
        Thread.Sleep(50);
        Assert.AreEqual(0, progressed, "Worker should be blocked while paused.");

        gate.Resume();

        Assert.IsTrue(worker.Wait(TimeSpan.FromMilliseconds(500)), "Worker did not resume after gate opened.");
        Assert.AreEqual(1, progressed, "Worker did not proceed after resume.");
    }

    [TestMethod]
    public void CustomsTurnstile_MustReleaseExactlyOnePerSignal()
    {
        using var turnstile = new CustomsTurnstile();
        const int waiters = 5;
        var passed = new ConcurrentBag<int>();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        var tasks = Enumerable.Range(0, waiters).Select(i => Task.Run(() =>
        {
            turnstile.WaitForClearance(cts.Token);
            passed.Add(i);
        }, cts.Token)).ToArray();

        // allow only 2 ships
        turnstile.AllowNextShip();
        turnstile.AllowNextShip();

        // small wait
        Task.Delay(200).Wait();
        Assert.AreEqual(2, passed.Count, "Exactly two ships should have passed after two signals.");

        // allow remaining
        for (int i = 0; i < waiters - 2; i++) turnstile.AllowNextShip();

        Assert.IsTrue(Task.WaitAll(tasks, 500), "All ships should pass after enough signals.");
        Assert.AreEqual(waiters, passed.Count, "All ships must eventually pass.");
    }
}

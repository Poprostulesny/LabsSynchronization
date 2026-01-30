using Ceres9.Core.Domain;
using Ceres9.Core.Student;
using Ceres9.Core.Student.Boss;
using ThreadTimer = System.Threading.Timer;

namespace Ceres9.Gui.Teacher;

/// <summary>
/// Teacher-side controller that wires the GUI to the simulation engine.
/// Relies on student implementations in Ceres9.Core/Student.
/// </summary>
public sealed class StationController : IDisposable
{
    public event EventHandler<StationStateChangedEventArgs>? StateChanged;
    public event EventHandler<LogEventArgs>? LogEmitted;

    public StationState State { get; private set; } = StationState.Idle;
    public bool ChaosModeEnabled { get; private set; }

    private StationEngine? _engine;

    public void Start()
    {
        if (State is StationState.Running or StationState.Paused)
        {
            Emit("Already running.");
            return;
        }

        _engine?.Dispose();
        _engine = new StationEngine(Emit, OnIntegrityViolation);
        _engine.Start();

        State = StationState.Running;
        Emit("Engine started.");
        RaiseStateChanged();
    }

    public void Pause()
    {
        if (State is not StationState.Running)
        {
            Emit("Cannot pause: not running.");
            return;
        }

        State = StationState.Paused;
        _engine?.Pause();
        Emit("Paused.");
        RaiseStateChanged();
    }

    public void Resume()
    {
        if (State is not StationState.Paused)
        {
            Emit("Cannot resume: not paused.");
            return;
        }

        State = StationState.Running;
        _engine?.Resume();
        Emit("Resumed.");
        RaiseStateChanged();
    }

    public async void Stop()
    {
        if (State is StationState.Idle)
        {
            Emit("Already stopped.");
            return;
        }

        Emit("Stopping...");
        await (_engine?.StopAsync() ?? Task.CompletedTask);
        _engine?.Dispose();
        _engine = null;

        State = StationState.Idle;
        Emit("Stopped.");
        RaiseStateChanged();
    }

    public void Reset()
    {
        _engine?.Dispose();
        _engine = null;
        State = StationState.Idle;
        ChaosModeEnabled = false;
        Emit("Reset.");
        RaiseStateChanged();
    }

    public void SpawnShip()
    {
        if (State != StationState.Running)
        {
            Emit("Cannot spawn: engine not running.");
            return;
        }

        _engine?.SpawnShip(manual: true);
    }

    public void ToggleChaosMode()
    {
        ChaosModeEnabled = !ChaosModeEnabled;
        _engine?.SetChaosMode(ChaosModeEnabled);
        Emit($"Chaos Mode: {(ChaosModeEnabled ? "ON" : "OFF")}.");
    }

    private void OnIntegrityViolation(string message) => Emit("[VIOLATION] " + message);

    private void RaiseStateChanged() => StateChanged?.Invoke(this, new StationStateChangedEventArgs(State));
    private void Emit(string message) => LogEmitted?.Invoke(this, new LogEventArgs(message));

    public void Dispose()
    {
        _engine?.Dispose();
    }
}

public enum StationState
{
    Idle = 0,
    Running = 1,
    Paused = 2,
}

public sealed class StationStateChangedEventArgs(StationState state) : EventArgs
{
    public StationState State { get; } = state;
}

public sealed class LogEventArgs(string message) : EventArgs
{
    public string Message { get; } = message;
}

/// <summary>
/// Minimal teacher-side simulation engine that exercises student code.
/// </summary>
internal sealed class StationEngine : IDisposable
{
    private readonly Action<string> _log;
    private readonly Action<string> _violation;

    private readonly StopSignal _stop = new();
    private readonly PauseGate _pause = new();
    private readonly DockPermitPool _docks = new(4);
    private readonly WorkOrderQueue _queue = new(32);
    private readonly Scoreboard _scoreboard = new();
    private readonly CustomsTurnstile _customs = new();
    private readonly WarehouseInventory _inventory = new();
    private readonly ShutdownLatch _latch;
    private readonly ShiftCoordinator _shift;

    private readonly List<Task> _workers = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Random _random = new();
    private ThreadTimer? _spawnTimer;
    private ThreadTimer? _customsTimer;
    private bool _chaos;

    public StationEngine(Action<string> log, Action<string> violation)
    {
        _log = log;
        _violation = violation;
        int workerCount = 6;
        _latch = new ShutdownLatch(workerCount);
        _shift = new ShiftCoordinator(workerCount, shift => _log($"Shift {shift} complete."));
    }

    public void Start()
    {
        _stop.Reset();
        _spawnTimer = new ThreadTimer(_ => SpawnShip(), null, 0, 200);
        _customsTimer = new ThreadTimer(_ => _customs.AllowNextShip(), null, 0, 60);

        for (int i = 0; i < 6; i++)
        {
            _workers.Add(Task.Run(() => WorkerLoop(i, _cts.Token)));
        }
    }

    public void Pause() => _pause.Pause();
    public void Resume() => _pause.Resume();

    public void SetChaosMode(bool enabled)
    {
        _chaos = enabled;
        if (_chaos)
        {
            _spawnTimer?.Change(0, 80);
            _customsTimer?.Change(0, 25);
        }
        else
        {
            _spawnTimer?.Change(0, 200);
            _customsTimer?.Change(0, 60);
        }
    }

    public void SpawnShip(bool manual = false)
    {
        if (manual)
        {
            _log("Manual ship spawn.");
        }
        var orderId = DateTime.UtcNow.Ticks ^ Environment.TickCount64;
        var shipId = (int)(orderId % int.MaxValue);
        var company = _random.Next(0, 2) == 0 ? "ACME" : "Weyland";
        var crates = _random.Next(1, 6);
        var credits = crates * 10;
        var order = new WorkOrder(orderId, shipId, company, "Cargo", crates, credits, DateTimeOffset.UtcNow);
        try
        {
            _queue.Enqueue(order, _cts.Token);
        }
        catch (OperationCanceledException) { }
    }

    private async Task WorkerLoop(int workerId, CancellationToken token)
    {
        try
        {
            while (!_stop.IsStopRequested)
            {
                _pause.WaitIfPaused(token);

                _customs.WaitForClearance(token);

                WorkOrder? order;
                if (!_queue.TryDequeue(out order, TimeSpan.FromMilliseconds(200), token))
                {
                    if (_stop.IsStopRequested) break;
                    continue;
                }

                await using var permit = await _docks.AcquireAsync(token);

                // process order
                _inventory.Add(order.CargoType, order.Crates);
                await Task.Delay(50, token); // simulate work
                bool ok = _inventory.TryTake(order.CargoType, order.Crates);
                if (!ok)
                {
                    _violation($"Negative inventory risk on {order.CargoType}");
                }

                _scoreboard.RecordDelivery(order.Company, order.Crates, order.Credits);
                _shift.SignalAndWait(token);
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        finally
        {
            _latch.WorkerDone();
        }
    }

    public async Task StopAsync()
    {
        _stop.RequestStop();
        _queue.Complete();
        _cts.Cancel();
        _spawnTimer?.Dispose();
        _spawnTimer = null;
        _customsTimer?.Dispose();
        _customsTimer = null;

        await Task.WhenAny(Task.WhenAll(_workers), Task.Delay(2000));
        _latch.WaitAll(TimeSpan.FromSeconds(2), CancellationToken.None);
    }

    private void OnIntegrityViolation(string message) => _violation(message);

    public void Dispose()
    {
        _spawnTimer?.Dispose();
        _customsTimer?.Dispose();
        _cts.Cancel();
        _queue.Dispose();
        _pause.Dispose();
        _docks.DisposeAsync().AsTask().Wait();
        _customs.Dispose();
        _inventory.Dispose();
        _shift.Dispose();
        _latch.Dispose();
    }
}

# Ceres‑9 Dockmaster: Cargo Chaos (C# Synchronization Lab)

## Story
You are the new **Dockmaster** of orbital station **Ceres‑9**. Tonight is the Intergalactic Festival.
Hundreds of ships arrive at once. Your station has:

- a limited number of **docks**
- a **customs** checkpoint that clears ships **one-by-one**
- a shared **warehouse inventory** read constantly by the dashboard
- multiple worker crews operating in synchronized **shifts**

The GUI and test harness are prepared by the teacher. **Your job is to implement the missing synchronization modules** in `Ceres9.Core`.

If you implement them correctly:
- the station never overbooks docks,
- inventory never goes negative,
- the app always stops cleanly,
- and stress tests pass reliably.

---

## What you edit (students)
Only files in:

- `Ceres9.Core/Student/`
- `Ceres9.Core/Student/Boss/` (optional boss fights)

All student tasks are currently `throw new NotImplementedException();`.
Replace those throws with correct implementations **without changing public method signatures**.

## What you do NOT edit (students)
- `Ceres9.Gui/**` (teacher GUI)
- `Ceres9.Tests/**` (teacher tests)
- method signatures / namespaces in `Ceres9.Core/Student/**`

---

## How to run
From the solution directory:

- GUI (teacher will complete the engine later): `dotnet run --project Ceres9.Gui`
- Tests: `dotnet test`

> Note: in this skeleton, most tests are marked `[Ignore]` and the GUI only logs placeholder messages.
> In the final lab, the teacher removes `[Ignore]`, implements tests, and wires the GUI to the engine.

---

# Stages (students implement these)

Each stage below lists:
- **File(s) to edit**
- **Members to implement**
- **Exact behavior requirements**
- **Hints** (what tools from the materials are likely relevant)
- **How it will be tested** (teacher side)

---

## Stage 1 — Stop button that never hangs (memory visibility)

### File(s) to edit
- `Ceres9.Core/Student/StopSignal.cs`

### Implement these members
- `StopSignal.RequestStop()`
- `StopSignal.Reset()`
- `StopSignal.IsStopRequested` (getter)

### Required behavior
- `RequestStop()`:
    - After it returns, *other threads* must reliably observe `IsStopRequested == true` even if they are in a tight loop.
    - It must be safe if called multiple times.
- `Reset()`:
    - After it returns, `IsStopRequested` becomes `false` again.
    - Allows reusing the same instance for a new simulation run.
- `IsStopRequested`:
    - Must be safe to read very frequently by many threads.

### Hints (materials)
- Plain `bool` without memory barriers can be “invisible” to other threads in tight loops.
- Consider `Volatile.Read/Write` or an `Interlocked`-based flag.

### Teacher verification (tests + GUI)
- A stress test starts many hot-loop workers, calls `RequestStop()`, and asserts all workers exit within a timeout.
- GUI “Stop” should always return to Idle quickly (no permanent “Stopping…”).

---

## Stage 2 — Unique IDs and atomic counters (Interlocked)

### File(s) to edit
- `Ceres9.Core/Student/IdGenerator.cs`
- `Ceres9.Core/Student/AtomicCounter.cs`

### Implement these members
- `IdGenerator.NextId()`
- `AtomicCounter.Value` (getter)
- `AtomicCounter.Increment()`
- `AtomicCounter.Add(long delta)`

### Required behavior
**IdGenerator**
- `NextId()` must return a new unique ID every time.
- Must not produce duplicates under concurrency.
- IDs must be strictly increasing relative to the starting value.

**AtomicCounter**
- `Increment()` increases by exactly 1 and returns the new value.
- `Add(delta)` increases by `delta` (delta can be negative) and returns the new value.
- `Value` returns the current value in a thread-safe way.

### Hints (materials)
- `counter++` is not atomic.
- Look at `Interlocked.Increment`, `Interlocked.Add`, `Interlocked.Read`.

### Teacher verification (tests + GUI)
- Parallel generation of IDs → all unique.
- Heavy concurrent increments → final total exactly matches expected.
- GUI should never show duplicate ship IDs; totals should be consistent after stress.

---

## Stage 3 — Dock permits (SemaphoreSlim + concurrent collection)

### File(s) to edit
- `Ceres9.Core/Student/DockPermitPool.cs`

### Implement these members
- `DockPermitPool.AcquireAsync(CancellationToken token)`
- `DockPermitPool.DisposeAsync()`
- `DockPermit.DockIndex` (getter)
- `DockPermit.DisposeAsync()`

### Required behavior
Your station has `dockCount` docking bays. A ship can only dock if it holds a permit.

**Acquire**
- `AcquireAsync(token)` waits until a dock is free, then returns a `DockPermit`.
- The returned permit must have a valid `DockIndex` in `[0..dockCount-1]`.
- While permits are active, **no two active permits may have the same `DockIndex`**.
- If `token` is cancelled while waiting, `AcquireAsync` must stop waiting and throw `OperationCanceledException`.

**Release**
- Calling `DisposeAsync()` on the permit must:
    - return the dock index to the pool
    - release capacity so another waiter can proceed
    - be safe even if called more than once (no double-release bugs)

**Pool dispose**
- Disposing the pool should release native resources (if any are used).

### Hints (materials)
- `SemaphoreSlim` is intended for “at most N concurrent users”.
- To track which concrete dock index is free, seed a thread-safe collection with indices (0..dockCount-1).

### Teacher verification (tests + GUI)
- Stress test acquires/releases from many tasks and asserts:
    - max simultaneous permits ≤ dockCount
    - dock indices are never duplicated among active permits
- GUI never shows “more ships docked than docks”.

---

## Stage 4 — Work orders pipeline + scoreboard (BlockingCollection + ConcurrentDictionary)

### File(s) to edit
- `Ceres9.Core/Student/WorkOrderQueue.cs`
- `Ceres9.Core/Student/Scoreboard.cs`

### Stage 4A: WorkOrderQueue

#### Implement these members
- `WorkOrderQueue.Enqueue(WorkOrder order, CancellationToken token)`
- `WorkOrderQueue.TryDequeue(out WorkOrder? order, TimeSpan timeout, CancellationToken token)`
- `WorkOrderQueue.Complete()`
- `WorkOrderQueue.Dispose()`

#### Required behavior
The simulation has producers (ship arrivals) and consumers (dock workers).

**Enqueue**
- Adds an order to the queue.
- If the queue is full:
    - block until space becomes available **or** cancellation is requested.
- Must throw `OperationCanceledException` if `token` is cancelled before the add completes.

**TryDequeue**
- Waits up to `timeout` for an item.
- Returns:
    - `true` and sets `order` when an item is obtained,
    - `false` and sets `order = null` when:
        - the queue is completed and empty, **or**
        - the timeout expires with no item.
- Must respect `token` (throw if cancelled while waiting).

**Complete**
- Signals “no more items will be enqueued”.
- Consumers should eventually stop getting items once the queue is drained.

**Dispose**
- Releases internal resources.

#### Hints (materials)
- `BlockingCollection<T>` already implements bounded producer/consumer behavior.
- It can wrap an `IProducerConsumerCollection<T>` like `ConcurrentQueue<T>`.

#### Teacher verification (tests + GUI)
- Produce M work orders and consume with K workers:
    - each order is processed **exactly once**
    - no duplicates, no lost orders
- GUI queue length should go up and down; total processed should match total created when simulation ends.

### Stage 4B: Scoreboard

#### Implement these members
- `Scoreboard.RecordDelivery(string company, int crates, int credits)`
- `Scoreboard.Get(string company)`
- `Scoreboard.Top(int count)`

#### Required behavior
- `RecordDelivery` is called by many worker threads concurrently.
- For each company, you must aggregate:
    - number of ships processed,
    - total crates delivered,
    - total credits earned.
- `Get(company)` returns the current stats for that company.
    - If the company does not exist yet, return zeros (ships/crates/credits all 0).
- `Top(count)` returns up to `count` best companies by **Credits** (descending).

#### Hints (materials)
- `ConcurrentDictionary<TKey,TValue>` with `AddOrUpdate` is intended for lock-free-ish aggregation.

#### Teacher verification (tests + GUI)
- Concurrent updates from many tasks → totals match the sum of all recorded deliveries.
- GUI “Top Companies” panel updates without glitches or negative totals.

---

## Stage 5 — Pause/Resume + Customs “one ship at a time” (signaling)

### File(s) to edit
- `Ceres9.Core/Student/PauseGate.cs`
- `Ceres9.Core/Student/CustomsTurnstile.cs`

### Stage 5A: PauseGate

#### Implement these members
- `PauseGate.IsPaused` (getter)
- `PauseGate.Pause()`
- `PauseGate.Resume()`
- `PauseGate.WaitIfPaused(CancellationToken token)`
- `PauseGate.Dispose()`

#### Required behavior
- `Pause()` switches the gate to “paused”.
- While paused, any worker calling `WaitIfPaused(token)` must block until:
    - `Resume()` is called (then it proceeds), or
    - `token` is cancelled (then it throws).
- When not paused, `WaitIfPaused` must return quickly (no busy wait).

#### Hints (materials)
- `ManualResetEventSlim` is a classic “gate”: open lets everyone through; closed blocks everyone.

### Stage 5B: CustomsTurnstile

#### Implement these members
- `CustomsTurnstile.AllowNextShip()`
- `CustomsTurnstile.WaitForClearance(CancellationToken token)`
- `CustomsTurnstile.Dispose()`

#### Required behavior
- Many ships can wait at customs simultaneously.
- Each call to `AllowNextShip()` must allow **exactly one** ship to pass.
    - If nobody is waiting, the “permit” should be stored so that the next waiter passes immediately.
- `WaitForClearance(token)` must block until a permit is available or cancellation is requested.

#### Hints (materials)
- `AutoResetEvent` behaves like a turnstile: one signal releases one waiter.
- For cancellation with wait handles, you can combine handles (`WaitAny`) including `token.WaitHandle`.

### Teacher verification (tests + GUI)
- Pause freezes processed counters; resume continues.
- Customs clearance decreases the waiting count exactly by 1 per clearance.

---

## Stage 6 — Warehouse inventory (ReaderWriterLockSlim)

### File(s) to edit
- `Ceres9.Core/Student/WarehouseInventory.cs`

### Implement these members
- `WarehouseInventory.GetQuantity(string item)`
- `WarehouseInventory.Add(string item, int amount)`
- `WarehouseInventory.TryTake(string item, int amount)`
- `WarehouseInventory.Snapshot()`
- `WarehouseInventory.Dispose()`

### Required behavior
- `GetQuantity` returns current quantity (0 if the item doesn’t exist yet).
- `Add(item, amount)` increases quantity; amount is positive.
- `TryTake(item, amount)`:
    - returns `true` and decrements quantity if enough stock exists,
    - returns `false` and leaves quantity unchanged if not enough stock exists,
    - must **never** allow quantity to become negative.
- `Snapshot()` returns a consistent view for GUI refresh:
    - while building the snapshot, inventory must not change “halfway”.

### Hints (materials)
- This is a “many readers, few writers” scenario → `ReaderWriterLockSlim`.
- Use `try/finally` for lock release.

### Teacher verification (tests + GUI)
- Stress test with many readers + writers:
    - no exceptions
    - no negative inventory
    - snapshot totals remain consistent
- GUI inventory grid never shows negative quantities.

---

## Stage 7 — Shifts + clean shutdown (Barrier + CountdownEvent)

### File(s) to edit
- `Ceres9.Core/Student/ShiftCoordinator.cs`
- `Ceres9.Core/Student/ShutdownLatch.cs`

### Stage 7A: ShiftCoordinator (Barrier)

#### Implement these members
- `ShiftCoordinator.CurrentShift` (getter)
- `ShiftCoordinator.SignalAndWait(CancellationToken token)`
- `ShiftCoordinator.Dispose()`

#### Required behavior
- There are `participants` workers.
- Each shift:
    - every worker does its work for that shift,
    - then calls `SignalAndWait(token)` once,
    - and must not start the next shift until everyone arrived.
- `afterShift(shiftNumber)` must execute exactly once per shift (after all participants arrived).
- Cancellation must be supported (if a worker is cancelled while waiting).

#### Hints (materials)
- `Barrier` has a post-phase action and tracks phases.

### Stage 7B: ShutdownLatch (CountdownEvent)

#### Implement these members
- `ShutdownLatch.WorkerDone()`
- `ShutdownLatch.WaitAll(TimeSpan timeout, CancellationToken token)`
- `ShutdownLatch.Dispose()`

#### Required behavior
- Constructed with `workerCount`.
- Each worker calls `WorkerDone()` exactly once when it exits.
- `WaitAll(timeout, token)`:
    - returns `true` if all workers are done within `timeout`,
    - returns `false` if timeout expires first,
    - throws `OperationCanceledException` if `token` is cancelled while waiting.

#### Hints (materials)
- `CountdownEvent` is exactly “wait until N signals”.

### Teacher verification (tests + GUI)
- Shift counter increments reliably (no worker “runs ahead”).
- Stop waits for workers and always returns to Idle.

---

# Boss fights (optional extra credit)

## Boss 1 — Custom bounded queue (Monitor.Wait/Pulse)
### File to edit
- `Ceres9.Core/Student/Boss/MonitorBoundedQueue.cs`

### Implement
- `MonitorBoundedQueue.Enqueue(T item, CancellationToken token)`
- `MonitorBoundedQueue.Dequeue(CancellationToken token)`

### Requirements
- Bounded capacity, blocking when full/empty.
- Correct under concurrency.
- Must re-check conditions in a loop (spurious wakeups / races).

## Boss 2 — Single-instance app (named Mutex)
### File to edit
- `Ceres9.Core/Student/Boss/SingleInstanceGuard.cs`

### Implement
- `SingleInstanceGuard.TryAcquire(string name, out SingleInstanceGuard? guard)`
- `SingleInstanceGuard.Dispose()`

### Requirements
- First process acquires the named mutex and returns `true`.
- Second process returns `false` (guard is null) and should exit (teacher wires GUI message).

## Boss 3 — SpinLock telemetry (SpinLock)
### File to edit
- `Ceres9.Core/Student/Boss/SpinLockedStats.cs`

### Implement
- `SpinLockedStats.AddSample(int value)`
- `SpinLockedStats.Snapshot()`

### Requirements
- Correct under contention.
- Keep the critical section minimal (no I/O, no allocation-heavy work while holding the spin lock).

---

# Teacher section (implementation checklist)

## GUI / engine wiring (teacher)
Project: `Ceres9.Gui`

- Implement simulation engine behind `Ceres9.Gui/Teacher/StationController.cs`:
    - start/stop worker tasks,
    - spawn ships (producers),
    - dock workers (consumers),
    - connect to student modules in `Ceres9.Core/Student/*`.
- Add dashboard panels:
    - dock occupancy (count + dock indices),
    - customs queue length,
    - work queue length,
    - inventory grid,
    - top companies list,
    - integrity violations list.
- Add “Chaos Mode (30s)”:
    - random ship bursts,
    - random pauses,
    - random cancellations,
    - random slowdowns.

## Tests (teacher)
Project: `Ceres9.Tests`

- Remove `[Ignore]` and implement:
    - stress tests with timeouts for every stage,
    - correctness invariants (no duplicates, no negatives, max concurrency ≤ N),
    - repeated runs (to catch flaky visibility bugs).

Suggested pattern:
- run each stress test multiple times in a loop (e.g., 20 iterations),
- keep timeouts strict but realistic,
- prefer `CancellationTokenSource` for stopping workers.

---

## Submission (students)
- Submit only `Ceres9.Core/Student/**` (and optionally `Ceres9.Core/Student/Boss/**`).
- Code must pass tests on the lab machine in Release configuration.


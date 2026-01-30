using Ceres9.Gui.Teacher;

namespace Ceres9.Gui;

public sealed class MainForm : Form
{
    private readonly StationController _controller = new();

    private readonly Button _startButton = new() { Text = "Start" };
    private readonly Button _pauseResumeButton = new() { Text = "Pause" };
    private readonly Button _spawnShipButton = new() { Text = "Spawn Ship" };
    private readonly Button _chaosButton = new() { Text = "Chaos Mode" };
    private readonly Button _stopButton = new() { Text = "Stop" };
    private readonly Button _resetButton = new() { Text = "Reset" };

    private readonly Label _stateLabel = new() { AutoSize = true };
    private readonly Label _integrityLabel = new() { AutoSize = true, Text = "Integrity: (teacher implements)" };
    private readonly ListBox _logList = new() { HorizontalScrollbar = true };

    public MainForm()
    {
        Text = "Ceres-9 Dockmaster (Lab Skeleton)";
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(10),
            WrapContents = true,
        };

        buttonsPanel.Controls.AddRange(
        [
            _startButton,
            _pauseResumeButton,
            _spawnShipButton,
            _chaosButton,
            _stopButton,
            _resetButton,
        ]);

        var statusPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(10, 0, 10, 10),
            WrapContents = true,
        };

        statusPanel.Controls.AddRange([_stateLabel, _integrityLabel]);

        _logList.Dock = DockStyle.Fill;

        Controls.Add(_logList);
        Controls.Add(statusPanel);
        Controls.Add(buttonsPanel);

        _startButton.Click += (_, _) => _controller.Start();
        _pauseResumeButton.Click += (_, _) =>
        {
            if (_controller.State is StationState.Paused)
            {
                _controller.Resume();
            }
            else
            {
                _controller.Pause();
            }
        };
        _spawnShipButton.Click += (_, _) => _controller.SpawnShip();
        _chaosButton.Click += (_, _) => _controller.ToggleChaosMode();
        _stopButton.Click += (_, _) => _controller.Stop();
        _resetButton.Click += (_, _) => _controller.Reset();

        _controller.LogEmitted += (_, e) => AppendLog(e.Message);
        _controller.StateChanged += (_, e) =>
        {
            _stateLabel.Text = $"State: {e.State}";
            _pauseResumeButton.Text = e.State is StationState.Paused ? "Resume" : "Pause";
        };

        _controller.Reset();
    }

    private void AppendLog(string message)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(() => AppendLog(message));
            return;
        }

        _logList.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        _logList.TopIndex = _logList.Items.Count - 1;
    }
}


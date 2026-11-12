using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using WuWaSwitcher;

public class DownloadProgressForm : Form {
    private Label lblCurrentPath;
    private Label lblCurrentSize;
    private Label lblPercent;
    private ProgressBar progressBar;
    private Button btnCancel;
    private ToolTip toolTip;
    private CancellationTokenSource _cts;
    private System.Windows.Forms.Timer _refreshTimer;   // 注意：与 System.Threading.Timer 冲突，用全名

    // 后台线程只写这个快照（引用赋值原子，volatile 保证可见性）
    private class ProgressSnapshot {
        public string Path;
        public string Size;
        public int Percent;
    }
    private volatile ProgressSnapshot _pending;

    // UI 线程已显示的值，用来跳过无变化的重绘
    private string _displayedPath;
    private string _displayedSize;
    private int _displayedPercent = -1;

    public CancellationToken Token { get { return _cts.Token; } }

    public DownloadProgressForm(ClientType clientType, long totalSize) {
        _cts = new CancellationTokenSource();

        Text = $"{clientType}-正在下载 ({ResourceDiff.FormatSize(totalSize)})";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = false;
        ClientSize = new Size(560, 150);
        ShowInTaskbar = true;

        toolTip = new ToolTip();

        lblCurrentPath = new Label {
            AutoSize = false,
            AutoEllipsis = true,
            Location = new Point(12, 12),
            Size = new Size(430, 20),
            Text = "准备下载..."
        };

        lblCurrentSize = new Label {
            AutoSize = false,
            Location = new Point(448, 12),
            Size = new Size(100, 20),
            TextAlign = ContentAlignment.MiddleRight,
            Text = ""
        };

        progressBar = new ProgressBar {
            Location = new Point(12, 40),
            Size = new Size(536, 24),
            Minimum = 0,
            Maximum = 100
        };

        lblPercent = new Label {
            AutoSize = false,
            Location = new Point(12, 70),
            Size = new Size(300, 20),
            Text = "0%"
        };

        btnCancel = new Button {
            Location = new Point(432, 105),
            Size = new Size(116, 30),
            Text = "取消",
            Enabled = true
        };
        btnCancel.Click += (s, e) => {
            btnCancel.Enabled = false;
            btnCancel.Text = "正在取消...";
            try { _cts.Cancel(); } catch { }
        };

        Controls.Add(lblCurrentPath);
        Controls.Add(lblCurrentSize);
        Controls.Add(progressBar);
        Controls.Add(lblPercent);
        Controls.Add(btnCancel);

        // 100ms 定时刷新 UI，避免被进度回调淹没
        _refreshTimer = new System.Windows.Forms.Timer { Interval = 100 };
        _refreshTimer.Tick += (s, e) => RefreshUI();
        _refreshTimer.Start();
    }

    /// <summary>
    /// 线程安全地提交进度（后台下载线程可高频调用）。
    /// 只写一个快照字段，不碰任何控件，也不 BeginInvoke。
    /// </summary>
    public void UpdateProgress(string filePath, string formattedSize, int totalPercent) {
        if (IsDisposed) return;

        _pending = new ProgressSnapshot {
            Path = filePath,
            Size = formattedSize,
            Percent = totalPercent
        };
    }

    /// <summary>UI 线程定时调用，把待刷新数据同步到控件</summary>
    private void RefreshUI() {
        if (IsDisposed) return;

        var snap = _pending;
        if (snap == null) return;

        // 路径：只在变化时更新文本 + tooltip（避免 tooltip 闪烁）
        if (snap.Path != _displayedPath) {
            _displayedPath = snap.Path;
            lblCurrentPath.Text = "正在下载: " + snap.Path;
            toolTip.SetToolTip(lblCurrentPath, snap.Path);
        }

        // 大小：只在变化时更新
        if (snap.Size != _displayedSize) {
            _displayedSize = snap.Size;
            lblCurrentSize.Text = snap.Size;
        }

        // 百分比：只在变化时更新（ProgressBar.Value 赋值会触发重绘）
        if (snap.Percent != _displayedPercent) {
            _displayedPercent = snap.Percent;
            int v = Math.Max(0, Math.Min(100, snap.Percent));
            if (progressBar.Value != v) progressBar.Value = v;
            lblPercent.Text = snap.Percent + "%";
        }
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (_refreshTimer != null) {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
            }
            if (_cts != null) _cts.Dispose();
            if (toolTip != null) toolTip.Dispose();
        }
        base.Dispose(disposing);
    }
}
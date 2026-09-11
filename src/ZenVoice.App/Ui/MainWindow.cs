using System.Runtime.InteropServices;
using ZenVoice.Core;

namespace ZenVoice.App.Ui;

internal sealed class MainWindow : IHud
{
    private static readonly (string Id, string Title, int Cmd)[] Destinations =
    [
        ("home", "Home", UiIds.NavHome),
        ("models", "Models", UiIds.NavModels),
        ("history", "History", UiIds.NavHistory),
        ("personalisation", "Personalisation", UiIds.NavFormatting),
        ("shortcuts", "Shortcuts", UiIds.NavShortcuts),
        ("settings", "Settings", UiIds.NavPrivacy)
    ];

    private readonly Win32.WndProc _proc;
    private readonly Dictionary<string, ISettingsPage> _pages;
    private readonly IAudioCapture? _capture;
    private ISettingsPage? _current;
    private DictationPhase _phase = DictationPhase.Idle;
    private string _detail = "";
    private double _level;
    private string _nav = "home";

    public IntPtr Handle { get; }

    public MainWindow(
        IAudioCapture? capture,
        TranscriptVault vault,
        Action<string>? downloadModel = null,
        Action? deleteHistory = null)
    {
        _capture = capture;
        Theme.Init();
        NativeUi.Init();
        _pages = new Dictionary<string, ISettingsPage>
        {
            ["home"] = new OverviewPage(),
            ["models"] = new ModelsPage(downloadModel),
            ["history"] = new HistoryPage(vault, deleteHistory),
            ["personalisation"] = new FormattingPage(),
            ["shortcuts"] = new ShortcutsPage(),
            ["settings"] = new PrivacyPage(vault, deleteHistory)
        };

        _proc = WndProc;
        var inst = Win32.GetModuleHandle(null);
        var wnd = new Win32.WNDCLASS
        {
            style = 0x0003,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_proc),
            hInstance = inst,
            hCursor = Win32.LoadCursor(IntPtr.Zero, (IntPtr)Win32.IDC_ARROW),
            hbrBackground = Theme.CanvasBrush,
            lpszClassName = "ZenVoiceMain"
        };
        Win32.RegisterClassW(ref wnd);
        Handle = Win32.CreateWindowExW(
            0, "ZenVoiceMain", "ZenVoice",
            Win32.WS_OVERLAPPEDWINDOW,
            80, 80, 960, 640,
            IntPtr.Zero, IntPtr.Zero, inst, IntPtr.Zero);
        if (Handle != IntPtr.Zero)
        {
            Win32.SetTimer(Handle, (UIntPtr)1, 100, IntPtr.Zero);
            Select("home");
            Reveal();
        }
    }

    public void Reveal()
    {
        if (Handle == IntPtr.Zero)
        {
            return;
        }

        Win32.ShowWindow(Handle, Win32.IsIconic(Handle) ? Win32.SW_RESTORE : Win32.SW_SHOW);
        Win32.SetWindowPos(Handle, (IntPtr)Win32.HWND_TOP, 0, 0, 0, 0,
            Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_SHOWWINDOW);
        Win32.SetForegroundWindow(Handle);
        Win32.UpdateWindow(Handle);
    }

    public void Show(DictationPhase phase, string? detail = null, double level = 0)
    {
        _phase = phase;
        _detail = detail ?? "";
        _level = level;
        if (Handle != IntPtr.Zero)
        {
            Win32.InvalidateRect(Handle, IntPtr.Zero, false);
        }

        _current?.Refresh();
    }

    private void Select(string id)
    {
        _current?.Unmount();
        _nav = id;
        _current = _pages[id];
        _current.Mount(Handle);
        Layout();
        _current.Refresh();
        Win32.InvalidateRect(Handle, IntPtr.Zero, true);
    }

    private void Layout()
    {
        if (Handle == IntPtr.Zero)
        {
            return;
        }

        Win32.GetClientRect(Handle, out var rc);
        var bounds = new Win32.RECT
        {
            Left = Theme.SidebarWidth + 24,
            Top = Theme.HeaderHeight + 72,
            Right = rc.Right - 24,
            Bottom = rc.Bottom - 24
        };
        _current?.Layout(bounds);
    }
    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            return HandleMessage(hWnd, msg, wParam, lParam);
        }
        catch
        {
            return Win32.DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }

    private IntPtr HandleMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == Win32.WM_TIMER)
        {
            if (_phase == DictationPhase.Listening && _capture is not null)
            {
                _level = _capture.Level;
                Win32.InvalidateRect(hWnd, IntPtr.Zero, false);
            }

            return IntPtr.Zero;
        }

        if (msg == Win32.WM_SIZE)
        {
            Layout();
            Win32.InvalidateRect(hWnd, IntPtr.Zero, true);
            return IntPtr.Zero;
        }

        if (msg == Win32.WM_CLOSE)
        {
            Win32.ShowWindow(hWnd, Win32.SW_HIDE);
            return IntPtr.Zero;
        }

        if (msg == Win32.WM_LBUTTONDOWN)
        {
            var x = (short)(lParam.ToInt64() & 0xFFFF);
            var y = (short)((lParam.ToInt64() >> 16) & 0xFFFF);
            if (x < Theme.SidebarWidth && y > Theme.HeaderHeight)
            {
                var i = (y - Theme.HeaderHeight - 8) / Theme.NavRow;
                if (i >= 0 && i < Destinations.Length)
                {
                    Select(Destinations[i].Id);
                }
            }

            return IntPtr.Zero;
        }

        if (msg == Win32.WM_COMMAND)
        {
            var id = (int)(wParam.ToInt64() & 0xFFFF);
            if (_current?.HandleCommand(id) == true)
            {
                return IntPtr.Zero;
            }
        }

        if (msg is Win32.WM_CTLCOLORSTATIC or Win32.WM_CTLCOLORBTN)
        {
            Win32.SetTextColor(wParam, Theme.Text);
            Win32.SetBkColor(wParam, Theme.Canvas);
            Win32.SetBkMode(wParam, Win32.TRANSPARENT);
            return Theme.CanvasBrush;
        }

        if (msg == Win32.WM_CTLCOLOREDIT)
        {
            Win32.SetTextColor(wParam, Theme.Text);
            Win32.SetBkColor(wParam, Theme.Raised);
            return Theme.RaisedBrush;
        }

        if (msg == Win32.WM_PAINT)
        {
            var hdc = Win32.BeginPaint(hWnd, out var ps);
            Win32.GetClientRect(hWnd, out var rc);
            Win32.FillRect(hdc, ref rc, Theme.CanvasBrush);
            var side = rc;
            side.Right = Theme.SidebarWidth;
            Win32.FillRect(hdc, ref side, Theme.SurfaceBrush);

            Win32.SetBkMode(hdc, Win32.TRANSPARENT);
            var brand = new Win32.RECT { Left = 20, Top = 16, Right = Theme.SidebarWidth - 12, Bottom = 48 };
            Win32.SetTextColor(hdc, Theme.Text);
            if (NativeUi.FontTitle != IntPtr.Zero)
            {
                Win32.SelectObject(hdc, NativeUi.FontTitle);
            }

            Win32.DrawTextW(hdc, "ZenVoice", -1, ref brand, Win32.DT_LEFT | Win32.DT_TOP);

            if (NativeUi.Font != IntPtr.Zero)
            {
                Win32.SelectObject(hdc, NativeUi.Font);
            }

            for (var i = 0; i < Destinations.Length; i++)
            {
                var row = new Win32.RECT
                {
                    Left = 8,
                    Top = Theme.HeaderHeight + 8 + i * Theme.NavRow,
                    Right = Theme.SidebarWidth - 8,
                    Bottom = Theme.HeaderHeight + 8 + (i + 1) * Theme.NavRow - 4
                };
                if (Destinations[i].Id == _nav)
                {
                    Win32.FillRect(hdc, ref row, Theme.RaisedBrush);
                    Win32.SetTextColor(hdc, Theme.Text);
                }
                else
                {
                    Win32.SetTextColor(hdc, Theme.Secondary);
                }

                var textRc = row;
                textRc.Left += 16;
                Win32.DrawTextW(hdc, Destinations[i].Title, -1, ref textRc,
                    Win32.DT_LEFT | Win32.DT_VCENTER | Win32.DT_SINGLELINE);
            }

            var title = _current?.Title ?? "ZenVoice";
            var sub = _current?.Subtitle ?? "";
            var head = new Win32.RECT
            {
                Left = Theme.SidebarWidth + 24,
                Top = 16,
                Right = rc.Right - 24,
                Bottom = 44
            };
            Win32.SetTextColor(hdc, Theme.Text);
            if (NativeUi.FontTitle != IntPtr.Zero)
            {
                Win32.SelectObject(hdc, NativeUi.FontTitle);
            }

            Win32.DrawTextW(hdc, title, -1, ref head, Win32.DT_LEFT | Win32.DT_TOP);
            var subRc = new Win32.RECT
            {
                Left = Theme.SidebarWidth + 24,
                Top = 44,
                Right = rc.Right - 24,
                Bottom = Theme.HeaderHeight + 36
            };
            Win32.SetTextColor(hdc, Theme.Secondary);
            if (NativeUi.Font != IntPtr.Zero)
            {
                Win32.SelectObject(hdc, NativeUi.Font);
            }

            var meter = _phase == DictationPhase.Listening
                ? "  " + new string('|', Math.Clamp((int)(_level * 12), 0, 12)).PadRight(12, '.')
                : "";
            Win32.DrawTextW(
                hdc,
                $"{sub}\n{_phase}{meter}" + (_detail.Length == 0 ? "" : "  —  " + _detail),
                -1, ref subRc, Win32.DT_LEFT | Win32.DT_TOP | Win32.DT_WORDBREAK);
            Win32.EndPaint(hWnd, ref ps);
            return IntPtr.Zero;
        }

        return Win32.DefWindowProc(hWnd, msg, wParam, lParam);
    }
}

using System.Runtime.InteropServices;
using ZenVoice.App.Ui;
using ZenVoice.Core;

namespace ZenVoice.App;
internal static class Program
{
    private const int HotKeyId = 1;
    private const int CmdExit = 1;
    private const int CmdDownload = 2;
    private const int CmdDelete = 3;
    private const int CmdAutoStart = 4;
    private const int CmdShow = 5;
    private const uint MfSeparator = 0x00000800;
    private const uint MfChecked = 0x00000008;

    private static DictationController? _dictation;
    private static TranscriptVault? _vault;
    private static MainWindow? _hud;
    private static HoldToDictate? _hold;
    private static IntPtr _hwnd;
    private static Win32.WndProc? _proc;
    private static Win32.NOTIFYICONDATA _tray;

    [STAThread]
    private static void Main()
    {
        try
        {
            Run();
        }
        catch (Exception ex)
        {
            Log("crash", ex.ToString());
        }
    }

    private static void Run()
    {
        using var mutex = new Mutex(true, @"Local\ZenVoice.App", out var created);
        if (!created)
        {
            ShowExisting();
            return;
        }

        if (!OperatingSystem.IsWindows())
        {
            Environment.Exit(2);
            return;
        }

        _proc = WndProc;
        var wnd = new Win32.WNDCLASS
        {
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_proc),
            hInstance = Win32.GetModuleHandle(null),
            hCursor = Win32.LoadCursor(IntPtr.Zero, (IntPtr)Win32.IDC_ARROW),
            lpszClassName = "ZenVoiceHidden"
        };
        Win32.RegisterClassW(ref wnd);
        _hwnd = Win32.CreateWindowExW(
            0, "ZenVoiceHidden", "ZenVoice", 0,
            0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, wnd.hInstance, IntPtr.Zero);

        var data = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZenVoice");
        _vault = new TranscriptVault(Path.Combine(data, "Data", "history.db"), new WindowsVaultKey());
        var capture = new WindowsCapture();
        _hud = new MainWindow(
            capture,
            _vault,
            id => _ = DownloadModel(id),
            () => _hud?.Show(DictationPhase.Idle, "History deleted"));
        if (_hud.Handle == IntPtr.Zero)
        {
            Log("create", "MainWindow handle is 0 err=" + Marshal.GetLastPInvokeError());
        }

        _dictation = new DictationController(
            capture,
            new SettingsTranscriber(),
            new WindowsInserter(),
            _hud,
            _vault);
        ApplySessionSettings();
        AppSettings.Applied = ApplySessionSettings;
        if (AppSettings.Current.AutoStart)
        {
            AutoStart.Enable();
        }
        Win32.RegisterHotKey(_hwnd, HotKeyId, Win32.MOD_CONTROL | Win32.MOD_ALT, Win32.VK_SPACE);
        _hold = new HoldToDictate(
            () => _dictation?.Phase == DictationPhase.Listening,
            Toggle);
        _hold.Start();
        AddTray();
        _hud.Reveal();

        while (Win32.GetMessage(out var msg, IntPtr.Zero, 0, 0))
        {
            Win32.TranslateMessage(ref msg);
            Win32.DispatchMessage(ref msg);
        }

        _hold.Dispose();
        Win32.UnregisterHotKey(_hwnd, HotKeyId);
        Win32.Shell_NotifyIcon(Win32.NIM_DELETE, ref _tray);
        _vault.Dispose();
    }

    private static void ShowExisting()
    {
        var hwnd = Win32.FindWindowW("ZenVoiceMain", null);
        if (hwnd == IntPtr.Zero)
        {
            Log("mutex", "already running, no window");
            return;
        }

        Win32.ShowWindow(hwnd, Win32.SW_RESTORE);
        Win32.SetWindowPos(hwnd, (IntPtr)Win32.HWND_TOP, 0, 0, 0, 0,
            Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_SHOWWINDOW);
        Win32.SetForegroundWindow(hwnd);
    }

    private static void Log(string tag, string detail)
    {
        try
        {
            Directory.CreateDirectory(AppSettings.Dir);
            File.AppendAllText(
                Path.Combine(AppSettings.Dir, "launch.log"),
                $"{DateTime.Now:o} {tag} {detail}\n");
        }
        catch
        {
        }
    }

    private static ITranscriber ChooseTranscriber() => new SettingsTranscriber();

    private static void ApplySessionSettings()
    {
        if (_dictation is null)
        {
            return;
        }

        _dictation.CleanTranscript = AppSettings.Current.Formatting != "off";
        _dictation.SaveHistory = AppSettings.Current.SaveHistory;
        _dictation.EngineId = AppSettings.Current.EngineId;
    }

    private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == Win32.WM_HOTKEY && wParam == HotKeyId)
        {
            _ = Toggle();
            return IntPtr.Zero;
        }

        if (msg == Win32.WM_TRAY && (lParam & 0xFFFF) == 0x0205)
        {
            ShowMenu();
            return IntPtr.Zero;
        }

        return Win32.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private static async Task Toggle()
    {
        if (_dictation is null)
        {
            return;
        }

        ApplySessionSettings();
        try
        {
            await _dictation.ToggleAsync();
        }
        catch
        {
        }
    }

    private static void AddTray()
    {
        _tray = new Win32.NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<Win32.NOTIFYICONDATA>(),
            hWnd = _hwnd,
            uID = 1,
            uFlags = Win32.NIF_MESSAGE | Win32.NIF_ICON | Win32.NIF_TIP,
            uCallbackMessage = Win32.WM_TRAY,
            hIcon = Win32.LoadImage(
                IntPtr.Zero, (IntPtr)Win32.IDI_APPLICATION,
                Win32.IMAGE_ICON, 0, 0, Win32.LR_SHARED),
            szTip = "ZenVoice — Ctrl+Alt+Space"
        };
        Win32.Shell_NotifyIcon(Win32.NIM_ADD, ref _tray);
    }

    private static void ShowMenu()
    {
        var menu = Win32.CreatePopupMenu();
        Win32.AppendMenu(menu, Win32.MF_STRING, (UIntPtr)CmdShow, "Show window");
        Win32.AppendMenu(menu, Win32.MF_STRING, (UIntPtr)CmdDownload, "Download Distil model");
        Win32.AppendMenu(menu, Win32.MF_STRING, (UIntPtr)CmdDelete, "Delete all history");
        var autoFlags = Win32.MF_STRING | (AutoStart.IsEnabled ? MfChecked : 0);
        Win32.AppendMenu(menu, autoFlags, (UIntPtr)CmdAutoStart, "Start with Windows");
        Win32.AppendMenu(menu, MfSeparator, UIntPtr.Zero, "");
        Win32.AppendMenu(menu, Win32.MF_STRING, (UIntPtr)CmdExit, "Exit");
        Win32.GetCursorPos(out var pt);
        Win32.SetForegroundWindow(_hwnd);
        var cmd = Win32.TrackPopupMenu(
            menu, Win32.TPM_RETURNCMD, pt.X, pt.Y, 0, _hwnd, IntPtr.Zero);
        Win32.DestroyMenu(menu);
        if (cmd == CmdExit)
        {
            Win32.PostQuitMessage(0);
        }
        else if (cmd == CmdDelete)
        {
            _vault?.DeleteAll();
            _hud?.Show(DictationPhase.Idle, "History deleted");
        }
        else if (cmd == CmdDownload)
        {
            _ = DownloadDistil();
        }
        else if (cmd == CmdAutoStart)
        {
            AutoStart.Toggle();
            _hud?.Show(DictationPhase.Idle, AutoStart.IsEnabled ? "Starts with Windows" : "Won't start with Windows");
        }
        else if (cmd == CmdShow)
        {
            _hud?.Reveal();
        }
    }

    private static async Task DownloadDistil() => await DownloadModel(EngineIds.WhisperDistilLargeV3);

    private static async Task DownloadModel(string id)
    {
        var model = ModelCatalog.Find(id);
        if (model is null)
        {
            return;
        }

        AppSettings.Current.EngineId = model.Id;
        AppSettings.Current.Save();
        ApplySessionSettings();
        var dest = Path.Combine(AppSettings.ModelsDir, model.Filename);
        if (File.Exists(dest))
        {
            _hud?.Show(DictationPhase.Idle, "Using " + model.DisplayName);
            return;
        }

        try
        {
            _hud?.Show(DictationPhase.Transcribing, "Downloading " + model.DisplayName);
            var progress = new Progress<double>(p =>
                _hud?.Show(DictationPhase.Transcribing, $"{model.DisplayName} {(int)(p * 100)}%"));
            await new ModelDownloader().DownloadAsync(model, AppSettings.ModelsDir, progress);
            _hud?.Show(DictationPhase.Success, model.DisplayName + " ready");
        }
        catch (Exception ex)
        {
            _hud?.Show(DictationPhase.Error, ex.Message);
        }
    }
}

using System.Runtime.InteropServices;

namespace ZenVoice.App;

internal sealed class HoldToDictate : IDisposable
{
    private const int WhKeyboardLl = 13;
    private const int WmKeyDown = 0x0100;
    private const int WmKeyUp = 0x0101;
    private const int WmSysKeyDown = 0x0104;
    private const int WmSysKeyUp = 0x0105;
    private const uint VkRControl = 0xA3;
    private const uint LlkhfInjected = 0x10;

    private readonly Func<bool> _isListening;
    private readonly Func<Task> _toggle;
    private readonly LowLevelKeyboardProc _proc;
    private IntPtr _hook;
    private bool _down;

    public HoldToDictate(Func<bool> isListening, Func<Task> toggle)
    {
        _isListening = isListening;
        _toggle = toggle;
        _proc = Hook;
    }

    public void Start()
    {
        if (_hook != IntPtr.Zero)
        {
            return;
        }

        _hook = SetWindowsHookEx(WhKeyboardLl, _proc, GetModuleHandle(null), 0);
    }

    public void Stop()
    {
        if (_hook == IntPtr.Zero)
        {
            return;
        }

        UnhookWindowsHookEx(_hook);
        _hook = IntPtr.Zero;
        _down = false;
    }

    public void Dispose()
    {
        Stop();
    }

    private IntPtr Hook(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var info = Marshal.PtrToStructure<KbdLlHook>(lParam);
            if (info.vkCode == VkRControl && (info.flags & LlkhfInjected) == 0)
            {
                var msg = (int)wParam;
                if (msg == WmKeyDown || msg == WmSysKeyDown)
                {
                    if (!_down)
                    {
                        _down = true;
                        if (!_isListening())
                        {
                            _ = _toggle();
                        }
                    }

                    return (IntPtr)1;
                }

                if (msg == WmKeyUp || msg == WmSysKeyUp)
                {
                    _down = false;
                    if (_isListening())
                    {
                        _ = _toggle();
                    }

                    return (IntPtr)1;
                }
            }
        }

        return CallNextHookEx(_hook, nCode, wParam, lParam);
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct KbdLlHook
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);
}

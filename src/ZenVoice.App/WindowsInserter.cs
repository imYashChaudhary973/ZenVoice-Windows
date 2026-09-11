using System.Runtime.InteropServices;
using ZenVoice.Core;

namespace ZenVoice.App;

internal sealed class WindowsInserter : ITextInserter
{
    public InsertResult Insert(string text)
    {
        if (!OperatingSystem.IsWindows())
        {
            return InsertResult.CopiedOnly;
        }

        SetClipboard(text);
        var focus = FocusedControl();
        if (focus != IntPtr.Zero && IsPassword(focus))
        {
            return InsertResult.BlockedBySecureInput;
        }

        if (TypeUnicode(text) > 0)
        {
            return InsertResult.Pasted;
        }

        return SendCtrlV() ? InsertResult.Pasted : InsertResult.CopiedOnly;
    }

    private static IntPtr FocusedControl()
    {
        var info = new Win32.GUITHREADINFO { cbSize = Marshal.SizeOf<Win32.GUITHREADINFO>() };
        var fg = Win32.GetForegroundWindow();
        var thread = Win32.GetWindowThreadProcessId(fg, out _);
        return Win32.GetGUIThreadInfo(thread, ref info) ? info.hwndFocus : IntPtr.Zero;
    }

    private static bool IsPassword(IntPtr hwnd)
    {
        var buf = new char[64];
        Win32.GetClassName(hwnd, buf, buf.Length);
        var cls = new string(buf).TrimEnd('\0');
        if (cls.Equals("Edit", StringComparison.OrdinalIgnoreCase)
            && (Win32.GetWindowLong(hwnd, Win32.GWL_STYLE) & Win32.ES_PASSWORD) != 0)
        {
            return true;
        }

        return cls.Contains("Password", StringComparison.OrdinalIgnoreCase);
    }

    private static void SetClipboard(string text)
    {
        if (!Win32.OpenClipboard(IntPtr.Zero))
        {
            return;
        }

        try
        {
            Win32.EmptyClipboard();
            var bytes = (text.Length + 1) * 2;
            var h = Win32.GlobalAlloc(Win32.GMEM_MOVEABLE, (UIntPtr)bytes);
            var ptr = Win32.GlobalLock(h);
            Marshal.Copy(text.ToCharArray(), 0, ptr, text.Length);
            Marshal.WriteInt16(ptr, text.Length * 2, 0);
            Win32.GlobalUnlock(h);
            Win32.SetClipboardData(Win32.CF_UNICODETEXT, h);
        }
        finally
        {
            Win32.CloseClipboard();
        }
    }

    private static uint TypeUnicode(string text)
    {
        var inputs = new Win32.INPUT[text.Length * 2];
        var i = 0;
        foreach (var ch in text)
        {
            inputs[i++] = Unicode(ch, 0);
            inputs[i++] = Unicode(ch, Win32.KEYEVENTF_KEYUP);
        }

        return Win32.SendInput(
            (uint)inputs.Length, inputs, Marshal.SizeOf<Win32.INPUT>());
    }

    private static bool SendCtrlV()
    {
        var inputs = new[]
        {
            Vk(Win32.VK_CONTROL, 0),
            Vk(Win32.VK_V, 0),
            Vk(Win32.VK_V, Win32.KEYEVENTF_KEYUP),
            Vk(Win32.VK_CONTROL, Win32.KEYEVENTF_KEYUP)
        };
        return Win32.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Win32.INPUT>()) > 0;
    }

    private static Win32.INPUT Unicode(char ch, uint extraFlags) => new()
    {
        type = Win32.INPUT_KEYBOARD,
        U = new Win32.InputUnion
        {
            ki = new Win32.KEYBDINPUT
            {
                wScan = ch,
                dwFlags = Win32.KEYEVENTF_UNICODE | extraFlags
            }
        }
    };

    private static Win32.INPUT Vk(byte vk, uint flags) => new()
    {
        type = Win32.INPUT_KEYBOARD,
        U = new Win32.InputUnion
        {
            ki = new Win32.KEYBDINPUT { wVk = vk, dwFlags = flags }
        }
    };
}

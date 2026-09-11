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

        if (SendCtrlV())
        {
            return InsertResult.Pasted;
        }

        return SetViaUia(text);
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

    private static InsertResult SetViaUia(string text)
    {
        try
        {
            var type = Type.GetTypeFromCLSID(new Guid("ff48dba4-60ef-4201-aa87-54103eef594e"), throwOnError: false);
            if (type is null || Activator.CreateInstance(type) is not IUIAutomation uia)
            {
                return InsertResult.CopiedOnly;
            }

            var el = uia.GetFocusedElement();
            if (el is null)
            {
                return InsertResult.CopiedOnly;
            }

            try
            {
                if (el.get_CurrentIsPassword() != 0)
                {
                    return InsertResult.BlockedBySecureInput;
                }
            }
            catch
            {
            }

            if (el.GetCurrentPattern(10002) is not IUIAutomationValuePattern value
                || value.get_CurrentIsReadOnly() != 0)
            {
                return InsertResult.CopiedOnly;
            }

            var next = text;
            try
            {
                var existing = value.get_CurrentValue();
                if (!string.IsNullOrEmpty(existing))
                {
                    next = existing + text;
                }
            }
            catch
            {
            }

            value.SetValue(next);
            return InsertResult.Pasted;
        }
        catch
        {
            return InsertResult.CopiedOnly;
        }
    }
}

[ComImport]
[Guid("30cbe57d-d9d0-452a-ab13-7ac5ac4825ee")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
file interface IUIAutomation
{
    void _1();
    void _2();
    void _3();
    void _4();
    void _5();
    IUIAutomationElement GetFocusedElement();
}

[ComImport]
[Guid("d22108aa-8ac5-49a5-837b-37bbb3d7591e")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
file interface IUIAutomationElement
{
    void _1(); void _2(); void _3(); void _4(); void _5(); void _6(); void _7();
    void _8(); void _9(); void _10(); void _11(); void _12(); void _13();
    [return: MarshalAs(UnmanagedType.IUnknown)]
    object GetCurrentPattern(int patternId);
    void _15(); void _16(); void _17(); void _18(); void _19(); void _20();
    void _21(); void _22(); void _23(); void _24(); void _25(); void _26();
    void _27(); void _28(); void _29(); void _30(); void _31(); void _32();
    int get_CurrentIsPassword();
}

[ComImport]
[Guid("a94cd8b1-0844-4cd6-9d2d-640537ab39e9")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
file interface IUIAutomationValuePattern
{
    void SetValue([MarshalAs(UnmanagedType.BStr)] string value);
    [return: MarshalAs(UnmanagedType.BStr)]
    string get_CurrentValue();
    int get_CurrentIsReadOnly();
}

namespace ZenVoice.App.Ui;

internal static class NativeUi
{
    public static IntPtr Font = IntPtr.Zero;
    public static IntPtr FontTitle = IntPtr.Zero;

    public static void Init()
    {
        if (Font != IntPtr.Zero)
        {
            return;
        }

        Font = Win32.CreateFontW(16, 0, 0, 0, 400, 0, 0, 0, 1, 0, 0, 5, 0, "Segoe UI");
        FontTitle = Win32.CreateFontW(22, 0, 0, 0, 700, 0, 0, 0, 1, 0, 0, 5, 0, "Segoe UI");
    }

    public static IntPtr Label(IntPtr parent, int id, string text, int x, int y, int w, int h) =>
        Child("STATIC", text, 0, parent, id, x, y, w, h);

    public static IntPtr Button(IntPtr parent, int id, string text, int x, int y, int w, int h) =>
        Child("BUTTON", text, Win32.BS_PUSHBUTTON | Win32.WS_TABSTOP, parent, id, x, y, w, h);

    public static IntPtr Check(IntPtr parent, int id, string text, int x, int y, int w, int h) =>
        Child("BUTTON", text, Win32.BS_AUTOCHECKBOX | Win32.WS_TABSTOP, parent, id, x, y, w, h);

    public static IntPtr Radio(IntPtr parent, int id, string text, int x, int y, int w, int h) =>
        Child("BUTTON", text, Win32.BS_AUTORADIOBUTTON | Win32.WS_TABSTOP, parent, id, x, y, w, h);

    public static IntPtr Edit(IntPtr parent, int id, string text, int x, int y, int w, int h, bool multiline = false)
    {
        var style = Win32.WS_BORDER | Win32.WS_TABSTOP | Win32.ES_AUTOHSCROLL | Win32.ES_LEFT;
        if (multiline)
        {
            style |= Win32.ES_MULTILINE | Win32.ES_AUTOVSCROLL | Win32.WS_VSCROLL;
        }

        return Child("EDIT", text, style, parent, id, x, y, w, h);
    }

    public static IntPtr List(IntPtr parent, int id, int x, int y, int w, int h) =>
        Child("LISTBOX", "", Win32.LBS_NOTIFY | Win32.LBS_NOINTEGRALHEIGHT | Win32.WS_VSCROLL | Win32.WS_BORDER | Win32.WS_TABSTOP,
            parent, id, x, y, w, h);

    public static IntPtr Child(string cls, string text, int style, IntPtr parent, int id, int x, int y, int w, int h)
    {
        var hwnd = Win32.CreateWindowExW(
            0, cls, text,
            Win32.WS_CHILD | Win32.WS_VISIBLE | style,
            x, y, w, h, parent, (IntPtr)id, Win32.GetModuleHandle(null), IntPtr.Zero);
        if (hwnd != IntPtr.Zero && Font != IntPtr.Zero)
        {
            Win32.SendMessage(hwnd, Win32.WM_SETFONT, Font, 1);
        }

        return hwnd;
    }

    public static void Move(IntPtr hwnd, int x, int y, int w, int h)
    {
        if (hwnd != IntPtr.Zero)
        {
            Win32.MoveWindow(hwnd, x, y, w, h, true);
        }
    }

    public static void Destroy(ref IntPtr hwnd)
    {
        if (hwnd != IntPtr.Zero)
        {
            Win32.DestroyWindow(hwnd);
            hwnd = IntPtr.Zero;
        }
    }

    public static string Text(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
        {
            return "";
        }

        var n = Win32.GetWindowTextLength(hwnd);
        var buf = new char[n + 1];
        Win32.GetWindowTextW(hwnd, buf, buf.Length);
        return new string(buf, 0, n);
    }

    public static void Text(IntPtr hwnd, string value)
    {
        if (hwnd != IntPtr.Zero)
        {
            Win32.SetWindowTextW(hwnd, value);
        }
    }

    public static bool Checked(IntPtr hwnd) =>
        hwnd != IntPtr.Zero && Win32.SendMessage(hwnd, Win32.BM_GETCHECK, IntPtr.Zero, IntPtr.Zero) == (IntPtr)Win32.BST_CHECKED;

    public static void Checked(IntPtr hwnd, bool on)
    {
        if (hwnd != IntPtr.Zero)
        {
            Win32.SendMessage(hwnd, Win32.BM_SETCHECK, on ? Win32.BST_CHECKED : Win32.BST_UNCHECKED, IntPtr.Zero);
        }
    }
}

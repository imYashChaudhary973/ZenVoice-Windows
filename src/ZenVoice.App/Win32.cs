using System.Runtime.InteropServices;

namespace ZenVoice.App;

internal static class Win32
{
    public const int WM_HOTKEY = 0x0312;
    public const int WM_PAINT = 0x000F;
    public const int WM_DESTROY = 0x0002;
    public const int WM_USER = 0x0400;
    public const int WM_TRAY = WM_USER + 1;
    public const int WM_COMMAND = 0x0111;
    public const int WM_QUIT = 0x0012;
    public const int WM_TIMER = 0x0113;
    public const int WM_SIZE = 0x0005;
    public const int WM_CLOSE = 0x0010;
    public const int WM_LBUTTONDOWN = 0x0201;
    public const int WM_SETFONT = 0x0030;
    public const int WM_CTLCOLORSTATIC = 0x0138;
    public const int WM_CTLCOLORBTN = 0x0135;
    public const int WM_CTLCOLOREDIT = 0x0133;
    public const int WM_GETTEXT = 0x000D;
    public const int WM_GETTEXTLENGTH = 0x000E;

    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint VK_SPACE = 0x20;

    public const uint CF_UNICODETEXT = 13;
    public const uint KEYEVENTF_KEYUP = 0x0002;
    public const byte VK_CONTROL = 0x11;
    public const byte VK_V = 0x56;

    public const int GWL_STYLE = -16;
    public const int ES_PASSWORD = 0x0020;
    public const int WS_POPUP = unchecked((int)0x80000000);
    public const int WS_VISIBLE = 0x10000000;
    public const int WS_CHILD = 0x40000000;
    public const int WS_OVERLAPPEDWINDOW = 0x00CF0000;
    public const int WS_VSCROLL = 0x00200000;
    public const int WS_BORDER = 0x00800000;
    public const int WS_TABSTOP = 0x00010000;
    public const int WS_EX_TOPMOST = 0x00000008;
    public const int WS_EX_TOOLWINDOW = 0x00000080;
    public const int WS_EX_LAYERED = 0x00080000;
    public const int WS_EX_NOACTIVATE = 0x08000000;
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_SHOWWINDOW = 0x0040;
    public const int HWND_TOPMOST = -1;
    public const int HWND_TOP = 0;
    public const int SW_HIDE = 0;
    public const int SW_SHOW = 5;
    public const int SW_RESTORE = 9;
    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOMOVE = 0x0002;
    public const int IDC_ARROW = 32512;
    public const uint DT_LEFT = 0x00000000;
    public const uint DT_TOP = 0x00000000;
    public const uint DT_WORDBREAK = 0x00000010;
    public const int BN_CLICKED = 0;
    public const int IdDownload = 1001;
    public const int BS_PUSHBUTTON = 0x00000000;
    public const int BS_AUTOCHECKBOX = 0x00000003;
    public const int BS_AUTORADIOBUTTON = 0x00000009;
    public const int ES_LEFT = 0x0000;
    public const int ES_MULTILINE = 0x0004;
    public const int ES_AUTOVSCROLL = 0x0040;
    public const int ES_AUTOHSCROLL = 0x0080;
    public const int ES_READONLY = 0x0800;
    public const int BM_GETCHECK = 0x00F0;
    public const int BM_SETCHECK = 0x00F1;
    public const int BST_UNCHECKED = 0;
    public const int BST_CHECKED = 1;
    public const int LBN_SELCHANGE = 1;
    public const int LB_ADDSTRING = 0x0180;
    public const int LB_GETCURSEL = 0x0188;
    public const int LB_GETTEXT = 0x0189;
    public const int LB_RESETCONTENT = 0x0184;
    public const int LBS_NOTIFY = 0x0001;
    public const int LBS_NOINTEGRALHEIGHT = 0x0100;
    public const uint NIM_ADD = 0x00000000;
    public const uint NIM_DELETE = 0x00000002;
    public const uint NIF_MESSAGE = 0x00000001;
    public const uint NIF_ICON = 0x00000002;
    public const uint NIF_TIP = 0x00000004;
    public const uint MF_STRING = 0x00000000;
    public const uint TPM_RETURNCMD = 0x0100;
    public const int IDI_APPLICATION = 32512;
    public const uint IMAGE_ICON = 1;
    public const uint LR_SHARED = 0x8000;
    public const uint WAVE_MAPPER = 0xFFFFFFFF;
    public const uint CALLBACK_FUNCTION = 0x00030000;
    public const uint WHDR_DONE = 0x00000001;

    public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    public delegate void WaveInProc(IntPtr hwi, uint uMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern ushort RegisterClassW(ref WNDCLASS lpWndClass);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr CreateWindowExW(
        int dwExStyle, string lpClassName, string lpWindowName, int dwStyle,
        int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu,
        IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool DestroyWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
    [DllImport("user32.dll")] public static extern bool TranslateMessage(ref MSG lpMsg);
    [DllImport("user32.dll")] public static extern IntPtr DispatchMessage(ref MSG lpMsg);
    [DllImport("user32.dll")] public static extern void PostQuitMessage(int nExitCode);
    [DllImport("user32.dll")] public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    [DllImport("user32.dll")] public static extern bool OpenClipboard(IntPtr hWndNewOwner);
    [DllImport("user32.dll")] public static extern bool CloseClipboard();
    [DllImport("user32.dll")] public static extern bool EmptyClipboard();
    [DllImport("user32.dll")] public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetClassName(IntPtr hWnd, char[] lpClassName, int nMaxCount);
    [DllImport("user32.dll")] public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")] public static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO pgui);
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")] public static extern IntPtr BeginPaint(IntPtr hWnd, out PAINTSTRUCT lpPaint);
    [DllImport("user32.dll")] public static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool DrawTextW(IntPtr hdc, string lpchText, int nCount, ref RECT lpRect, uint uFormat);
    [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")] public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);
    [DllImport("user32.dll")] public static extern bool UpdateWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
    [DllImport("user32.dll")] public static extern bool GetCursorPos(out POINT lpPoint);
    [DllImport("user32.dll")] public static extern uint TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindowW(string? lpClassName, string? lpWindowName);
    [DllImport("user32.dll")] public static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);
    [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern IntPtr CreatePopupMenu();
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool AppendMenu(IntPtr hMenu, uint uFlags, UIntPtr uIDNewItem, string lpNewItem);
    [DllImport("user32.dll")] public static extern bool DestroyMenu(IntPtr hMenu);
    [DllImport("user32.dll")] public static extern UIntPtr SetTimer(IntPtr hWnd, UIntPtr nIDEvent, uint uElapse, IntPtr lpTimerFunc);
    [DllImport("user32.dll")] public static extern bool KillTimer(IntPtr hWnd, UIntPtr uIDEvent);
    [DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool SetWindowTextW(IntPtr hWnd, string lpString);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowTextW(IntPtr hWnd, char[] lpString, int nMaxCount);
    [DllImport("user32.dll")] public static extern int GetWindowTextLength(IntPtr hWnd);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);
    [DllImport("user32.dll")] public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);
    [DllImport("user32.dll")] public static extern IntPtr SetFocus(IntPtr hWnd);
    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr CreateFontW(int cHeight, int cWidth, int cEscapement, int cOrientation, int cWeight, uint bItalic, uint bUnderline, uint bStrikeOut, uint iCharSet, uint iOutPrecision, uint iClipPrecision, uint iQuality, uint iPitchAndFamily, string pszFaceName);
    [DllImport("user32.dll")] public static extern int FillRect(IntPtr hDC, ref RECT lprc, IntPtr hbr);
    [DllImport("gdi32.dll")] public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);
    [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr ho);
    [DllImport("gdi32.dll")] public static extern uint SetBkColor(IntPtr hdc, uint color);
    [DllImport("shell32.dll")] public static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);
    [DllImport("user32.dll")]
    public static extern IntPtr LoadImage(IntPtr hInst, IntPtr name, uint type, int cx, int cy, uint fuLoad);
    [DllImport("kernel32.dll")] public static extern IntPtr GetModuleHandle(string? lpModuleName);
    [DllImport("kernel32.dll")] public static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);
    [DllImport("kernel32.dll")] public static extern IntPtr GlobalLock(IntPtr hMem);
    [DllImport("kernel32.dll")] public static extern bool GlobalUnlock(IntPtr hMem);
    [DllImport("gdi32.dll")] public static extern IntPtr CreateSolidBrush(uint color);
    [DllImport("gdi32.dll")] public static extern int SetBkMode(IntPtr hdc, int mode);
    [DllImport("gdi32.dll")] public static extern uint SetTextColor(IntPtr hdc, uint color);
    [DllImport("winmm.dll")] public static extern int waveInOpen(out IntPtr phwi, uint uDeviceID, ref WAVEFORMATEX pwfx, WaveInProc dwCallback, IntPtr dwInstance, uint fdwOpen);
    [DllImport("winmm.dll")] public static extern int waveInPrepareHeader(IntPtr hwi, IntPtr lpHdr, int uSize);
    [DllImport("winmm.dll")] public static extern int waveInAddBuffer(IntPtr hwi, IntPtr lpHdr, int uSize);
    [DllImport("winmm.dll")] public static extern int waveInStart(IntPtr hwi);
    [DllImport("winmm.dll")] public static extern int waveInStop(IntPtr hwi);
    [DllImport("winmm.dll")] public static extern int waveInReset(IntPtr hwi);
    [DllImport("winmm.dll")] public static extern int waveInUnprepareHeader(IntPtr hwi, IntPtr lpHdr, int uSize);
    [DllImport("winmm.dll")] public static extern int waveInClose(IntPtr hwi);

    public const uint GMEM_MOVEABLE = 0x0002;
    public const uint LWA_ALPHA = 0x00000002;
    public const uint DT_SINGLELINE = 0x00000020;
    public const uint DT_VCENTER = 0x00000004;
    public const uint DT_CENTER = 0x00000001;
    public const int TRANSPARENT = 1;
    public const int WIM_DATA = 0x3C0;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASS
    {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string? lpszMenuName;
        public string lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT { public int X; public int Y; }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }

    [StructLayout(LayoutKind.Sequential)]
    public struct PAINTSTRUCT
    {
        public IntPtr hdc;
        public bool fErase;
        public RECT rcPaint;
        public bool fRestore;
        public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GUITHREADINFO
    {
        public int cbSize;
        public uint flags;
        public IntPtr hwndActive;
        public IntPtr hwndFocus;
        public IntPtr hwndCapture;
        public IntPtr hwndMenuOwner;
        public IntPtr hwndMoveSize;
        public IntPtr hwndCaret;
        public RECT rcCaret;
    }

    public const uint KEYEVENTF_UNICODE = 0x0004;
    public const uint INPUT_KEYBOARD = 1;

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WAVEFORMATEX
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WAVEHDR
    {
        public IntPtr lpData;
        public uint dwBufferLength;
        public uint dwBytesRecorded;
        public IntPtr dwUser;
        public uint dwFlags;
        public uint dwLoops;
        public IntPtr lpNext;
        public IntPtr reserved;
    }
}

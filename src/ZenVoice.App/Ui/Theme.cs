namespace ZenVoice.App.Ui;

internal static class Theme
{
    public const int SidebarWidth = 248;
    public const int HeaderHeight = 56;
    public const int NavRow = 44;

    public const uint Canvas = 0x00333030;
    public const uint Surface = 0x003B3939;
    public const uint Raised = 0x00484344;
    public const uint Text = 0x00FFFEFE;
    public const uint Secondary = 0x00BBB9B9;
    public const uint AccentFill = 0x00F53E54;

    public static IntPtr CanvasBrush;
    public static IntPtr SurfaceBrush;
    public static IntPtr RaisedBrush;
    public static IntPtr AccentBrush;

    public static void Init()
    {
        if (CanvasBrush != IntPtr.Zero)
        {
            return;
        }

        CanvasBrush = Win32.CreateSolidBrush(Canvas);
        SurfaceBrush = Win32.CreateSolidBrush(Surface);
        RaisedBrush = Win32.CreateSolidBrush(Raised);
        AccentBrush = Win32.CreateSolidBrush(AccentFill);
    }
}

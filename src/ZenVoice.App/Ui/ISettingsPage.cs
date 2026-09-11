namespace ZenVoice.App.Ui;

internal interface ISettingsPage
{
    string Id { get; }
    string Title { get; }
    string Subtitle { get; }
    void Mount(IntPtr parent);
    void Unmount();
    void Layout(Win32.RECT bounds);
    void Refresh();
    bool HandleCommand(int id);
}

internal static class UiIds
{
    public const int NavHome = 1101;
    public const int NavModels = 1102;
    public const int NavHistory = 1103;
    public const int NavFormatting = 1104;
    public const int NavShortcuts = 1105;
    public const int NavPrivacy = 1106;

    public const int Models = 2000;
    public const int History = 2100;
    public const int Formatting = 2200;
    public const int Shortcuts = 2300;
    public const int Privacy = 2400;
    public const int Home = 2500;
}

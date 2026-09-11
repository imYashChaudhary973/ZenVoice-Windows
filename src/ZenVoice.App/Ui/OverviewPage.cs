using ZenVoice.Core;

namespace ZenVoice.App.Ui;

internal sealed class OverviewPage : ISettingsPage
{
    public string Id => "home";
    public string Title => "Home";
    public string Subtitle => "Speak. It types. Local by default.";

    private IntPtr _body;

    public void Mount(IntPtr parent)
    {
        var engine = AppSettings.Current.EngineId;
        var model = ModelCatalog.Find(engine);
        var ready = model is not null && File.Exists(Path.Combine(AppSettings.ModelsDir, model.Filename));
        _body = NativeUi.Label(
            parent, UiIds.Home + 1,
            $"Engine: {engine}\n" +
            (ready ? "Model file is on disk." : "Model missing — open Models and download it.") +
            "\n\nCtrl+Alt+Space starts and stops.\nKeep the target field focused.\nClosing this window does not quit — use the tray to Exit.",
            0, 0, 400, 200);
    }

    public void Unmount() => NativeUi.Destroy(ref _body);

    public void Layout(Win32.RECT bounds) =>
        NativeUi.Move(_body, bounds.Left, bounds.Top, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top);

    public void Refresh()
    {
    }

    public bool HandleCommand(int id) => false;
}

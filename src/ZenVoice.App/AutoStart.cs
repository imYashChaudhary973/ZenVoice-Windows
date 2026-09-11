using Microsoft.Win32;

namespace ZenVoice.App;

internal static class AutoStart
{
    private const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "ZenVoice";

    public static string ExePath =>
        Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "ZenVoice.exe");

    public static bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
            return key?.GetValue(ValueName) is string s
                && s.Contains("ZenVoice", StringComparison.OrdinalIgnoreCase);
        }
    }

    public static void Enable()
    {
        using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
        key.SetValue(ValueName, $"\"{ExePath}\"");
    }

    public static void Disable()
    {
        using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
        key.DeleteValue(ValueName, false);
    }

    public static void Toggle()
    {
        if (IsEnabled)
        {
            Disable();
        }
        else
        {
            Enable();
        }
    }
}

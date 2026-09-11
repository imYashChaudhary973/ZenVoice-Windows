using System.Runtime.InteropServices;
using System.Text;

namespace ZenVoice.App;

internal static class CloudKeyStore
{
    public static void Set(string engineId, string key)
    {
        var bytes = Encoding.UTF8.GetBytes(key);
        var cred = new CREDENTIAL
        {
            Type = 1,
            TargetName = Target(engineId),
            CredentialBlobSize = bytes.Length,
            CredentialBlob = Marshal.AllocHGlobal(bytes.Length),
            Persist = 2,
            UserName = "ZenVoice"
        };
        Marshal.Copy(bytes, 0, cred.CredentialBlob, bytes.Length);
        CredWriteW(ref cred, 0);
        Marshal.FreeHGlobal(cred.CredentialBlob);
    }

    public static string? Get(string engineId)
    {
        if (!CredReadW(Target(engineId), 1, 0, out var ptr))
        {
            return null;
        }

        var cred = Marshal.PtrToStructure<CREDENTIAL>(ptr);
        var bytes = new byte[cred.CredentialBlobSize];
        Marshal.Copy(cred.CredentialBlob, bytes, 0, bytes.Length);
        CredFree(ptr);
        return Encoding.UTF8.GetString(bytes);
    }

    public static void Delete(string engineId) => CredDelete(Target(engineId), 1, 0);

    private static string Target(string engineId) => $"ZenVoice/cloud/{engineId}";

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWriteW(ref CREDENTIAL userCredential, uint flags);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredReadW(string target, uint type, uint flags, out IntPtr credential);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredDelete(string target, uint type, uint flags);

    [DllImport("advapi32.dll")]
    private static extern void CredFree(IntPtr buffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CREDENTIAL
    {
        public uint Flags;
        public uint Type;
        public string TargetName;
        public string? Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public int CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public string? TargetAlias;
        public string? UserName;
    }
}

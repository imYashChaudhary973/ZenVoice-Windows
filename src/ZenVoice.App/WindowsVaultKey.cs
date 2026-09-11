using System.Runtime.InteropServices;
using System.Security.Cryptography;
using ZenVoice.Core;

namespace ZenVoice.App;

internal sealed class WindowsVaultKey : IVaultKey
{
    private const string Target = "ZenVoice/transcript-key";

    public byte[] LoadOrCreate()
    {
        if (CredRead(out var existing))
        {
            return existing;
        }

        var key = RandomNumberGenerator.GetBytes(32);
        CredWrite(key);
        return key;
    }

    public void Delete()
    {
        CredDelete(Target, 1, 0);
        var key = RandomNumberGenerator.GetBytes(32);
        CredWrite(key);
    }

    private static void CredWrite(byte[] key)
    {
        var blob = Marshal.AllocHGlobal(key.Length);
        Marshal.Copy(key, 0, blob, key.Length);
        var cred = new CREDENTIAL
        {
            Type = 1,
            TargetName = Target,
            CredentialBlobSize = (uint)key.Length,
            CredentialBlob = blob,
            Persist = 2
        };
        try
        {
            if (!CredWriteW(ref cred, 0))
            {
                throw new InvalidOperationException("Credential Manager write failed");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(blob);
        }
    }

    private static bool CredRead(out byte[] key)
    {
        key = [];
        if (!CredReadW(Target, 1, 0, out var ptr))
        {
            return false;
        }

        try
        {
            var cred = Marshal.PtrToStructure<CREDENTIAL>(ptr);
            key = new byte[cred.CredentialBlobSize];
            Marshal.Copy(cred.CredentialBlob, key, 0, key.Length);
            return key.Length == 32;
        }
        finally
        {
            CredFree(ptr);
        }
    }

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
        public long LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public string? TargetAlias;
        public string? UserName;
    }
}

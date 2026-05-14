using System.Runtime.InteropServices;
using System.Text;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Infrastructure.Services;

public class WindowsCredentialService : ICredentialService
{
    public string? GetPassword(string target)
    {
        if (!NativeMethods.CredRead(target, NativeMethods.CRED_TYPE_GENERIC, 0, out var credentialPtr))
            return null;

        try
        {
            var credential = Marshal.PtrToStructure<NativeMethods.CREDENTIAL>(credentialPtr);
            if (credential.CredentialBlobSize == 0)
                return null;
            return Encoding.Unicode.GetString(GetBytes(credential.CredentialBlob, credential.CredentialBlobSize));
        }
        finally
        {
            NativeMethods.CredFree(credentialPtr);
        }
    }

    public void SavePassword(string target, string username, string password)
    {
        var passwordBytes = Encoding.Unicode.GetBytes(password);
        var credential = new NativeMethods.CREDENTIAL
        {
            Type = NativeMethods.CRED_TYPE_GENERIC,
            TargetName = target,
            UserName = username,
            CredentialBlobSize = passwordBytes.Length,
            CredentialBlob = Marshal.AllocHGlobal(passwordBytes.Length),
            Persist = NativeMethods.CRED_PERSIST_LOCAL_MACHINE
        };
        Marshal.Copy(passwordBytes, 0, credential.CredentialBlob, passwordBytes.Length);
        try
        {
            NativeMethods.CredWrite(ref credential, 0);
        }
        finally
        {
            Marshal.FreeHGlobal(credential.CredentialBlob);
        }
    }

    public void DeletePassword(string target)
    {
        NativeMethods.CredDelete(target, NativeMethods.CRED_TYPE_GENERIC, 0);
    }

    private static byte[] GetBytes(IntPtr ptr, int size)
    {
        var bytes = new byte[size];
        Marshal.Copy(ptr, bytes, 0, size);
        return bytes;
    }

    private static class NativeMethods
    {
        public const int CRED_TYPE_GENERIC = 1;
        public const int CRED_PERSIST_LOCAL_MACHINE = 2;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CREDENTIAL
        {
            public int Flags;
            public int Type;
            public string TargetName;
            public string Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public int CredentialBlobSize;
            public IntPtr CredentialBlob;
            public int Persist;
            public int AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CredRead(string target, int type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CredDelete(string target, int type, int flags);

        [DllImport("advapi32.dll")]
        public static extern void CredFree([In] IntPtr buffer);
    }
}

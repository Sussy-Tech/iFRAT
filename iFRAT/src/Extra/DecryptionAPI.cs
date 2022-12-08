using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace iFRAT.Extras.Password;
public sealed partial class DecryptionAPI
{
    public enum KeyType { UserKey = 1, MachineKey }

    private const KeyType defaultKeyType = KeyType.UserKey;
    static private readonly IntPtr NullPtr = ((IntPtr)((int)(0)));
    private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;
    private const int CRYPTPROTECT_LOCAL_MACHINE = 0x4;
    internal partial class Unmanaged
    {
        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool CryptProtectData(ref DATA_BLOB pPlainText, string szDescription, ref DATA_BLOB pEntropy, IntPtr pReserved, ref CRYPTPROTECT_PROMPTSTRUCT pPrompt, int dwFlags, ref DATA_BLOB pCipherText);

        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool CryptUnprotectData(ref DATA_BLOB pCipherText, ref string pszDescription, ref DATA_BLOB pEntropy, IntPtr pReserved, ref CRYPTPROTECT_PROMPTSTRUCT pPrompt, int dwFlags, ref DATA_BLOB pPlainText);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DATA_BLOB : IDisposable
        {
            public int cbData;
            public IntPtr pbData;

            public void Dispose()
            {
                if (pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(pbData);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CRYPTPROTECT_PROMPTSTRUCT : IDisposable
        {
            public int cbSize;
            public int dwPromptFlags;
            public IntPtr hwndApp;
            public string szPrompt;

            public void Dispose()
            {
                if (hwndApp != IntPtr.Zero)
                    Marshal.FreeHGlobal(hwndApp);
            }
        }
    }

    private static void InitPrompt(ref Unmanaged.CRYPTPROTECT_PROMPTSTRUCT ps)
    {
        ps.cbSize = Marshal.SizeOf(typeof(Unmanaged.CRYPTPROTECT_PROMPTSTRUCT));
        ps.dwPromptFlags = 0;
        ps.hwndApp = NullPtr;
        ps.szPrompt = null;
    }
    private static void InitBLOB(byte[] data, ref Unmanaged.DATA_BLOB blob)
    {
        // Use empty array for null parameter.
        data ??= Array.Empty<byte>();

        // Allocate memory for the BLOB data.
        blob.pbData = Marshal.AllocHGlobal(data.Length);

        // Make sure that memory allocation was successful.
        if (blob.pbData == IntPtr.Zero)
            throw new Exception("Unable to allocate data buffer for BLOB structure.");

        // Specify number of bytes in the BLOB.
        blob.cbData = data.Length;

        // Copy data from original source to the BLOB structure.
        Marshal.Copy(data, 0, blob.pbData, data.Length);
    }

    public static string Encrypt(string plainText)
    {
        return Encrypt(defaultKeyType, plainText, String.Empty, String.Empty);
    }

    public static string Encrypt(KeyType keyType, string plainText)
    {
        return Encrypt(keyType, plainText, String.Empty,
                        String.Empty);
    }

    public static string Encrypt(KeyType keyType, string plainText, string entropy)
    {
        return Encrypt(keyType, plainText, entropy, String.Empty);
    }

    public static string Encrypt(KeyType keyType, string plainText, string entropy, string description)
    {
        // Make sure that parameters are valid.
        plainText ??= String.Empty;
        entropy ??= String.Empty;

        // Call encryption routine and convert returned bytes into
        // a base64-encoded value.
        return Convert.ToBase64String(
                Encrypt(keyType,
                        Encoding.UTF8.GetBytes(plainText),
                        Encoding.UTF8.GetBytes(entropy),
                        description));
    }
    public static byte[] Encrypt(KeyType keyType, byte[] plainTextBytes, byte[] entropyBytes, string description)
    {
        // Make sure that parameters are valid.
        plainTextBytes ??= Array.Empty<byte>();
        entropyBytes ??= Array.Empty<byte>();
        description ??= string.Empty;

        // Create BLOBs to hold data.
        Unmanaged.DATA_BLOB plainTextBlob = new();
        Unmanaged.DATA_BLOB cipherTextBlob = new();
        Unmanaged.DATA_BLOB entropyBlob = new();

        // We only need prompt structure because it is a required
        // parameter.
        Unmanaged.CRYPTPROTECT_PROMPTSTRUCT prompt = new();
        InitPrompt(ref prompt);

        try
        {
            // Convert plaintext bytes into a BLOB structure.
            try
            {
                InitBLOB(plainTextBytes, ref plainTextBlob);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot initialize plaintext BLOB.", ex);
            }

            // Convert entropy bytes into a BLOB structure.
            try
            {
                InitBLOB(entropyBytes, ref entropyBlob);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot initialize entropy BLOB.", ex);
            }

            // Disable any types of UI.
            int flags = CRYPTPROTECT_UI_FORBIDDEN;

            // When using machine-specific key, set up machine flag.
            if (keyType == KeyType.MachineKey)
                flags |= CRYPTPROTECT_LOCAL_MACHINE;

            // Call DPAPI to encrypt data.
            bool success = Unmanaged.CryptProtectData(ref plainTextBlob,
                                                description,
                                            ref entropyBlob,
                                                IntPtr.Zero,
                                            ref prompt,
                                                flags,
                                            ref cipherTextBlob);
            // Check the result.
            if (!success)
            {
                // If operation failed, retrieve last Win32 error.
                int errCode = Marshal.GetLastWin32Error();

                // Win32Exception will contain error message corresponding
                // to the Windows error code.
                throw new Exception("CryptProtectData failed.", new Win32Exception(errCode));
            }

            // Allocate memory to hold ciphertext.
            byte[] cipherTextBytes = new byte[cipherTextBlob.cbData];

            // Copy ciphertext from the BLOB to a byte array.
            Marshal.Copy(cipherTextBlob.pbData,
                            cipherTextBytes,
                            0,
                            cipherTextBlob.cbData);

            // Return the result.
            return cipherTextBytes;
        }
        catch (Exception ex)
        {
            throw new Exception("DPAPI was unable to encrypt data.", ex);
        }
        // Free all memory allocated for BLOBs.
        finally
        {
            plainTextBlob.Dispose();
            cipherTextBlob.Dispose();
            entropyBlob.Dispose();
        }
    }
    public static string Decrypt(string cipherText)
    {
        string description;

        return Decrypt(cipherText, String.Empty, out description);
    }

    public static string Decrypt(string cipherText, out string description)
    {
        return Decrypt(cipherText, String.Empty, out description);
    }

    public static string Decrypt(string cipherText, string entropy, out string description)
    {
        // Make sure that parameters are valid.
        if (entropy == null) entropy = String.Empty;

        return Encoding.UTF8.GetString(
                    Decrypt(Convert.FromBase64String(cipherText),
                                Encoding.UTF8.GetBytes(entropy),
                            out description));
    }

    public static byte[] Decrypt(byte[] cipherTextBytes, byte[] entropyBytes, out string description)
    {
        // Create BLOBs to hold data.
        Unmanaged.DATA_BLOB plainTextBlob = new();
        Unmanaged.DATA_BLOB cipherTextBlob = new();
        Unmanaged.DATA_BLOB entropyBlob = new();

        // We only need prompt structure because it is a required
        // parameter.
        Unmanaged.CRYPTPROTECT_PROMPTSTRUCT prompt = new();
        InitPrompt(ref prompt);

        // Initialize description string.
        description = String.Empty;

        try
        {
            // Convert ciphertext bytes into a BLOB structure.
            try
            {
                InitBLOB(cipherTextBytes, ref cipherTextBlob);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot initialize ciphertext BLOB.", ex);
            }

            // Convert entropy bytes into a BLOB structure.
            try
            {
                InitBLOB(entropyBytes, ref entropyBlob);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot initialize entropy BLOB.", ex);
            }

            // Disable any types of UI. CryptUnprotectData does not
            // mention CRYPTPROTECT_LOCAL_MACHINE flag in the list of
            // supported flags so we will not set it up.
            const int flags = CRYPTPROTECT_UI_FORBIDDEN;

            // Call DPAPI to decrypt data.
            bool success = Unmanaged.CryptUnprotectData(ref cipherTextBlob,
                                              ref description,
                                              ref entropyBlob,
                                                  IntPtr.Zero,
                                              ref prompt,
                                                  flags,
                                              ref plainTextBlob);

            // Check the result.
            if (!success)
            {
                // If operation failed, retrieve last Win32 error.
                int errCode = Marshal.GetLastWin32Error();

                // Win32Exception will contain error message corresponding
                // to the Windows error code.
                throw new Exception("CryptUnprotectData failed.", new Win32Exception(errCode));
            }

            // Allocate memory to hold plaintext.
            byte[] plainTextBytes = new byte[plainTextBlob.cbData];

            // Copy ciphertext from the BLOB to a byte array.
            Marshal.Copy(plainTextBlob.pbData,
                         plainTextBytes,
                         0,
                         plainTextBlob.cbData);

            // Return the result.
            return plainTextBytes;
        }
        catch (Exception ex)
        {
            throw new Exception("DPAPI was unable to decrypt data.", ex);
        }
        // Free all memory allocated for BLOBs.
        finally
        {
            plainTextBlob.Dispose();
            cipherTextBlob.Dispose();
            entropyBlob.Dispose();
        }
    }
}
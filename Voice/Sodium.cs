using System.Runtime.InteropServices;

namespace DiscordUnity
{
    internal unsafe static class SecretBox
    {
        private static class SafeNativeMethods
        {
            [DllImport("libsodium", EntryPoint = "crypto_secretbox_easy", CallingConvention = CallingConvention.Cdecl)]
            public static extern int SecretBoxEasy(byte* output, byte[] input, long inputLength, byte[] nonce, byte[] secret);
            [DllImport("libsodium", EntryPoint = "crypto_secretbox_open_easy", CallingConvention = CallingConvention.Cdecl)]
            public static extern int SecretBoxOpenEasy(byte[] output, byte* input, ulong inputLength, byte[] nonce, byte[] secret);
        }

        public static int Encrypt(byte[] input, long inputLength, byte[] output, byte[] nonce, byte[] secret)
        {
            fixed (byte* outPtr = output)
                return SafeNativeMethods.SecretBoxEasy(outPtr + 12, input, inputLength, nonce, secret);
        }
        public static int Decrypt(byte[] input, ulong inputLength, byte[] output, byte[] nonce, byte[] secret)
        {
            fixed (byte* inPtr = input)
                return SafeNativeMethods.SecretBoxOpenEasy(output, inPtr + 12, inputLength, nonce, secret);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace Lamie.LibVData
{
    /// <summary>
    /// Static class provide methods to decrypt data from .V files
    /// </summary>
    public class Decryptor
    {
        internal const int SecretByte = 0x55;

        /// <summary>
        /// Decrypt the buffer with the secret key.
        /// </summary>
        /// <param name="buffer">The buffer to be decrypted</param>
        public static void DecryptBuffer(byte[] buffer) => DecryptBuffer(buffer, 0, buffer.Length);

        /// <summary>
        /// Decrypt the buffer with the secret key starting from the given offset.
        /// </summary>
        /// <param name="buffer">The buffer to be decrypted</param>
        /// <param name="offset">The start offset of the buffer</param>
        public static void DecryptBuffer(byte[] buffer, int offset) => DecryptBuffer(buffer, offset, buffer.Length);

        /// <summary>
        /// Decrypt the buffer with the secret key starting from the given offset with given length.
        /// </summary>
        /// <param name="buffer">The buffer to be decrypted</param>
        /// <param name="offset">The start offset of the buffer</param>
        /// <param name="length">The maximum number of bytes to decrypt</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="IndexOutOfRangeException" />
        public static void DecryptBuffer(byte[] buffer, int offset, int length)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (buffer.Length == 0) return;
            if (offset >= buffer.Length)
                throw new IndexOutOfRangeException("The offset value should be within the buffer length.");

            int count = (offset + length);
            if (count > buffer.Length)
                count = buffer.Length - offset;
            for (int i = offset; i < count; i++)
                buffer[i] ^= SecretByte;
        }

        /// <summary>
        /// Decrypt the byte with the secret key.
        /// </summary>
        /// <param name="b">The byte to be decrypted</param>
        /// <returns></returns>
        public static byte Decrypt(byte b)
        {
            return (byte)(b ^ SecretByte);
        }

        internal static byte Decrypt(int b)
        {
            return (byte)(b ^ SecretByte);
        }
    }
}

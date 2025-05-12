using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    /// <summary>
    /// Provides method to compress and decompress strings
    /// </summary>
    public static class StringCompression
    {
        public static void CompressStream(Stream input, Stream output)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            using var gzipStream = new GZipStream(output, CompressionMode.Compress, leaveOpen: true);
            input.CopyTo(gzipStream);
            gzipStream.Flush(); // finalize if needed
        }

        public static void DecompressStream(Stream input, Stream output)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            using var gzipStream = new GZipStream(input, CompressionMode.Decompress, leaveOpen: true);
            gzipStream.CopyTo(output);
            output.Flush();
        }

        public static byte[] CompressBytes(byte[] input)
        {
            using var inputStream = new MemoryStream(input);
            using var outputStream = new MemoryStream();
            CompressStream(inputStream, outputStream);
            return outputStream.ToArray();
        }

        public static byte[] DecompressBytes(byte[] input)
        {
            using var inputStream = new MemoryStream(input);
            using var outputStream = new MemoryStream();
            DecompressStream(inputStream, outputStream);
            return outputStream.ToArray();
        }

        public static string CompressString(string input, Encoding? encoding = null)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            encoding ??= Encoding.UTF8;
            var bytes = encoding.GetBytes(input);
            var compressed = CompressBytes(bytes);
            return encoding.GetString(compressed);
        }

        public static string DecompressString(string input, Encoding? encoding = null)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            encoding ??= Encoding.UTF8;
            var compressed = encoding.GetBytes(input);
            var decompressed = DecompressBytes(compressed);
            return encoding.GetString(decompressed);
        }

    }
}

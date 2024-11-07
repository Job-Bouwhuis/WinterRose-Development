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
        /// <summary>
        /// Compresses a string using GZip
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static (string CompressedData, bool WorthIt) GZipCompress(string input)
        {
            if (string.IsNullOrEmpty(input))
                return (string.Empty, false);

            // Convert input string to byte array
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            using (var outputStream = new MemoryStream())
            {
                // Create a new GZipStream for compression
                using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    gzipStream.Write(inputBytes, 0, inputBytes.Length);
                }

                // Convert compressed data to base64 string
                var arr = outputStream.ToArray();
                int len = input.Length;
                return (Convert.ToBase64String(arr), len > arr.Length);
            }
        }

        /// <summary>
        /// Decompresses a string using GZip
        /// </summary>
        /// <param name="compressedInput"></param>
        /// <returns></returns>
        public static string GZipDecompress(string compressedInput)
        {
            if (string.IsNullOrEmpty(compressedInput))
                return string.Empty;

            // Convert base64 string to byte array
            byte[] compressedBytes = Convert.FromBase64String(compressedInput);

            using (var inputStream = new MemoryStream(compressedBytes))
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(gzipStream, Encoding.UTF8))
            {
                // Read decompressed data as string
                return reader.ReadToEnd();
            }
        }
    }
}

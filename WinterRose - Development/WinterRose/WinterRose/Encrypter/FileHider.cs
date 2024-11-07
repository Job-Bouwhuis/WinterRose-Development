using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#pragma warning disable CA1416

namespace WinterRose;

/// <summary>
/// Provides methods to hide files, or strings, inside image files. provided that the image file is large enough to contain the data.
/// </summary>
[Experimental("WR_EXPERIMENTAL")]
public class FileHider : IClearDisposable
{
    private Bitmap bitmap;
    private string imagePath;

    public bool IsDisposed { get; private set; }

    public FileHider(string imagePath)
    {
        if (!File.Exists(imagePath))
            throw new FileNotFoundException("Image file not found.", imagePath);

        this.imagePath = imagePath;
        this.bitmap = new Bitmap(imagePath);
    }

    public void Hide(string text)
    {
        byte[] textBytes = Encoding.UTF8.GetBytes(text);
        Hide(textBytes);
    }

    public void Hide(FileInfo file)
    {
        byte[] fileBytes = File.ReadAllBytes(file.FullName);
        Hide(fileBytes);
    }

    private void Hide(byte[] data)
    {
        // 2555936

        // Get length prefix
        byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(lengthPrefix); // Ensure network byte order (big-endian)

        // Combine length prefix and data
        byte[] dataWithLength = new byte[lengthPrefix.Length + data.Length];
        Array.Copy(lengthPrefix, dataWithLength, lengthPrefix.Length);
        Array.Copy(data, 0, dataWithLength, lengthPrefix.Length, data.Length);

        if (!CanHideData(dataWithLength))
            throw new InvalidOperationException("Data is too large to hide in the image.");

        // Hide the data in the image
        HideDataInBitmap(dataWithLength);

        // Save the result
        SaveResult();
    }

    private void HideDataInBitmap(byte[] data)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;
        int dataBitIndex = 0;
        int totalBits = data.Length * 8;

        for (int y = 0; y < height && dataBitIndex < totalBits; y++)
        {
            for (int x = 0; x < width && dataBitIndex < totalBits; x++)
            {
                Color pixelColor = bitmap.GetPixel(x, y);
                byte r = pixelColor.R;
                byte g = pixelColor.G;
                byte b = pixelColor.B;
                byte a = pixelColor.A;

                // Modify the least significant bits of the color channels
                if (dataBitIndex < totalBits)
                    r = (byte)((r & 0xFE) | GetBit(data, dataBitIndex++));
                if (dataBitIndex < totalBits)
                    g = (byte)((g & 0xFE) | GetBit(data, dataBitIndex++));
                if (dataBitIndex < totalBits)
                    b = (byte)((b & 0xFE) | GetBit(data, dataBitIndex++));

                bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
            }
        }
    }

    private void SaveResult()
    {
        string fullPath = Path.GetFullPath(imagePath);
        bitmap.Save(Path.GetFileNameWithoutExtension(fullPath) + "--Result.jpg", ImageFormat.Jpeg);

        // open folder containing the image
        Process.Start("explorer.exe", Path.GetDirectoryName(fullPath));
    }

    private byte[] RevealData()
    {
        int width = bitmap.Width;
        int height = bitmap.Height;
        List<byte> dataBytes = new List<byte>();
        int bitIndex = 0;
        byte currentByte = 0;
        int dataLength = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = bitmap.GetPixel(x, y);
                byte[] channels = { pixelColor.R, pixelColor.G, pixelColor.B };

                foreach (var channel in channels)
                {
                    currentByte = (byte)((currentByte << 1) | (channel & 0x01));
                    bitIndex++;
                    if (bitIndex == 8)
                    {
                        dataBytes.Add(currentByte);
                        if (dataBytes.Count == 4) // First 4 bytes give us the length
                        {
                            // Decode length prefix
                            byte[] lengthBytes = dataBytes.ToArray();
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(lengthBytes); // Ensure network byte order (big-endian)
                            dataLength = BitConverter.ToInt32(lengthBytes, 0);
                        }
                        if (dataBytes.Count == dataLength + 4) // Data length + 4 bytes for the length prefix
                            return dataBytes.GetRange(4, dataLength).ToArray();
                        bitIndex = 0;
                        currentByte = 0;
                    }
                }
            }
        }

        if (dataLength > dataBytes.Count)
        {
            char[] errorMessage = "Image contains no message.".ToCharArray();
            byte[] bytes = new byte[errorMessage.Length];

            int i = 0;
            foreach (char c in errorMessage)
            {
                bytes[i] = (byte)(int)(c);
                i++;
            }
            return bytes;
        }

        if(dataLength < 0)
        {
            char[] errorMessage = "Failed to identify message (if it even has one) within the image".ToCharArray();
            byte[] bytes = new byte[errorMessage.Length];

            int i = 0;
            foreach (char c in errorMessage)
            {
                bytes[i] = (byte)(int)(c);
                i++;
            }
            return bytes;
        }

        return dataBytes.GetRange(4, dataLength).ToArray(); // Return data, skipping length prefix
    }

    public string RevealString()
    {
        byte[] data = RevealData();
        return Encoding.UTF8.GetString(data);
    }

    private bool CanHideData(byte[] data)
    {
        // Calculate the maximum number of bits we can hide
        int maxBits = bitmap.Width * bitmap.Height * 3;
        int requiredBits = data.Length * 8;
        return requiredBits <= maxBits;
    }

    private int GetBit(byte[] data, int bitIndex)
    {
        int byteIndex = bitIndex / 8;  // Determine the byte in the array
        int bitPosition = bitIndex % 8;  // Determine the bit position within the byte
        return (data[byteIndex] >> (7 - bitPosition)) & 1;
    }

    /// <summary>
    /// Checks if the image file can contain the specified file content.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public bool CanHideFile(FileInfo file)
    {
        long fileSize = file.Length;
        int maxBits = bitmap.Width * bitmap.Height * 3;
        return fileSize <= maxBits / 8;
    }

    /// <summary>
    /// Checks if the image file can contain the specified text.
    /// </summary>
    /// <returns></returns>
    public bool CanHideString(string text) => CanHideData(Encoding.UTF8.GetBytes(text));

    /// <summary>
    /// Estimates the storage capacity of the image file in bytes.
    /// </summary>
    /// <returns>The amount of bytes that can be hidden in this file</returns>
    public int EstimateStorageCapacity() => bitmap.Width * bitmap.Height * 3 / 8;

    /// <summary>
    /// Checks if the image file can contain the specified file content.
    /// </summary>
    /// <returns></returns>
    public bool CanHideFile(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        return CanHideFile(fileInfo);
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;

        bitmap.Dispose();
        IsDisposed = true;
    }
}

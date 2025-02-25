using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace WinterRose.Music;

public class AudioPlayer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WAVEFORMATEX
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [DllImport("winmm.dll")]
    public static extern int waveOutOpen(out IntPtr hWaveOut, int uDeviceID, WAVEFORMATEX lpFormat, IntPtr dwCallback, IntPtr dwInstance, uint dwFlags);

    [DllImport("winmm.dll")]
    public static extern int waveOutPrepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, uint uSize);

    [DllImport("winmm.dll")]
    public static extern int waveOutWrite(IntPtr hWaveOut, IntPtr lpWaveOutHdr, uint uSize);

    [DllImport("winmm.dll")]
    public static extern int waveOutUnprepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, uint uSize);

    [DllImport("winmm.dll")]
    public static extern int waveOutClose(IntPtr hWaveOut);

    [StructLayout(LayoutKind.Sequential)]
    public struct WAVEHDR
    {
        public IntPtr lpData;
        public uint dwBufferLength;
        public uint dwBytesRecorded;
        public IntPtr dwUser;
        public uint dwFlags;
        public uint dwLoops;
        public IntPtr lpNext;
        public IntPtr reserved;
    }

    public static void Play(byte[] buffer, int sampleRate = 44100, float volume = 1.0f, bool waitForSoundToBePlayed = true)
    {
        // Apply volume adjustment
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)(buffer[i] * volume);
        }

        WAVEFORMATEX format = new WAVEFORMATEX
        {
            wFormatTag = 1,
            nChannels = 1,
            nSamplesPerSec = (uint)sampleRate,
            wBitsPerSample = 8,
            nBlockAlign = 1,
            nAvgBytesPerSec = (uint)sampleRate,
            cbSize = 0
        };

        IntPtr hWaveOut;
        waveOutOpen(out hWaveOut, -1, format, IntPtr.Zero, IntPtr.Zero, 0);

        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            WAVEHDR header = new WAVEHDR
            {
                lpData = handle.AddrOfPinnedObject(),
                dwBufferLength = (uint)buffer.Length,
                dwBytesRecorded = 0,
                dwUser = IntPtr.Zero,
                dwFlags = 0,
                dwLoops = 0,
                lpNext = IntPtr.Zero,
                reserved = IntPtr.Zero
            };

            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            Marshal.StructureToPtr(header, headerPtr, false);

            waveOutPrepareHeader(hWaveOut, headerPtr, (uint)Marshal.SizeOf(header));
            waveOutWrite(hWaveOut, headerPtr, (uint)Marshal.SizeOf(header));

            if (waitForSoundToBePlayed)
            {
                ulong sleepTime = (ulong)buffer.Length * 1000 / (ulong)sampleRate;
                while (sleepTime > 0)
                {
                    int nextSleep = (int)(sleepTime > int.MaxValue ? int.MaxValue : sleepTime);
                    sleepTime -= (ulong)nextSleep;
                    Thread.Sleep(nextSleep);
                }
            }

            waveOutUnprepareHeader(hWaveOut, headerPtr, (uint)Marshal.SizeOf(header));
            Marshal.FreeHGlobal(headerPtr);
        }
        finally
        {
            handle.Free();
            waveOutClose(hWaveOut);
        }
    }
}

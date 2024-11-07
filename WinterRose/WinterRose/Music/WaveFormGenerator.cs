using System;
using System.Collections.Generic;

namespace WinterRose.Music;

public class WaveformGenerator
{
    public static byte[] GenerateSquareWave(int frequency, int durationMs, int sampleRate = 44100)
    {
        int samples = (int)((long)sampleRate * durationMs / 1000);
        byte[] buffer = new byte[samples];
        int period = sampleRate / frequency;

        for (int i = 0; i < samples; i++)
        {
            buffer[i] = (byte)((i % period < period / 2) ? 255 : 0);
        }

        return buffer;
    }

    public static byte[] GenerateSilence(int durationMs, int sampleRate = 44100)
    {
        int samples = (int)((long)sampleRate * durationMs / 1000);
        return new byte[samples];
    }

    public static byte[] GenerateSineWave(int frequency, int durationMs, int sampleRate = 44100)
    {
        int samples = (int)((long)sampleRate * durationMs / 1000);
        byte[] buffer = new byte[samples * 2]; // 2 bytes per sample for 16-bit audio
        double amplitude = 32760; // Max amplitude for 16-bit audio
        double increment = 2.0 * Math.PI * frequency / sampleRate;
        double angle = 0;

        for (int i = 0; i < samples; i++)
        {
            short sample = (short)(amplitude * Math.Sin(angle));
            buffer[i * 2] = (byte)(sample & 0xff);
            buffer[i * 2 + 1] = (byte)((sample >> 8) & 0xff);
            angle += increment;
        }

        return buffer;
    }

    public static byte[] GenerateCombinedSineWave(List<int> frequencies, int durationMs, int sampleRate = 44100)
    {
        int samples = (int)((long)sampleRate * durationMs / 1000);
        byte[] buffer = new byte[samples * 2]; // 2 bytes per sample for 16-bit audio
        double amplitude = 32760 / frequencies.Count; // Adjust amplitude based on the number of frequencies

        double[] increments = new double[frequencies.Count];
        double[] angles = new double[frequencies.Count];

        for (int i = 0; i < frequencies.Count; i++)
        {
            increments[i] = 2.0 * Math.PI * frequencies[i] / sampleRate;
            angles[i] = 0;
        }

        for (int i = 0; i < samples; i++)
        {
            double sampleValue = 0;

            for (int j = 0; j < frequencies.Count; j++)
            {
                sampleValue += amplitude * Math.Sin(angles[j]);
                angles[j] += increments[j];
            }

            short sample = (short)sampleValue;
            buffer[i * 2] = (byte)(sample & 0xff);
            buffer[i * 2 + 1] = (byte)((sample >> 8) & 0xff);
        }

        return buffer;
    }

    public static byte[] GenerateKickDrum(Note note, int sampleRate)
    {
        int samples = (int)((long)sampleRate * note.DurationMs / 1000);
        byte[] buffer = new byte[samples * 2];
        double amplitude = 32760; // Max amplitude for 16-bit audio
        double angularFrequency = 2.0 * Math.PI * note.Frequency / sampleRate;
        double decay = 1.0 / note.DurationMs; // Simple linear decay

        for (int i = 0; i < samples; i++)
        {
            short sampleValue = (short)(amplitude * Math.Sin(angularFrequency * i) * Math.Exp(-decay * i));
            buffer[2 * i] = (byte)(sampleValue & 0xFF);
            buffer[2 * i + 1] = (byte)((sampleValue >> 8) & 0xFF);
        }

        return buffer;
    }
}

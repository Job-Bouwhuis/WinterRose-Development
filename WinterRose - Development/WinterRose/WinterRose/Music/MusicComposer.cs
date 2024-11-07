using System;
using System.IO;

namespace WinterRose.Music;

public class MusicComposer
{
    public static byte[] ComposeFlat(Note[] notes, int sampleRate = 44100)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            foreach (var note in notes)
            {
                if (note.Frequency == 0)
                {
                    stream.Write(WaveformGenerator.GenerateSilence(note.DurationMs, sampleRate), 0, note.DurationMs * sampleRate / 1000);
                }
                else if (note.IsKick)
                {
                    stream.Write(WaveformGenerator.GenerateKickDrum(note, sampleRate));
                }
                else
                {
                    stream.Write(WaveformGenerator.GenerateSquareWave(note.Frequency, note.DurationMs, sampleRate), 0, note.DurationMs * sampleRate / 1000);
                }
            }
            return stream.ToArray();
        }
    }

    public static byte[] ComposeSine(Note[] notes, int sampleRate = 44100)
    {
        int totalSamples = 0;

        // Calculate total number of samples needed
        foreach (var note in notes)
        {
            totalSamples += (int)((sampleRate * note.DurationMs) / 1000.0);
        }

        byte[] buffer = new byte[totalSamples * 2]; // 2 bytes per sample for 16-bit audio

        int bufferIndex = 0;

        foreach (var note in notes)
        {
            byte[] noteWaveform = WaveformGenerator.GenerateSineWave(note.Frequency, note.DurationMs, sampleRate);
            Array.Copy(noteWaveform, 0, buffer, bufferIndex, noteWaveform.Length);
            bufferIndex += noteWaveform.Length;
        }

        return buffer;
    }

    public static byte[] ComposeBass(int frequency, int durationMs = 500, int sampleRate = 44100)
    {
        return WaveformGenerator.GenerateSquareWave(frequency, durationMs, sampleRate);
    }

    public static byte[] ComposeCombinedSine(Chord[] chords, int sampleRate = 44100)
    {
        using MemoryStream stream = new();
        foreach (var chord in chords)
        {
            if (chord.Frequencies.Count is 0)
            {
                stream.Write(WaveformGenerator.GenerateSilence(chord.DurationMs, sampleRate), 0, chord.DurationMs * sampleRate / 1000);
            }
            else
            {
                byte[] chordWaveform = WaveformGenerator.GenerateCombinedSineWave(chord.Frequencies, chord.DurationMs, sampleRate);
                stream.Write(chordWaveform, 0, chordWaveform.Length);
            }
        }

        return stream.ToArray();
    }
}

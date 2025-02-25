using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vortice.Multimedia;

namespace WinterRose.ImGuiApps
{
    public class MediaInfo
    {
        public string Path { get; set; }
        public MediaType Type { get; set; }
    }

    public enum MediaType
    {
        Unknown,
        Sound
    }

    public static class Media
    {

        // Define flags for PlaySound function
        const uint SND_FILENAME = 0x00020000;  // play the sound specified by the file name
        const uint SND_ASYNC = 0x0001;  // play asynchronously

        private static Dictionary<string, MediaInfo> media = new();



        static Media()
        {
            const string path = "Media\\";

            var dir = Directory.CreateDirectory(path);

            foreach (var file in dir.GetFiles())
            {
                if (file.Extension is ".wav" or ".mp3")
                {
                    // sound
                    media.Add(
                        Path.GetFileNameWithoutExtension(file.FullName),
                        new MediaInfo
                        {
                            Path = file.FullName,
                            Type = MediaType.Sound
                        });
                }
            }
        }

        public static void Play(string name)
        {
            if (!media.TryGetValue(name, out var info))
            {
                throw new ArgumentException($"Media '{name}' not found", nameof(name));
            }

            if (info.Type is not MediaType.Sound)
            {
                throw new ArgumentException($"Media '{name}' is not a sound", nameof(name));
            }

            // Specify the path to the audio file
            string audioFilePath = info.Path;

            // Create a new thread for audio playback
            Thread playbackThread = new Thread(() =>
            {
                // Create a WaveOutEvent instance for playback
                using (var waveOut = new WaveOutEvent())
                {
                    // Create a MediaFoundationReader to read the audio file
                    using (var reader = new MediaFoundationReader(audioFilePath))
                    {
                        // Create a SampleToProvider instance to convert the audio format if needed
                        var sampleProvider = reader.ToSampleProvider();

                        // Initiate the WaveOutEvent with the SampleToProvider
                        waveOut.Init(sampleProvider);

                        // Start playback asynchronously
                        waveOut.Play();

                        // Wait for playback to complete
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
            });

            // Start the playback thread
            playbackThread.Start();
        }
    }
}
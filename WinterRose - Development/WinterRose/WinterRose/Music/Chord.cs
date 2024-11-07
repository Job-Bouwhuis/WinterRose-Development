using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Music;

public struct Chord
{
    public List<int> Frequencies { get; }
    public int DurationMs { get; }

    public Chord(List<int> frequencies, int durationMs)
    {
        Frequencies = frequencies;
        DurationMs = durationMs;
    }

    public static Chord Rest(int durationMs = 500) => new([], durationMs);

    public static class Octave4
    {
        public static Chord C(int durationMs = 500) => new Chord(new List<int> { 261, 523, 1046, 2093, 4186, 8372, 16744, 33488 }, durationMs);
        public static Chord CSharp(int durationMs = 500) => new Chord(new List<int> { 277, 554, 1109, 2218, 4435, 8870, 17740, 35480 }, durationMs);
        public static Chord D(int durationMs = 500) => new Chord(new List<int> { 293, 587, 1175, 2349, 4699, 9398, 18796, 37592 }, durationMs);
        public static Chord DSharp(int durationMs = 500) => new Chord(new List<int> { 311, 622, 1245, 2489, 4978, 9956, 19912, 39824 }, durationMs);
        public static Chord E(int durationMs = 500) => new Chord(new List<int> { 329, 659, 1319, 2637, 5274, 10548, 21096, 42192 }, durationMs);
        public static Chord F(int durationMs = 500) => new Chord(new List<int> { 349, 698, 1397, 2794, 5587, 11174, 22348, 44696 }, durationMs);
        public static Chord FSharp(int durationMs = 500) => new Chord(new List<int> { 370, 740, 1480, 2960, 5920, 11840, 23680, 47360 }, durationMs);
        public static Chord G(int durationMs = 500) => new Chord(new List<int> { 392, 784, 1568, 3136, 6271, 12542, 25084, 50168 }, durationMs);
        public static Chord GSharp(int durationMs = 500) => new Chord(new List<int> { 415, 831, 1661, 3322, 6644, 13288, 26576, 53152 }, durationMs);
        public static Chord A(int durationMs = 500) => new Chord(new List<int> { 440, 880, 1760, 3520, 7040, 14080, 28160, 56320 }, durationMs);
        public static Chord ASharp(int durationMs = 500) => new Chord(new List<int> { 466, 932, 1864, 3729, 7459, 14918, 29836, 59672 }, durationMs);
        public static Chord B(int durationMs = 500) => new Chord(new List<int> { 493, 987, 1975, 3951, 7902, 15804, 31608, 63216 }, durationMs);
    }

    public static class Octave5
    {
        public static Chord C(int durationMs = 500) => new Chord(new List<int> { 523, 1046, 2093, 4186, 8372, 16744, 33488, 66976 }, durationMs);
        public static Chord CSharp(int durationMs = 500) => new Chord(new List<int> { 554, 1109, 2218, 4435, 8870, 17740, 35480, 70960 }, durationMs);
        public static Chord D(int durationMs = 500) => new Chord(new List<int> { 587, 1175, 2349, 4699, 9398, 18796, 37592, 75184 }, durationMs);
        public static Chord DSharp(int durationMs = 500) => new Chord(new List<int> { 622, 1245, 2489, 4978, 9956, 19912, 39824, 79648 }, durationMs);
        public static Chord E(int durationMs = 500) => new Chord(new List<int> { 659, 1319, 2637, 5274, 10548, 21096, 42192, 84384 }, durationMs);
        public static Chord F(int durationMs = 500) => new Chord(new List<int> { 698, 1397, 2794, 5587, 11174, 22348, 44696, 89392 }, durationMs);
        public static Chord FSharp(int durationMs = 500) => new Chord(new List<int> { 740, 1480, 2960, 5920, 11840, 23680, 47360, 94720 }, durationMs);
        public static Chord G(int durationMs = 500) => new Chord(new List<int> { 784, 1568, 3136, 6271, 12542, 25084, 50168, 100336 }, durationMs);
        public static Chord GSharp(int durationMs = 500) => new Chord(new List<int> { 831, 1661, 3322, 6644, 13288, 26576, 53152, 106304 }, durationMs);
        public static Chord A(int durationMs = 500) => new Chord(new List<int> { 880, 1760, 3520, 7040, 14080, 28160, 56320, 112640 }, durationMs);
        public static Chord ASharp(int durationMs = 500) => new Chord(new List<int> { 932, 1864, 3729, 7459, 14918, 29836, 59672, 119344 }, durationMs);
        public static Chord B(int durationMs = 500) => new Chord(new List<int> { 987, 1975, 3951, 7902, 15804, 31608, 63216, 126432 }, durationMs);
    }
}

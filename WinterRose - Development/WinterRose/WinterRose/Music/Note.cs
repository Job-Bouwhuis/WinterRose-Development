
namespace WinterRose.Music;

public partial struct Note
{
    public int Frequency { get; set; }
    public int DurationMs { get; set; }

    public bool IsKick { get; set; } = false;

    public Note(int frequency, int durationMs = 500)
    {
        Frequency = frequency;
        DurationMs = durationMs;
    }

    public static Note Rest(int durationMs = 500) => new Note(0, durationMs);

    // Nested classes for octaves
    public static class Octave1
    {
        public static Note C(int durationMs = 500) => new Note(33, durationMs);
        public static Note CSharp(int durationMs = 500) => new Note(35, durationMs);
        public static Note D(int durationMs = 500) => new Note(37, durationMs);
        public static Note DSharp(int durationMs = 500) => new Note(39, durationMs);
        public static Note E(int durationMs = 500) => new Note(41, durationMs);
        public static Note F(int durationMs = 500) => new Note(44, durationMs);
        public static Note FSharp(int durationMs = 500) => new Note(46, durationMs);
        public static Note G(int durationMs = 500) => new Note(49, durationMs);
        public static Note GSharp(int durationMs = 500) => new Note(52, durationMs);
        public static Note A(int durationMs = 500) => new Note(55, durationMs);
        public static Note ASharp(int durationMs = 500) => new Note(58, durationMs);
        public static Note B(int durationMs = 500) => new Note(62, durationMs);
    }

    public static class Octave2
    {
        public static Note C(int durationMs = 500) => new Note(65, durationMs);
        public static Note CSharp(int durationMs = 500) => new Note(69, durationMs);
        public static Note D(int durationMs = 500) => new Note(73, durationMs);
        public static Note DSharp(int durationMs = 500) => new Note(78, durationMs);
        public static Note E(int durationMs = 500) => new Note(82, durationMs);
        public static Note F(int durationMs = 500) => new Note(87, durationMs);
        public static Note FSharp(int durationMs = 500) => new Note(93, durationMs);
        public static Note G(int durationMs = 500) => new Note(98, durationMs);
        public static Note GSharp(int durationMs = 500) => new Note(104, durationMs);
        public static Note A(int durationMs = 500) => new Note(110, durationMs);
        public static Note ASharp(int durationMs = 500) => new Note(117, durationMs);
        public static Note B(int durationMs = 500) => new Note(123, durationMs);
    }

    public static class Octave3
    {
        public static Note C(int durationMs = 500) => new Note(130, durationMs);
        public static Note CSharp(int durationMs = 500) => new Note(138, durationMs);
        public static Note D(int durationMs = 500) => new Note(146, durationMs);
        public static Note DSharp(int durationMs = 500) => new Note(155, durationMs);
        public static Note E(int durationMs = 500) => new Note(164, durationMs);
        public static Note F(int durationMs = 500) => new Note(174, durationMs);
        public static Note FSharp(int durationMs = 500) => new Note(185, durationMs);
        public static Note G(int durationMs = 500) => new Note(196, durationMs);
        public static Note GSharp(int durationMs = 500) => new Note(207, durationMs);
        public static Note A(int durationMs = 500) => new Note(220, durationMs);
        public static Note ASharp(int durationMs = 500) => new Note(233, durationMs);
        public static Note B(int durationMs = 500) => new Note(246, durationMs);
    }

    public static class Octave4
    {
        public static Note C(int durationMs = 500) => new Note(261, durationMs);
        public static Note CSharp(int durationMs = 500) => new Note(277, durationMs);
        public static Note D(int durationMs = 500) => new Note(293, durationMs);
        public static Note DSharp(int durationMs = 500) => new Note(311, durationMs);
        public static Note E(int durationMs = 500) => new Note(329, durationMs);
        public static Note F(int durationMs = 500) => new Note(349, durationMs);
        public static Note FSharp(int durationMs = 500) => new Note(369, durationMs);
        public static Note G(int durationMs = 500) => new Note(392, durationMs);
        public static Note GSharp(int durationMs = 500) => new Note(415, durationMs);
        public static Note A(int durationMs = 500) => new Note(440, durationMs);
        public static Note ASharp(int durationMs = 500) => new Note(466, durationMs);
        public static Note B(int durationMs = 500) => new Note(493, durationMs);
    }

    public static class Octave5
    {
        public static Note C(int durationMs = 500) => new Note(523, durationMs);
        public static Note CSharp(int durationMs = 500) => new Note(554, durationMs);
        public static Note D(int durationMs = 500) => new Note(587, durationMs);
        public static Note DSharp(int durationMs = 500) => new Note(622, durationMs);
        public static Note E(int durationMs = 500) => new Note(659, durationMs);
        public static Note F(int durationMs = 500) => new Note(698, durationMs);
        public static Note FSharp(int durationMs = 500) => new Note(739, durationMs);
        public static Note G(int durationMs = 500) => new Note(783, durationMs);
        public static Note GSharp(int durationMs = 500) => new Note(830, durationMs);
        public static Note A(int durationMs = 500) => new Note(880, durationMs);
        public static Note ASharp(int durationMs = 500) => new Note(932, durationMs);
        public static Note B(int durationMs = 500) => new Note(987, durationMs);
    }

    public static class Octave6
    {
        public static Note C(int durationMs = 500) => new Note(1046, durationMs);
        public static Note CSharp(int durationMs = 500) => new Note(1108, durationMs);
        public static Note D(int durationMs = 500) => new Note(1174, durationMs);
        public static Note DSharp(int durationMs = 500) => new Note(1244, durationMs);
        public static Note E(int durationMs = 500) => new Note(1318, durationMs);
        public static Note F(int durationMs = 500) => new Note(1396, durationMs);
        public static Note FSharp(int durationMs = 500) => new Note(1479, durationMs);
        public static Note G(int durationMs = 500) => new Note(1567, durationMs);
        public static Note GSharp(int durationMs = 500) => new Note(1661, durationMs);
        public static Note A(int durationMs = 500) => new Note(1760, durationMs);
        public static Note ASharp(int durationMs = 500) => new Note(1864, durationMs);
        public static Note B(int durationMs = 500) => new Note(1975, durationMs);
    }

    public static class Octave7
    {
        public static Note C(int durationMs = 500) => new Note(2093, durationMs);
        public static Note CSharp(int durationMs = 500) => new Note(2217, durationMs);
        public static Note D(int durationMs = 500) => new Note(2349, durationMs);
        public static Note DSharp(int durationMs = 500) => new Note(2489, durationMs);
        public static Note E(int durationMs = 500) => new Note(2637, durationMs);
        public static Note F(int durationMs = 500) => new Note(2793, durationMs);
        public static Note FSharp(int durationMs = 500) => new Note(2959, durationMs);
        public static Note G(int durationMs = 500) => new Note(3135, durationMs);
        public static Note GSharp(int durationMs = 500) => new Note(3322, durationMs);
        public static Note A(int durationMs = 500) => new Note(3520, durationMs);
        public static Note ASharp(int durationMs = 500) => new Note(3729, durationMs);
        public static Note B(int durationMs = 500) => new Note(3951, durationMs);
    }

    public static class Octave8
    {
        public static Note C(int durationMs = 500) => new Note(4186, durationMs);
        public static Note CSharp(int durationMs = 500) => new Note(4435, durationMs);
        public static Note D(int durationMs = 500) => new Note(4699, durationMs);
        public static Note DSharp(int durationMs = 500) => new Note(4978, durationMs);
        public static Note E(int durationMs = 500) => new Note(5274, durationMs);
        public static Note F(int durationMs = 500) => new Note(5587, durationMs);
        public static Note FSharp(int durationMs = 500) => new Note(5920, durationMs);
        public static Note G(int durationMs = 500) => new Note(6271, durationMs);
        public static Note GSharp(int durationMs = 500) => new Note(6644, durationMs);
        public static Note A(int durationMs = 500) => new Note(7040, durationMs);
        public static Note ASharp(int durationMs = 500) => new Note(7459, durationMs);
        public static Note B(int durationMs = 500) => new Note(7902, durationMs);
    }
}

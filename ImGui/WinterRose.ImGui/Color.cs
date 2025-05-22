using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ImGuiApps
{
    public class Color
    {
        public static Color Black { get; } = new(0, 0, 0);
        public static Color White { get; } = new(255, 255, 255);
        public static Color Red { get; } = new(255, 0, 0);
        public static Color Green { get; } = new(0, 255, 0);
        public static Color Blue { get; } = new(0, 0, 255);
        public static Color Yellow { get; } = new(255, 255, 0);
        public static Color Cyan { get; } = new(0, 255, 255);
        public static Color Magenta { get; } = new(255, 0, 255);
        public static Color Transparent { get; } = new(0, 0, 0, 0);


        public byte R { get; set; } = 255;
        public byte G { get; set; } = 255;
        public byte B { get; set; } = 255;
        public byte A { get; set; } = 255;

        // include the packed value in serialization to make the color class use up less space in the serialzied data.
        [IncludeWithSerialization]
        public uint PackedValue
        {
            get => (uint)((A << 24) | (B << 16) | (G << 8) | R);
            set
            {
                R = (byte)(value & 0xFF);
                G = (byte)((value >> 8) & 0xFF);
                B = (byte)((value >> 16) & 0xFF);
                A = (byte)((value >> 24) & 0xFF);
            }
        }

        [DefaultArguments((byte)0, (byte)0, (byte)0)]
        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Color(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color(uint packedValue)
        {
            R = (byte)(packedValue & 0xFF);
            G = (byte)((packedValue >> 8) & 0xFF);
            B = (byte)((packedValue >> 16) & 0xFF);
            A = (byte)((packedValue >> 24) & 0xFF);
        }

        public static implicit operator System.Numerics.Vector4(Color color)
        {
            return new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        public static implicit operator Color(System.Numerics.Vector4 color)
        {
            return new((byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255), (byte)(color.W * 255));
        }

        public static implicit operator System.Drawing.Color(Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static implicit operator Color(uint packedValue)
        {
            return new(packedValue);
        }

        public static explicit operator uint(Color color)
        {
            uint c = ImGui.ColorConvertFloat4ToU32((Vector4)color);
            return c;
        }
    }
}

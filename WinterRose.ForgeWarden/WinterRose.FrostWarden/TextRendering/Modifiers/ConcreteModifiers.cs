namespace WinterRose.ForgeWarden.TextRendering;

/// <summary>
/// Stackable modifier for bold text rendering.
/// </summary>
public class BoldModifier : Modifier
{
    public override string Name => "bold";
    public override string Category => "bold";
    public override bool IsStackable => true;
}

/// <summary>
/// Stackable modifier for italic text rendering.
/// </summary>
public class ItalicModifier : Modifier
{
    public override string Name => "italic";
    public override string Category => "italic";
    public override bool IsStackable => true;
}

/// <summary>
/// Stackable modifier for wave animation effect.
/// </summary>
public class WaveModifier : Modifier
{
    public float Amplitude { get; set; } = 3f;
    public float Speed { get; set; } = 2f;
    public float Wavelength { get; set; } = 15f;

    public override string Name => "wave";
    public override string Category => "wave";
    public override bool IsStackable => true;
}

/// <summary>
/// Stackable modifier for shake animation effect.
/// </summary>
public class ShakeModifier : Modifier
{
    public float Intensity { get; set; } = 2f;
    public float Speed { get; set; } = 10f;

    public override string Name => "shake";
    public override string Category => "shake";
    public override bool IsStackable => true;
}

/// <summary>
/// Stackable modifier for typewriter reveal effect.
/// </summary>
public class TypewriterModifier : Modifier
{
    public float CharacterDelay { get; set; } = 0.1f;

    public override string Name => "typewriter";
    public override string Category => "typewriter";
    public override bool IsStackable => true;

    public float StartTime { get; set; } = float.NaN; 
}

/// <summary>
/// Stackable modifier for link regions (scoped hyperlinks).
/// </summary>
public class LinkModifier : Modifier
{
    public string Url { get; set; }

    public LinkModifier(string url)
    {
        Url = url;
    }

    public override string Name => "link";
    public override string Category => "link";
    public override bool IsStackable => true;
}

/// <summary>
/// Persistent modifier for text color.
/// Replaces previous color modifier when applied.
/// </summary>
public class ColorModifier : Modifier
{
    public Raylib_cs.Color Color { get; set; }

    public ColorModifier(Raylib_cs.Color color)
    {
        Color = color;
    }

    public override string Name => "color";
    public override string Category => "color";
    public override bool IsStackable => false;
}

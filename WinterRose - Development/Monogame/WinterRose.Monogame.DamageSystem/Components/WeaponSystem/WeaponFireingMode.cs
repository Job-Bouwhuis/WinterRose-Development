namespace WinterRose.Monogame.Weapons;

[System.Flags]
public enum WeaponFireingMode
{
    Single = 1 << 0,
    Burst = 1 << 1,
    Auto = 1 << 2
}
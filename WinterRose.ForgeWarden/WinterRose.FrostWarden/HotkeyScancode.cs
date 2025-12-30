namespace WinterRose.ForgeWarden;

[Flags]
public enum HotkeyScancode : int
{
    None = 0,

    MouseLeft = 0x01,
    MouseRight = 0x02,
    MouseMiddle = 0x04,
    Mouse4 = 0x05, 
    Mouse5 = 0x06,
    
    // Letters
    A = 0x1E,
    B = 0x30,
    C = 0x2E,
    D = 0x20,
    E = 0x12,
    F = 0x21,
    G = 0x22,
    H = 0x23,
    I = 0x17,
    J = 0x24,
    K = 0x25,
    L = 0x26,
    M = 0x32,
    N = 0x31,
    O = 0x18,
    P = 0x19,
    Q = 0x10,
    R = 0x13,
    S = 0x1F,
    T = 0x14,
    U = 0x16,
    V = 0x2F,
    W = 0x11,
    X = 0x2D,
    Y = 0x15,
    Z = 0x2C,

    // Numbers (top row)
    D1 = 0x02,
    D2 = 0x03,
    D3 = 0x04,
    D4 = 0x05,
    D5 = 0x06,
    D6 = 0x07,
    D7 = 0x08,
    D8 = 0x09,
    D9 = 0x0A,
    D0 = 0x0B,

    // Modifiers
    LeftShift = 0x2A,
    RightShift = 0x36,
    LeftCtrl = 0x1D,
    RightCtrl = 0x11D, // extended
    LeftAlt = 0x38,
    RightAlt = 0x138,  // extended
    LeftWin = 0x15B,
    RightWin = 0x15C,

    // Enter / Backspace / Space
    Enter = 0x1C,
    Backspace = 0x0E,
    Space = 0x39,
    Tab = 0x0F,
    Escape = 0x01,

    // Function keys
    F1 = 0x3B,
    F2 = 0x3C,
    F3 = 0x3D,
    F4 = 0x3E,
    F5 = 0x3F,
    F6 = 0x40,
    F7 = 0x41,
    F8 = 0x42,
    F9 = 0x43,
    F10 = 0x44,
    F11 = 0x57,
    F12 = 0x58,

    // Symbols
    OemMinus = 0x0C,        // -
    OemPlus = 0x0D,         // =
    OemOpenBrackets = 0x1A, // [
    OemCloseBrackets = 0x1B,// ]
    OemBackslash = 0x2B,    // \
    OemSemicolon = 0x27,    // ;
    OemQuotes = 0x28,       // '
    OemComma = 0x33,        // ,
    OemPeriod = 0x34,       // .
    OemSlash = 0x35,        // /
    OemTilde = 0x29         // `
}

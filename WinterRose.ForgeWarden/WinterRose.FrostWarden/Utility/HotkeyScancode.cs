namespace WinterRose.ForgeWarden.Utility;

[Flags]
public enum HotkeyScancode: int
{
    None = 0,

    MouseLeft = 0x01,
    MouseRight = 0x02,
    MouseMiddle = 0x04,
    Mouse4 = 0x05,   // usually “Mouse 4” (Back)
    Mouse5 = 0x06,   // usually “Mouse 5” (Forward)

    A = 0x41,
    B = 0x42,
    C = 0x43,
    D = 0x44,
    E = 0x45,
    F = 0x46,
    G = 0x47,
    H = 0x48,
    I = 0x49,
    J = 0x4A,
    K = 0x4B,
    L = 0x4C,
    M = 0x4D,
    N = 0x4E,
    O = 0x4F,
    P = 0x50,
    Q = 0x51,
    R = 0x52,
    S = 0x53,
    T = 0x54,
    U = 0x55,
    V = 0x56,
    W = 0x57,
    X = 0x58,
    Y = 0x59,
    Z = 0x5A,

    D0 = 0x30,
    D1 = 0x31,
    D2 = 0x32,
    D3 = 0x33,
    D4 = 0x34,
    D5 = 0x35,
    D6 = 0x36,
    D7 = 0x37,
    D8 = 0x38,
    D9 = 0x39,

    LeftShift = 0xA0,
    RightShift = 0xA1,
    LeftCtrl = 0xA2,
    RightCtrl = 0xA3,
    LeftAlt = 0xA4,
    RightAlt = 0xA5,

    Escape = 0x1B,
    Space = 0x20,
    Enter = 0x0D,
    Tab = 0x09,
    Backspace = 0x08,

    OemMinus = 0xBD,          // VK_OEM_MINUS
    OemPlus = 0xBB,           // VK_OEM_PLUS
    OemOpenBrackets = 0xDB,   // VK_OEM_4
    OemCloseBrackets = 0xDD,  // VK_OEM_6
    OemBackslash = 0xDC,      // VK_OEM_5
    OemSemicolon = 0xBA,      // VK_OEM_1
    OemQuotes = 0xDE,         // VK_OEM_7
    OemComma = 0xBC,          // VK_OEM_COMMA
    OemPeriod = 0xBE,         // VK_OEM_PERIOD
    OemSlash = 0xBF,          // VK_OEM_2
    OemTilde = 0xC0,          // VK_OEM_3

    F1 = 0x70,
    F2 = 0x71,
    F3 = 0x72,
    F4 = 0x73,
    F5 = 0x74,
    F6 = 0x75,
    F7 = 0x76,
    F8 = 0x77,
    F9 = 0x78,
    F10 = 0x79,
    F11 = 0x7A,
    F12 = 0x7B
}

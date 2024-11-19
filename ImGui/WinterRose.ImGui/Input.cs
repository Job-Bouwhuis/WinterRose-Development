using WinterRose.ImGuiUI.Win32;

namespace WinterRose.ImGuiApps;

public static class Input
{
    private static Dictionary<Key, WinKey> keys = InputHelper.CreateKeyMap();

    public static bool GetKeyDown(Key key) => Utils.IsKeyPressedAndNotTimeout(keys[key]);
    public static bool GetKey(Key key) => Utils.IsKeyPressed(keys[key]);
}

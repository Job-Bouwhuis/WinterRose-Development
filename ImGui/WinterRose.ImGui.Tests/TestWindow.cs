
namespace WinterRose.ImGuiApps.Tests;
using gui = ImGuiNET.ImGui;

public class TestWindow : ImGuiWindow
{
    public override void Render()
    {
        GifRenderer.Gif("test.gif", 200, 200, 24);
        GifRenderer.Gif("walk.gif", 800, 800, 30);

        gui.Button("Demo window");
        if (gui.IsItemHovered() && Input.GetKey(Key.LeftMouseButton))
            Application.AddWindow(new ImGuiDemoWindow());
    }
}
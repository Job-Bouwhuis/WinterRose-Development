namespace WinterRose.ImGuiApps;

/// <summary>
/// Adds the default ImGui demo window to the application.
/// </summary>
public class ImGuiDemoWindow() : ImGuiWindow("ImGuiDemoWindow")
{
    public override void Render()
    {
        Flags = ImGuiNET.ImGuiWindowFlags.None;
        gui.SetWindowSize(new(200, 200));

        gui.Text("Displaying ImGUI default demo window...");
                gui.ShowDemoWindow();
        if(gui.Button("Close"))
            Close();

    }
}

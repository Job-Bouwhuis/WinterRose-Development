using gui = ImGuiNET.ImGui;

namespace WinterRose.Monogame.Tests;

internal class BallSpawnerUtilsWindow : ImGuiLayout
{
    BallSpawner spawner;

    protected override void Awake()
    {
        spawner = FetchComponent<BallSpawner>();
    }


    public override void RenderLayout()
    {
        gui.Begin("Ball Spawner");

        if (gui.Button("Clear balls"))
        {
            foreach(var ball in spawner.spawned)
            {
                ball.Destroy();
            }
        }

        gui.End();
    }

    protected override void Update() { }
}

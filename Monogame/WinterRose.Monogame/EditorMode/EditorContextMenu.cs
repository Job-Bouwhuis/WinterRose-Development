using ImGuiNET;
using Microsoft.Xna.Framework;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame.EditorMode
{
    internal static class EditorContextMenu
    {
        public static bool editorContextMenuOpen;
        private static bool posSet = true;
        private static Vector2 objectSpawnPos;

        static string objectName = "New Object";

        public static void Render()
        {
            if (!editorContextMenuOpen)
            {
                posSet = true;
                return;
            }

            if (posSet)
            {
                gui.SetNextWindowPos(new System.Numerics.Vector2(ImGui.GetMousePos().X, ImGui.GetMousePos().Y), ImGuiCond.Always);
                posSet = false;
            }

            gui.Begin("Editor Context Menu", ref editorContextMenuOpen);

            gui.SetWindowSize(new System.Numerics.Vector2(200, 200));

            gui.InputText("Object Name", ref objectName, 100);
            if (gui.MenuItem("Create Object"))
            {
                WorldObject obj = Universe.CurrentWorld.CreateObject(objectName);
                obj.transform.position = objectSpawnPos;

                objectName = "New Object";
                editorContextMenuOpen = false;
            }

            gui.End();
        }

        internal static void Open()
        {
            editorContextMenuOpen = true;
            posSet = true;
            objectSpawnPos = Transform.ScreenToWorldPos(Input.MousePosition, Camera.current);
        }
    }
}

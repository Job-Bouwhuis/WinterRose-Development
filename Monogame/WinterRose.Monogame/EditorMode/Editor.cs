using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame.EditorMode;

/// <summary>
/// The editor class that manages the editor state.
/// </summary>
public static class Editor
{
    /// <summary>
    /// Whether or not the editor is currently open
    /// </summary>
    public static bool Opened
    {
        get => isEditMode;
        set
        {
            if (value)
            {
                cameraIndexBeforeEditMode = Application.Current.CameraIndex;

                World world = Universe.CurrentWorld;

                editorCamera = world.CreateObject<Camera>("Editor Camera (DO NOT DELETE)");

                Camera nonEditorCam = world.GetCamera(cameraIndexBeforeEditMode);
                editorCamera.transform.position = nonEditorCam?.transform?.position ?? MonoUtils.ScreenCenter;

                debugObject = world.CreateObject("Debug Object (DO NOT DELETE)");
                debugObject.AddUpdateBehavior(obj =>
                {
                    obj.transform.position = editorCamera.transform.position;
                });


                Application.Current.CameraIndex = editorCamera.CameraIndex;
                isEditMode = true;
            }
            else
            {
                isEditMode = false;

                if (editorCamera is not null)
                {
                    editorCamera.Destroy();
                    debugObject!.Destroy();
                    Application.Current.CameraIndex = cameraIndexBeforeEditMode;
                }
            }
        }
    }

    private static float camSpeed = 300f;
    private static float actualCamSpeed = camSpeed;
    public static Camera? editorCamera;
    public static WorldObject? debugObject;
    private static bool isEditMode = false;

    internal static bool AllowScrollForSpeed { get; set; } = true;

    private static int cameraIndexBeforeEditMode = -1;

    /// <summary>
    /// Toggles the editor open or closed
    /// </summary>
    public static void ToggleEditor()
    {
        Opened = !Opened;
    }

    /// <summary>
    /// Opens the editor
    /// </summary>
    public static void OpenEditor()
    {
        Opened = true;
    }
    /// <summary>
    /// Closes the editor
    /// </summary>
    public static void CloseEditor()
    {
        Opened = false;
    }

    internal static void Update()
    {
        EditorMain.Update();

        Vector2 input = Input.GetNormalizedWASDInput(true);

        if (AllowScrollForSpeed && !gui.IsWindowHovered(ImGuiNET.ImGuiHoveredFlags.AnyWindow) && Input.GetKey(Microsoft.Xna.Framework.Input.Keys.LeftShift, true))
        {
            camSpeed += Input.MouseScrollDelta * 10;
            if (camSpeed < 1)
                camSpeed = 1;
            actualCamSpeed = camSpeed * 2;
        }
        else
        {
            actualCamSpeed = camSpeed;
        }

        if (input != Vector2.Zero)
        {
            editorCamera!.transform.position += input * actualCamSpeed * Time.deltaTime;
        }
    }

    internal static void Render()
    {
        var batch = MonoUtils.SpriteBatch;
        editorCamera.BeginWorldSpaceSpriteBatch(batch);

        EditorMain.Render();

        string text = "Cam speed: " + actualCamSpeed;
        float width = MonoUtils.DefaultFont.MeasureString(text).X;

        batch.DrawString(
            MonoUtils.DefaultFont,
            "Cam speed: " + actualCamSpeed,
            editorCamera.TopRight + new Vector2(0, 10) - new Vector2(width, 0),
            Color.White);

        batch.End();
    }

}

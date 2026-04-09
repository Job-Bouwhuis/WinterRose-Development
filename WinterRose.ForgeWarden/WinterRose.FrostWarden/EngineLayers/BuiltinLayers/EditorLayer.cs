using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.EngineLayers.Events;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Tooltipping;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.ForgeWarden.Worlds;

namespace WinterRose.ForgeWarden.EngineLayers.BuiltinLayers;

/// <summary>
/// Editor Layer - Replaces RuntimeLayer when in editor mode.
/// Provides editor UI windows alongside world/UI rendering.
/// </summary>
public class EditorLayer : EngineLayer
{
    private RenderTexture2D worldTexture;
    private RenderTexture2D uiTexture;

    private UIWindow viewportWindow;
    private UIWindow logWindow;
    private UIWindow controlsWindow;
    private EditorViewport viewportContent;
    private UILog logContent;
    private bool windowsInitialized = false;
    private World currentWorld;

    public EditorLayer() : base("Editor") => Importance = 0;

    public override void OnUpdate()
    {
        // Initialize windows on first update if needed
        if (!windowsInitialized && worldTexture.Id != 0)
            InitializeEditorWindows();

        // Update current world
        currentWorld = Universe.CurrentWorld;

        // Handle keyboard shortcuts
        HandleEditorInput();

        // Handle play mode input
        if (viewportContent != null && viewportContent.IsInPlayMode)
        {
            HandlePlayModeInput();
        }
    }

    private void HandleEditorInput()
    {
        // P key to toggle play mode
        if (Raylib.IsKeyPressed(KeyboardKey.P) && viewportContent != null)
        {
            if (viewportContent.IsInPlayMode)
            {
                viewportContent.ExitPlayMode();
                LogMessage("Exited play mode", UILog.LogLevel.Info);
            }
            else
            {
                viewportContent.EnterPlayMode();
                LogMessage("Entered play mode", UILog.LogLevel.Info);
            }
        }

        // E key to export world (placeholder)
        if (Raylib.IsKeyPressed(KeyboardKey.E))
        {
            LogMessage("Export world - Not implemented yet", UILog.LogLevel.Warning);
        }
    }

    public override void OnRender()
    {
        // Initialize windows if needed
        if (!windowsInitialized)
            InitializeEditorWindows();

        // UI overlay
        Raylib.DrawTexturePro(
            uiTexture.Texture,
            new Rectangle(0, 0, uiTexture.Texture.Width, -uiTexture.Texture.Height),
            new Rectangle(0, 0, Engine.Window.Width, Engine.Window.Height),
            Vector2.Zero,
            0,
            Color.White);

        // Draw editor UI systems on top
        UIWindowManager.Draw();
        Dialogs.Draw();
        ToastToDialogMorpher.Draw();
        Toasts.Draw();
        Tooltips.Draw();
    }

    private void HandlePlayModeInput()
    {
        // In play mode, restrict mouse to viewport
        Vector2 mousePos = Raylib.GetMousePosition();
        if (viewportContent.TryRestrictMouseToViewport(ref mousePos))
        {
            // Mouse would be restricted here if Raylib supported it
            // For now, we just track it
        }

        // Allow Escape to exit play mode and unlock mouse
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            viewportContent.ExitPlayMode();
            LogMessage("Exited play mode", UILog.LogLevel.Info);
        }
    }

    public override void OnEvent<TEvent>(ref TEvent engineEvent)
    {
        if (engineEvent is not FrameCompleteEvent e)
            return;

        // Store the textures for rendering
        worldTexture = e.WorldTexture;
        uiTexture = e.UiTexture;

        // Update viewport if it exists
        if (viewportContent != null)
            viewportContent.SetRenderTexture(e.WorldTexture);
    }

    private void InitializeEditorWindows()
    {
        int screenWidth = Engine.Window.Width;
        int screenHeight = Engine.Window.Height;
        const int padding = 10;

        float viewportWidth = screenWidth * 0.66f - padding * 2;
        float viewportHeight = screenHeight * 0.75f;

        // Create viewport window (takes up right 2/3 of screen)
        RichText viewportTitle = "Viewport";
        viewportTitle.Font = ForgeWardenEngine.DefaultFont;
        viewportTitle.FontSize = 14;

        viewportWindow = new UIWindow(
            viewportTitle,
            viewportWidth,
            viewportHeight,
            screenWidth * 0.33f + padding,
            padding
        );

        currentWorld = Universe.CurrentWorld;
        viewportContent = new EditorViewport(worldTexture, currentWorld);
        viewportContent.ViewportSize = new Vector2(viewportWidth, viewportHeight);
        viewportWindow.AddContent(viewportContent);
        UIWindowManager.Show(viewportWindow);

        // Create log window (bottom left)
        float logWidth = screenWidth * 0.33f - padding * 2;
        float logHeight = screenHeight * 0.5f - padding * 3;

        RichText logTitle = "Editor Log";
        logTitle.Font = ForgeWardenEngine.DefaultFont;
        logTitle.FontSize = 14;

        logWindow = new UIWindow(
            logTitle,
            logWidth,
            logHeight,
            padding,
            padding + 30
        );
        logContent = new UILog();
        logWindow.AddContent(logContent);
        UIWindowManager.Show(logWindow);

        windowsInitialized = true;
        LogMessage("Editor initialized - Press 'P' to toggle play mode, ESC to exit play mode, 'E' to export", UILog.LogLevel.Info);
    }

    /// <summary>
    /// Log a message to the editor log window
    /// </summary>
    public void LogMessage(string message, UILog.LogLevel level = UILog.LogLevel.Info)
    {
        logContent?.AddLogEntry(message, level);
    }
}
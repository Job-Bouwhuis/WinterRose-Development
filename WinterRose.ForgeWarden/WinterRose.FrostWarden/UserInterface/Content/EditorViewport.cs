using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.Worlds;
using WinterRose.Recordium;
using World = WinterRose.ForgeWarden.Worlds.World;

namespace WinterRose.ForgeWarden.UserInterface.Content;

/// <summary>
/// Enhanced viewport for editor with entity selection and gizmo rendering
/// </summary>
public class EditorViewport : UIContent
{
    private RenderTexture2D renderTexture;
    public Vector2 ViewportSize { get; set; } = new Vector2(800, 600);
    public Rectangle ViewportBounds { get; private set; }

    Log log = new Log("EditorViewport");

    private EditorSelection selection;
    private World world;
    private Camera2D editorCamera;
    private Vector2 lastClickPosition;
    private List<Entity> hierarchyCache = new();
    private int hierarchySelectionIndex = 0;
    private ViewportHierarchyLink hierarchyLink;

    // Play mode state
    private bool isPlayMode = false;
    private WorldStateSnapshot playModeSnapshot;
    private bool mouseLockedToViewport = false;
    private Vector2 mouseLockCenter;

    // Gizmo state
    private GizmoMode currentGizmoMode = GizmoMode.None;
    private GizmoAxis draggedAxis = GizmoAxis.None;
    private Vector3 gizmoStartValue;
    private Vector2 gizmoDragStart;

    public EditorViewport(RenderTexture2D renderTexture, World world)
    {
        this.renderTexture = renderTexture;
        this.world = world;
        this.selection = new EditorSelection();
        this.editorCamera = new Camera2D
        {
            Target = Vector2.Zero,
            Offset = new Vector2(400, 300),
            Rotation = 0,
            Zoom = 1.0f
        };
    }

    public void SetRenderTexture(RenderTexture2D texture)
    {
        renderTexture = texture;
    }

    public EditorSelection GetSelection() => selection;

    public bool IsInPlayMode => isPlayMode;

    /// <summary>
    /// Get the optional hierarchy link (allows bidirectional selection sync)
    /// </summary>
    public ViewportHierarchyLink GetHierarchyLink()
    {
        return hierarchyLink ??= new ViewportHierarchyLink();
    }

    /// <summary>
    /// Set a hierarchy link for selection synchronization
    /// </summary>
    public void SetHierarchyLink(ViewportHierarchyLink link)
    {
        hierarchyLink = link;
        log.Info("[EditorViewport] Hierarchy link set");
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        return new Vector2(
            Math.Min(ViewportSize.X, availableArea.Width),
            Math.Min(ViewportSize.Y, availableArea.Height)
        );
    }

    protected override void Draw(Rectangle bounds)
    {
        ViewportBounds = bounds;

        if (renderTexture.Id == 0)
            return;

        // Draw the world texture
        Raylib.DrawTexturePro(
            renderTexture.Texture,
            new Rectangle(0, 0, renderTexture.Texture.Width, -renderTexture.Texture.Height),
            bounds,
            Vector2.Zero,
            0,
            Color.White);

        // Draw gizmos and selection overlays (when not in play mode)
        if (!isPlayMode)
        {
            DrawSelectionGizmos(bounds);
        }

        // Draw play mode indicator
        if (isPlayMode)
        {
            Raylib.DrawRectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, 20, new Color(0, 200, 0, 100));
            Raylib.DrawText("PLAY MODE", (int)bounds.X + 5, (int)bounds.Y + 2, 12, Color.White);
        }

        // Draw viewport border
        Color borderColor = isPlayMode ? Color.Green : Color.DarkGray;
        Raylib.DrawRectangleLines(
            (int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height,
            borderColor);
    }

    private void DrawSelectionGizmos(Rectangle bounds)
    {
        var selected = selection.PrimarySelection;
        if (selected == null)
            return;

        Vector3 entityPos = selected.transform.position;
        Vector2 screenPos = WorldToScreenCoordinates(new Vector2(entityPos.X, entityPos.Y), bounds);

        // Draw selection indicator circle
        Raylib.DrawCircleLines((int)screenPos.X, (int)screenPos.Y, 8, Color.Yellow);

        // Draw position gizmo (red/green arrows)
        DrawAxisGizmo(screenPos, bounds, selected);

        // Draw center square for freeform movement
        const int squareSize = 6;
        Raylib.DrawRectangle(
            (int)screenPos.X - squareSize / 2,
            (int)screenPos.Y - squareSize / 2,
            squareSize, squareSize,
            Color.White);

        // Draw scale gizmo (small rectangles at corners)
        DrawScaleGizmo(screenPos, bounds, selected);

        // Draw rotation gizmo (circle with indicator)
        DrawRotationGizmo(screenPos, bounds, selected);
    }

    private void DrawAxisGizmo(Vector2 screenPos, Rectangle bounds, Entity entity)
    {
        const int arrowLength = 40;

        // Red arrow (X axis)
        Vector2 xAxisEnd = screenPos + new Vector2(arrowLength, 0);
        Raylib.DrawLineEx(screenPos, xAxisEnd, 2, Color.Red);
        Raylib.DrawTriangle(
            xAxisEnd + new Vector2(0, -3),
            xAxisEnd,
            xAxisEnd + new Vector2(0, 3),
            Color.Red);

        // Green arrow (Y axis)
        Vector2 yAxisEnd = screenPos + new Vector2(0, -arrowLength);
        Raylib.DrawLineEx(screenPos, yAxisEnd, 2, Color.Green);
        Raylib.DrawTriangle(
            yAxisEnd + new Vector2(-3, 0),
            yAxisEnd,
            yAxisEnd + new Vector2(3, 0),
            Color.Green);
    }

    private void DrawScaleGizmo(Vector2 screenPos, Rectangle bounds, Entity entity)
    {
        const int handleSize = 4;
        const int distance = 30;

        Vector2[] corners = new Vector2[]
        {
            screenPos + new Vector2(distance, distance),
            screenPos + new Vector2(-distance, distance),
            screenPos + new Vector2(-distance, -distance),
            screenPos + new Vector2(distance, -distance),
        };

        foreach (var corner in corners)
        {
            Raylib.DrawRectangle(
                (int)corner.X - handleSize / 2,
                (int)corner.Y - handleSize / 2,
                handleSize, handleSize,
                Color.Magenta);
        }
    }

    private void DrawRotationGizmo(Vector2 screenPos, Rectangle bounds, Entity entity)
    {
        const float radius = 50;
        Color rotationColor = new Color(0, 255, 255, 255); // Cyan
        Raylib.DrawCircleLines((int)screenPos.X, (int)screenPos.Y, (int)radius, rotationColor);

        // Draw rotation indicator (small circle on the rotation ring)
        Vector3 euler = QuaternionToEuler(entity.transform.rotation);
        float angle = euler.Z; // Z rotation for 2D
        Vector2 indicatorPos = screenPos + new Vector2(
            MathF.Cos(angle) * radius,
            MathF.Sin(angle) * radius);
        Raylib.DrawCircle((int)indicatorPos.X, (int)indicatorPos.Y, 4, rotationColor);
    }

    internal protected override float GetHeight(float maxWidth)
    {
        return ViewportSize.Y;
    }

    // Input handling
    protected internal override void OnContentClicked(MouseButton button)
    {
        log.Info($"[EditorViewport] OnContentClicked fired with button: {button}");

        if (button != MouseButton.Left)
            return;

        log.Info($"[EditorViewport] ViewportBounds: {ViewportBounds}");
        log.Info($"[EditorViewport] Handling left mouse button click");

        if (isPlayMode)
        {
            log.Info($"[EditorViewport] In play mode, skipping selection");
            // In play mode, unlock mouse
            if (mouseLockedToViewport)
            {
                UnlockMouse();
            }
            return;
        }

        // Get click position from UIContent Input system (this is already relative to content)
        Vector2 clickPos = Input.MousePosition;
        log.Info($"[EditorViewport] Click position from Input: {clickPos}");

        // Convert screen coordinates to world coordinates
        Vector2 worldPos = ScreenToWorldCoordinates(clickPos, ViewportBounds);
        log.Info($"[EditorViewport] Converted to world position: {worldPos}");

        // Find entities at click position
        hierarchyCache = selection.FindEntitiesAtPosition(worldPos, world);

        if (hierarchyCache.Count == 0)
        {
            log.Info($"[EditorViewport] No entities found at click position");
            selection.Clear();
            hierarchySelectionIndex = 0;
        }
        else
        {
            // If we clicked on the same position, cycle through entities
            // Otherwise start fresh
            log.Info($"[EditorViewport] Found {hierarchyCache.Count} entities, selecting first");
            lastClickPosition = clickPos;
            hierarchySelectionIndex = 0;
            selection.Select(hierarchyCache[hierarchySelectionIndex]);

            // Sync to hierarchy if linked
            if (hierarchyLink?.IsLinked == true)
            {
                hierarchyLink.SyncViewportSelectionToHierarchy();
            }
        }
    }

    public void EnterPlayMode()
    {
        isPlayMode = true;
        playModeSnapshot = new WorldStateSnapshot(world);
        LockMouse();
    }

    public void ExitPlayMode()
    {
        isPlayMode = false;
        if (playModeSnapshot != null)
        {
            playModeSnapshot.RestoreState();
        }
        UnlockMouse();
    }

    private void LockMouse()
    {
        mouseLockedToViewport = true;
        mouseLockCenter = new Vector2(
            ViewportBounds.X + ViewportBounds.Width / 2,
            ViewportBounds.Y + ViewportBounds.Height / 2);
    }

    private void UnlockMouse()
    {
        mouseLockedToViewport = false;
    }

    public bool IsMouseLocked => mouseLockedToViewport;

    public bool TryRestrictMouseToViewport(ref Vector2 mousePos)
    {
        if (!mouseLockedToViewport)
            return false;

        // Clamp mouse to viewport bounds
        mousePos = new Vector2(
            Math.Clamp(mousePos.X, ViewportBounds.X, ViewportBounds.X + ViewportBounds.Width),
            Math.Clamp(mousePos.Y, ViewportBounds.Y, ViewportBounds.Y + ViewportBounds.Height));

        return true;
    }

    // Coordinate conversion (accounts for camera transform)
    private Vector2 WorldToScreenCoordinates(Vector2 worldPos, Rectangle bounds)
    {
        // Apply camera transformation to convert world space to screen space
        Vector2 relativeToCamera = worldPos - editorCamera.Target;

        // Apply camera zoom and rotation
        float cosRot = MathF.Cos(editorCamera.Rotation * MathF.PI / 180f);
        float sinRot = MathF.Sin(editorCamera.Rotation * MathF.PI / 180f);

        Vector2 rotated = new Vector2(
            relativeToCamera.X * cosRot - relativeToCamera.Y * sinRot,
            relativeToCamera.X * sinRot + relativeToCamera.Y * cosRot
        );

        Vector2 scaled = rotated * editorCamera.Zoom;

        // Apply camera offset (where on screen the camera target is rendered)
        return bounds.Position + editorCamera.Offset + scaled;
    }

    private Vector2 ScreenToWorldCoordinates(Vector2 screenPos, Rectangle bounds)
    {
        // Reverse the camera transformation to convert screen space to world space

        // Step 1: Remove viewport bounds offset
        Vector2 viewportRelative = screenPos - bounds.Position;

        // Step 2: Remove camera offset
        Vector2 relativeToCameraOffset = viewportRelative - editorCamera.Offset;

        // Step 3: Remove camera zoom
        Vector2 unscaled = relativeToCameraOffset / editorCamera.Zoom;

        // Step 4: Remove camera rotation
        float cosRot = MathF.Cos(editorCamera.Rotation * MathF.PI / 180f);
        float sinRot = MathF.Sin(editorCamera.Rotation * MathF.PI / 180f);

        Vector2 unrotated = new Vector2(
            unscaled.X * cosRot + unscaled.Y * sinRot,
            -unscaled.X * sinRot + unscaled.Y * cosRot
        );

        // Step 5: Add back camera target to get world position
        return unrotated + editorCamera.Target;
    }

    // Helper functions
    private Vector3 QuaternionToEuler(Quaternion q)
    {
        // Extract Z rotation for 2D (simplified)
        float sinZ = 2 * (q.W * q.Z + q.X * q.Y);
        float cosZ = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        return new Vector3(0, 0, MathF.Atan2(sinZ, cosZ));
    }

    private enum GizmoMode
    {
        None,
        Position,
        Rotation,
        Scale
    }

    private enum GizmoAxis
    {
        None,
        X,
        Y,
        Z,
        Freeform
    }
}

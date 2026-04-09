using System;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.UserInterface.Content;
using WinterRose.ForgeWarden.UserInterface.Windowing;

namespace WinterRose.ForgeWarden.UserInterface.Content;

/// <summary>
/// Optional bidirectional link between the viewport selection and hierarchy panel.
/// This allows the editor layer to sync selections between the two systems without
/// making them dependent on each other, enabling the hierarchy to work independently
/// in both editor and runtime contexts.
/// </summary>
public class ViewportHierarchyLink
{
    private EditorViewport viewport;
    private Action<Entity> hierarchySelectionCallback;
    private Action<Entity> viewportSelectionCallback;
    private bool isLinked = false;

    public ViewportHierarchyLink()
    {
        System.Diagnostics.Debug.WriteLine("[ViewportHierarchyLink] Created (not yet linked)");
    }

    /// <summary>
    /// Link the viewport selection to a hierarchy selection callback.
    /// The callback will be invoked whenever the viewport selection changes.
    /// </summary>
    public void LinkViewportToHierarchy(EditorViewport viewport, Action<Entity> hierarchySelectionCallback)
    {
        this.viewport = viewport;
        this.hierarchySelectionCallback = hierarchySelectionCallback;
        isLinked = true;
        System.Diagnostics.Debug.WriteLine("[ViewportHierarchyLink] Linked viewport to hierarchy");
    }

    /// <summary>
    /// Link the hierarchy selection to the viewport.
    /// Call this when the hierarchy selection changes to update the viewport.
    /// </summary>
    public void SelectInViewport(Entity entity)
    {
        if (!isLinked || viewport == null || entity == null)
            return;

        var selection = viewport.GetSelection();
        selection.Select(entity);
        System.Diagnostics.Debug.WriteLine($"[ViewportHierarchyLink] Selected '{entity.Name}' in viewport via hierarchy link");
    }

    /// <summary>
    /// Check if there's an active selection in the viewport and sync it to hierarchy
    /// </summary>
    public void SyncViewportSelectionToHierarchy()
    {
        if (!isLinked || viewport == null || hierarchySelectionCallback == null)
            return;

        var selection = viewport.GetSelection();
        var primarySelection = selection.PrimarySelection;
        
        if (primarySelection != null)
        {
            hierarchySelectionCallback.Invoke(primarySelection);
            System.Diagnostics.Debug.WriteLine($"[ViewportHierarchyLink] Synced viewport selection '{primarySelection.Name}' to hierarchy");
        }
    }

    /// <summary>
    /// Unlink the viewport and hierarchy
    /// </summary>
    public void Unlink()
    {
        viewport = null;
        hierarchySelectionCallback = null;
        viewportSelectionCallback = null;
        isLinked = false;
        System.Diagnostics.Debug.WriteLine("[ViewportHierarchyLink] Unlinked viewport and hierarchy");
    }

    public bool IsLinked => isLinked;
}

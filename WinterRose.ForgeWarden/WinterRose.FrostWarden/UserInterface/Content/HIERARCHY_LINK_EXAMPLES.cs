/*
 * VIEWPORT HIERARCHY LINK INTEGRATION EXAMPLE
 * 
 * This example shows how to optionally connect the EditorViewport and Hierarchy
 * panels so that selecting an entity in one updates the other.
 * 
 * IMPORTANT: This is optional! Both systems work independently.
 * The hierarchy can work in runtime layers without this link.
 */

using System;
using WinterRose.ForgeWarden.Editor;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.UserInterface.Content;

namespace WinterRose.ForgeWarden.EngineLayers.BuiltinLayers;

// ============================================================================
// EXAMPLE 1: Add to EditorLayer class
// ============================================================================
/*
public partial class EditorLayer : EngineLayer
{
    private Hierarchy hierarchyWindow;
    private ViewportHierarchyLink hierarchyLink;

    private void InitializeEditorWindows()
    {
        // ... existing viewport initialization code ...
        
        // Create Hierarchy window (left side)
        float hierarchyWidth = screenWidth * 0.33f - padding * 2;
        float hierarchyHeight = screenHeight * 0.35f - padding * 3;

        RichText hierarchyTitle = "Hierarchy";
        hierarchyTitle.Font = ForgeWardenEngine.DefaultFont;
        hierarchyTitle.FontSize = 14;

        hierarchyWindow = new Hierarchy();
        hierarchyWindow.ControlWidth = hierarchyWidth;
        hierarchyWindow.ControlHeight = hierarchyHeight;
        hierarchyWindow.ControlX = padding;
        hierarchyWindow.ControlY = padding;
        
        UIWindowManager.Show(hierarchyWindow);

        // Setup optional viewport-hierarchy link
        SetupViewportHierarchyLink();

        windowsInitialized = true;
    }

    private void SetupViewportHierarchyLink()
    {
        // Create the optional link between viewport and hierarchy
        hierarchyLink = new ViewportHierarchyLink();

        // When hierarchy selection changes, update viewport
        hierarchyLink.LinkViewportToHierarchy(
            viewportContent,
            (selectedEntity) =>
            {
                // This callback is called when viewport selection changes
                // Update the hierarchy to highlight the selected entity
                if (hierarchyWindow != null && selectedEntity != null)
                {
                    // Assuming hierarchy has a SelectEntity method
                    // hierarchyWindow.SelectEntity(selectedEntity);
                    System.Diagnostics.Debug.WriteLine($"[EditorLayer] Hierarchy notified of selection: {selectedEntity.Name}");
                }
            }
        );

        // Give the viewport the link so it can notify hierarchy on selection
        viewportContent.SetHierarchyLink(hierarchyLink);

        System.Diagnostics.Debug.WriteLine("[EditorLayer] Viewport-Hierarchy link established");
    }

    public override void OnUpdate()
    {
        // ... existing code ...

        // Periodically sync viewport selection to hierarchy
        // This could also be event-based instead of polling
        if (hierarchyLink?.IsLinked == true && hierarchyWindow != null)
        {
            hierarchyLink.SyncViewportSelectionToHierarchy();
        }
    }
}
*/

// ============================================================================
// EXAMPLE 2: Hierarchy selecting entity should also select in viewport
// ============================================================================
/*
// In Hierarchy.cs, modify the node selection handler:

private void OnHierarchyNodeSelected(Entity selectedEntity)
{
    // When user clicks entity in hierarchy, also select in viewport
    if (hierarchyLink?.IsLinked == true)
    {
        hierarchyLink.SelectInViewport(selectedEntity);
        System.Diagnostics.Debug.WriteLine($"[Hierarchy] Selected {selectedEntity.Name} in viewport via link");
    }
}
*/

// ============================================================================
// EXAMPLE 3: Standalone usage without hierarchy
// ============================================================================
/*
// EditorViewport works perfectly fine without any hierarchy link
// The link is completely optional and not required for basic functionality

var viewport = new EditorViewport(renderTexture, world);
// That's it - viewport works with or without hierarchy link
// No hierarchy link needed for viewport to work
*/

// ============================================================================
// BENEFITS OF THIS ARCHITECTURE
// ============================================================================
/*
✅ Optional Linking: Link is opt-in, not mandatory
✅ Separation of Concerns: Hierarchy doesn't depend on viewport
✅ Runtime Compatible: Hierarchy works in any layer (editor or runtime)
✅ Flexible: Can link one-way or two-way as needed
✅ Replaceable: Easy to swap hierarchy implementations
✅ Testable: Each component works independently

SYNC FLOWS:
───────────

1. User clicks entity in viewport:
   Viewport → [OnContentClicked] → Selection → [if linked] → Hierarchy

2. User clicks entity in hierarchy:
   Hierarchy → [if linked] → ViewportHierarchyLink → Viewport Selection

3. Multiple viewports (future):
   Just create multiple ViewportHierarchyLink instances
   or create a more complex multiplexer
*/

// ============================================================================
// DEBUG OUTPUT WHEN LINKED
// ============================================================================
/*
When selecting an entity with hierarchy linked, you'll see:

[EditorViewport] OnContentClicked fired with button: Left
[EditorViewport] Found 1 entities, selecting first
[EditorSelection] Selected entity: MyEntity at (100, 50, 0)
[ViewportHierarchyLink] Synced viewport selection 'MyEntity' to hierarchy
[EditorLayer] Hierarchy notified of selection: MyEntity

You can trace the entire selection flow in the Output window.
*/

// ============================================================================
// CUSTOMIZATION POINTS
// ============================================================================
/*
To customize the link behavior:

1. Override ViewportHierarchyLink and implement custom logic
2. Add debouncing/throttling if selections are too frequent
3. Add undo/redo integration
4. Add selection filters (only certain entity types)
5. Add multi-select support

Example custom link:
───────────────────
public class CustomHierarchyLink : ViewportHierarchyLink
{
    private float lastSyncTime = 0f;
    private const float SYNC_THROTTLE = 0.1f; // 100ms min between syncs

    public override void SyncViewportSelectionToHierarchy()
    {
        if (Time.time - lastSyncTime < SYNC_THROTTLE)
            return;

        base.SyncViewportSelectionToHierarchy();
        lastSyncTime = Time.time;
    }
}
*/

// ============================================================================
// FILE REFERENCE
// ============================================================================
/*
Main files involved:

1. EditorViewport.cs
   - GetHierarchyLink() - Get the current link (or create one)
   - SetHierarchyLink(link) - Assign a link
   - OnContentClicked() - Notifies link when selection changes

2. ViewportHierarchyLink.cs
   - LinkViewportToHierarchy() - Register hierarchy callback
   - SelectInViewport() - Update viewport from hierarchy
   - SyncViewportSelectionToHierarchy() - Sync viewport → hierarchy
   - Unlink() - Disconnect

3. EditorSelection.cs
   - Select() - Core selection logic
   - PrimarySelection - Current selected entity
   - FindEntitiesAtPosition() - Find clickable entities

4. EditorLayer.cs
   - InitializeEditorWindows() - Create windows
   - OnUpdate() - Call SyncViewportSelectionToHierarchy()
   - LogMessage() - Log to editor log window
*/


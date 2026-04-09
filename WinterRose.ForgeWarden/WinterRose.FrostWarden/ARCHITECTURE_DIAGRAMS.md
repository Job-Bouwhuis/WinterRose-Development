# Entity Selection and Hierarchy Linking Architecture

## System Flow Diagrams

### 1. Entity Selection Flow (Viewport Click)
```
┌─────────────────────────────────────────────────────────────┐
│ User clicks mouse on viewport                               │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ UIContainer Input System (inherited from UIContent)         │
│ - Checks if content is hovered                              │
│ - Verifies mouse button pressed/released                    │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
        ┌──────────────────────────────────────┐
        │ Content.IsHovered == true?           │
        └──────────────────────────────────────┘
            YES ▼                         NO ▼
           ✅ Proceed            ❌ No action (debug!)
               │
               ▼
┌─────────────────────────────────────────────────────────────┐
│ EditorViewport.OnContentClicked(button)                     │
│ [Detailed debug logging starts here]                        │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ Get mouse position from Input context                       │
│ Input.MousePosition → clickPos                              │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ Convert screen coordinates to world coordinates             │
│ ScreenToWorldCoordinates(clickPos, ViewportBounds)          │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ EditorSelection.FindEntitiesAtPosition(worldPos, world)     │
│ 1. Search all world._Entities                               │
│ 2. Check if within 50-unit radius                           │
│ 3. Sort by distance (closest first)                         │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
        ┌──────────────────────────────────────┐
        │ Found any entities?                  │
        └──────────────────────────────────────┘
           YES ▼                         NO ▼
           Select           ┌─────────────────────┐
           ↓               │ selection.Clear()    │
    hierarchyCache[0]      │ hierarchySelectionIn │
                           │ dex = 0              │
           │               └─────────────────────┘
           ▼                        │
┌─────────────────────────────────────────────────────────────┐
│ EditorSelection.Select(entity)                              │
│ - selectedEntities.Clear()                                  │
│ - selectedEntities.Add(entity)                              │
│ - Debug output: "Selected {name} at {position}"             │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼ [if hierarchyLink?.IsLinked]
┌─────────────────────────────────────────────────────────────┐
│ ViewportHierarchyLink.SyncViewportSelectionToHierarchy()   │
│ → Notify hierarchy of new selection                         │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ EditorViewport.DrawSelectionGizmos()                        │
│ (Next frame Draw call)                                      │
│ - Convert entity world pos to screen pos                    │
│ - Draw yellow circle around entity                          │
│ - Draw red/green axis gizmos                                │
│ - Draw white center square                                  │
│ - Draw cyan rotation circle                                 │
└─────────────────────────────────────────────────────────────┘
```

### 2. Play Mode State Management
```
┌─────────────────────────────────────────────────────────────┐
│ User presses P or calls EnterPlayMode()                     │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ EditorViewport.EnterPlayMode()                              │
│ - isPlayMode = true                                         │
│ - Create WorldStateSnapshot                                 │
│ - Lock mouse to viewport                                    │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ WorldStateSnapshot constructor calls CaptureState()         │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ WorldStateSnapshot.CaptureState()                           │
│ - Iterate through world._Entities                           │
│ - For each entity, create EntityState:                      │
│   - Entity reference                                        │
│   - Name (string copy)                                      │
│   - Tags (List copy)                                        │
│   - Position (Vector3 copy)                                 │
│   - Rotation (Quaternion copy)                              │
│   - Scale (Vector3 copy)                                    │
│ - Store in entityStates list                                │
│ - Debug: "Captured state for N entities"                    │
└─────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
                    ┌─────────────────┐
                    │  PLAY MODE      │
                    │  (player can    │
                    │   modify world) │
                    └────────┬────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│ User presses P again or ESC to ExitPlayMode()              │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ EditorViewport.ExitPlayMode()                               │
│ - isPlayMode = false                                        │
│ - If playModeSnapshot exists:                               │
│   - Call playModeSnapshot.RestoreState()                    │
│ - Unlock mouse                                              │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ WorldStateSnapshot.RestoreState()                           │
│ - Iterate through entityStates                              │
│ - For each state (parallel with world._Entities):           │
│   - Restore entity.Name from state.Name                     │
│   - Restore entity.Tags from state.Tags                     │
│   - Restore entity.transform.position from state.Position   │
│   - Restore entity.transform.rotation from state.Rotation   │
│   - Restore entity.transform.scale from state.Scale         │
│ - Debug: "Restored state for N entities"                    │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
                ┌──────────────────────┐
                │  EDITOR MODE         │
                │  (world restored)    │
                └──────────────────────┘
```

### 3. Optional Hierarchy-Viewport Linking
```
┌─────────────────────────────────────────────────────────────┐
│                   ViewportHierarchyLink                      │
│                   (Optional Bridge)                          │
├─────────────────────────────────────────────────────────────┤
│ ViewportHierarchyLink link = new ViewportHierarchyLink()   │
│                                                              │
│ link.LinkViewportToHierarchy(viewport, (entity) => {        │
│   // Callback when viewport selection changes              │
│   hierarchyWindow.SelectEntity(entity);                     │
│ });                                                          │
│                                                              │
│ viewport.SetHierarchyLink(link);                            │
└─────────────────────────────────────────────────────────────┘
        ▲                             ▲
        │                             │
        │ [Optional Link]             │ [Optional Link]
        │                             │
        │                             │
    ┌───┴─────────────────┐    ┌─────┴────────────────┐
    │  EditorViewport     │    │  Hierarchy Window    │
    ├─────────────────────┤    ├────────────────────┐─┤
    │                     │    │                    │ │
    │ selection.Select()  │    │ OnEntitySelected() │ │
    │         │           │    │        │           │ │
    │         ▼           │    │        ▼           │ │
    │ [if linked] ───────────→ │ hierarchyLink.     │ │
    │   notify hierarchy  │    │ SelectInViewport() │ │
    │                     │    │                    │ │
    └─────────────────────┘    └────────────────────┘─┘
            (Optional)              (Optional)
        Works w/o link          Works w/o link
```

## Class Relationships

```
┌──────────────────────────┐
│      EditorViewport      │
│  (extends UIContent)     │
├──────────────────────────┤
│ - EditorSelection        │
│ - World                  │
│ - ViewportHierarchyLink? │ ◄── Optional
├──────────────────────────┤
│ OnContentClicked()       │
│ GetHierarchyLink()       │
│ SetHierarchyLink()       │
│ DrawSelectionGizmos()    │
└──────────────────────────┘

┌──────────────────────────┐
│  EditorSelection         │
├──────────────────────────┤
│ - selectedEntities List  │
├──────────────────────────┤
│ Select(Entity)           │
│ Clear()                  │
│ FindEntitiesAtPosition() │
│ PrimarySelection         │
└──────────────────────────┘

┌──────────────────────────┐
│ ViewportHierarchyLink    │ ◄── Optional
│  (NEW)                   │
├──────────────────────────┤
│ - EditorViewport?        │
│ - hierarchyCallback?     │
├──────────────────────────┤
│ LinkViewportToHierarchy()│
│ SelectInViewport()       │
│ SyncViewportToHierarchy()│
│ IsLinked                 │
└──────────────────────────┘

┌──────────────────────────┐
│  WorldStateSnapshot      │
├──────────────────────────┤
│ - entityStates List      │ ◄── In-memory copies
│   (EntityState[])        │
├──────────────────────────┤
│ CaptureState()           │
│ RestoreState()           │
└──────────────────────────┘
```

## Data Flow: Screen Click → Selection → Rendering

```
Screen Space (pixels)
       │
       ▼ Input.MousePosition
       
[250, 150]  (viewport-relative pixels)
       │
       ▼ ScreenToWorldCoordinates(clickPos, bounds)
       
World Space (units)
       │
       ▼
       
[240, 120]  (world X, Y)
       │
       ├─→ FindEntitiesAtPosition([240, 120], 50-unit radius)
       │
       ▼
       
Entity[] candidates  (sorted by distance)
       │
       ├─→ Select entities[0]
       │
       ▼
       
selection.PrimarySelection = Entity
       │
       ├─→ [Next frame Draw call]
       │
       ▼
       
entity.transform.position ([240, 120, 0])
       │
       ▼ WorldToScreenCoordinates(entity.pos, viewport.bounds)
       
[250, 150]  (screen pixels for rendering)
       │
       ▼ Raylib.DrawCircleLines([250,150], 8, Yellow)
       
Gizmos rendered on screen!
```

## State Diagram: EditorViewport Modes

```
                ┌─────────────┐
                │   INITIAL   │
                │  isPlayMode │
                │   = false   │
                └──────┬──────┘
                       │
        ┌──────────────┴──────────────┐
        │                             │
        ▼                             ▼
 ┌────────────────┐          ┌───────────────┐
 │  EDITOR MODE   │          │  PLAY MODE    │
 │  (Selection)   │◄─────────┤(State Locked) │
 │                │  Exit    │               │
 │ OnClickContent │  Play    │ EnterPlayMode │
 │  .Select()     ├─────────►│ .CaptureState │
 │ .Gizmos on     │ Enter    │ .LockMouse    │
 │ .Hierarchy     │  Play    │               │
 │  updatable     │          │ ExitPlayMode  │
 │                │          │ .RestoreState │
 └────────────────┘          │ .UnlockMouse  │
                             └───────────────┘
```

## Coordinate System Transformation

```
Screen Space                World Space
(pixel coordinates)         (game units)

┌─────────────────┐
│  [250, 150]     │
│  (screen pixel) │         ┌─────────────────┐
│                 │         │ [240, 120]      │
│  Viewport at    │─RelPos─►│ (world units)   │
│  (160, 40)      │         │                 │
│  Size: 1088x1000         │ = [250-160,     │
└─────────────────┘         │    150-40]     │
                            │ = [90, 110]    │
                            │                 │
                            │ No camera zoom  │
                            │ (1:1 mapping)   │
                            └─────────────────┘
```

## Debug Output Timeline

```
Frame N: User clicks viewport at pixel [250, 150]
│
├─ Raylib.IsMouseButtonPressed() = true
├─ UIContainer checks IsHovered = true ✓
├─ UIContent.OnContentClicked(Left) called
│
└─► [EditorViewport] OnContentClicked fired with button: Left
    [EditorViewport] ViewportBounds: {160, 40, 1088, 1000}
    [EditorViewport] Click position from Input: 250, 150
    [EditorViewport] Converted to world position: 90, 110
    
    [EditorSelection] Finding entities at world position: (90, 110)
    [EditorSelection]   - Checking: Entity "Player" at (100, 100)
    [EditorSelection]   - Entity Player is 14.14 units away ✓ (within 50)
    [EditorSelection]   - Checking: Entity "Enemy" at (200, 200)
    [EditorSelection]   - Entity Enemy is 155.56 units away ✗ (outside 50)
    [EditorSelection] Found 1 entities at position (90, 110)
    
    [EditorSelection] Selected entity: Player at (100, 100, 0)
    [EditorViewport] Found 1 entities, selecting first
    [ViewportHierarchyLink] Synced viewport selection 'Player' to hierarchy

Frame N+1: Draw frame
│
└─► [EditorViewport.Draw]
    entity.transform.position = (100, 100, 0)
    Raylib.DrawCircleLines(screen_x, screen_y, 8, Yellow) ✓
    Gizmos visible!
```

---

This architecture ensures:
- ✅ Clear separation of concerns
- ✅ Testable individual components
- ✅ Optional linking doesn't affect core functionality
- ✅ Runtime-compatible hierarchy system
- ✅ Easy debugging with comprehensive logging


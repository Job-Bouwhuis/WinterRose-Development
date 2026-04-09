# Editor System Implementation

## Overview
This document describes the comprehensive editor system that has been added to WinterRose.ForgeWarden, enabling interactive entity selection, transformation gizmos, and play mode with world state management.

## Components Implemented

### 1. **EditorSelection.cs**
Manages entity selection in the editor with hierarchical selection support.

**Key Features:**
- Single and multi-entity selection
- Hierarchical selection based on entity proximity
- Selection state queries (IsSelected, PrimarySelection)
- Entity-at-position queries using simple AABB collision detection

**Public API:**
```csharp
Select(Entity entity)          // Select single entity
Toggle(Entity entity)          // Add/remove from selection
Add(Entity entity)             // Add to selection
IsSelected(Entity entity)      // Query selection state
PrimarySelection               // Get first selected entity
FindEntitiesAtPosition(Vector2 worldPos, World world) // Get entities near position
```

### 2. **WorldStateSnapshot.cs**
Captures and restores world state for undo/redo and play mode restoration.

**Key Features:**
- Serializes entity transforms to JSON
- Captures position, rotation, scale for each entity
- Restores world to exact previous state
- Error handling for serialization failures

**Public API:**
```csharp
CaptureState()        // Save current world state
RestoreState()        // Restore to saved state
```

### 3. **EditorViewport.cs**
Enhanced viewport UIContent with entity selection and gizmo rendering.

**Key Features:**
- Displays world render texture with overlay UI
- Click-to-select entities with visual feedback
- Gizmo rendering for selected entities:
  - Position gizmos (red/green arrows for X/Y axes, white square for freeform)
  - Rotation gizmo (cyan circle with rotation indicator)
  - Scale gizmo (magenta corner handles)
- Play mode with world state save/restore
- Mouse locking to viewport during play mode
- Screen-to-world coordinate conversion

**Public API:**
```csharp
GetSelection()              // Get EditorSelection instance
IsInPlayMode                // Check play mode state
EnterPlayMode()             // Start play mode with state save
ExitPlayMode()              // Exit play mode with state restore
IsMouseLocked               // Check if mouse is viewport-locked
TryRestrictMouseToViewport(ref Vector2 mousePos) // Restrict mouse bounds
```

### 4. **EditorLayer.cs** (Updated)
Main editor layer managing windows and editor input.

**New Features:**
- EditorViewport integration
- Keyboard shortcuts:
  - **P**: Toggle play mode
  - **ESC** (in play mode): Exit play mode
  - **E**: Export world (placeholder)
- Editor logging
- Input routing for play mode

**Public API:**
```csharp
LogMessage(string message, UILog.LogLevel level)  // Log to editor
```

## Usage

### Activating the Editor
The editor is activated by setting `ForgeWardenEngine.Current.EditorEnabled = true` before starting the engine.

### Basic Workflow

1. **Start Engine**: Engine loads with EditorLayer active
2. **Select Entities**: Click on entities in the viewport
3. **Transform Selected**: Gizmos appear for position, rotation, scale adjustment
4. **Enter Play Mode**: Press 'P' to test the world
   - World state is automatically saved
   - Mouse is locked to viewport
   - Input is directed to viewport
   - Press ESC to exit play mode
   - World is restored to pre-play state
5. **Export**: Press 'E' to export (placeholder for future implementation)

### Editor Layout

- **Right Side (66% of screen)**:
  - Viewport window (80% height) - Shows game world with gizmos
  - Log window (20% height) - Timestamped editor messages

- **Left Side (33% of screen)**:
  - Log window - Continues from right side layout

## Technical Details

### Coordinate Systems
- **Screen Coordinates**: Raw viewport window pixel positions
- **World Coordinates**: Game world units (currently 1:1 mapping)

### Selection Algorithm
1. Find all entities within 25 units of click position
2. Sort by distance (closest first)
3. Select closest entity
4. Support for cycling through overlapping entities on subsequent clicks

### Gizmo System
- **Position Gizmos**: 
  - Red arrow (X-axis movement)
  - Green arrow (Y-axis movement)
  - White square (freeform movement)
- **Rotation Gizmo**: Cyan ring with rotation indicator
- **Scale Gizmo**: Magenta corner handles for scale adjustment

### Play Mode Mechanics
- Saves complete world state (entity positions, rotations, scales, names, tags)
- Mouse restricted to viewport bounds (logical, not enforced by Raylib)
- Input routing to viewport layer
- Full state restoration on exit
- Escape key unlocks play mode

## Future Enhancements

1. **Gizmo Interactions**:
   - Drag position arrows to move entities
   - Rotate by dragging rotation gizmo
   - Scale by dragging corners

2. **Advanced Selection**:
   - Multi-select (Shift+Click)
   - Box select
   - Selection by component type

3. **World Management**:
   - Undo/Redo system using state snapshots
   - Export world to format
   - Scene management

4. **Inspector Panel**:
   - Display entity properties
   - Edit component values
   - Add/remove components

5. **Viewport Features**:
   - Grid/snap system
   - Camera controls (pan/zoom)
   - Collision visualization
   - Physics debug overlay

6. **Play Mode Enhancements**:
   - Record playback actions
   - Frame-by-frame stepping
   - Performance profiling

## Known Limitations

1. **Coordinate Conversion**: Currently simple 1:1 mapping; no camera support yet
2. **Entity Bounds**: Uses simple distance check (25 unit radius), not actual collider bounds
3. **Gizmo Dragging**: Visual gizmos present but drag logic not yet implemented
4. **Mouse Locking**: Logical restriction only; Raylib doesn't support OS-level mouse locking
5. **Serialization**: Basic JSON serialization; doesn't handle all component data

## Files Created

```
WinterRose.ForgeWarden\WinterRose.FrostWarden\UserInterface\Content\
  - EditorSelection.cs
  - WorldStateSnapshot.cs
  - EditorViewport.cs
  
WinterRose.ForgeWarden\WinterRose.FrostWarden\EngineLayers\BuiltinLayers\
  - EditorLayer.cs (updated)
```

## Integration Points

The editor system integrates with:
- **Engine**: Via ForgeWardenEngine.EditorEnabled property
- **Layers**: EditorLayer replaces RuntimeLayer when active
- **Input**: Keyboard shortcuts (P, E, ESC)
- **UI System**: Uses UIWindow, UIContent, UILog
- **Worlds**: Universe.CurrentWorld for entity access
- **Rendering**: Receives render textures via FrameCompleteEvent


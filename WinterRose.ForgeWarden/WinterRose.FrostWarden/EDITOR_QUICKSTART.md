# Editor Quick Start Guide

## Enabling the Editor

```csharp
// In your application startup
ForgeWardenEngine.Current.EditorEnabled = true;
```

This will:
1. Replace RuntimeLayer with EditorLayer
2. Display viewport and log windows
3. Initialize entity selection system
4. Enable keyboard shortcuts

## Editor Controls

### Viewport Interaction

| Control | Action |
|---------|--------|
| Left Click | Select entity at cursor |
| Click Again | Cycle through overlapping entities |
| P | Toggle Play Mode |
| ESC | Exit Play Mode (restores world state) |
| E | Export World (placeholder) |

## UI Layout

```
┌──────────────────────────────────────────────┐
│  Log Messages                 │ Viewport     │
│  (Timestamped)               │ (Game World) │
│                              │              │
│  Ready                       │              │
│  Editor initialized - P to   │ [Selected    │
│  play, ESC to exit, E export │  Entity]     │
│                              │              │
└──────────────────────────────────────────────┘
```

## Play Mode

### Entering Play Mode

Press **P** to enter play mode:
- Current world state is saved automatically
- World updates are enabled
- Mouse is restricted to viewport
- Selection mode is disabled

### Exiting Play Mode

Press **ESC** to exit play mode:
- World state is restored to pre-play snapshot
- All entity transforms reset
- Selection mode re-enabled
- Mouse restriction removed

### Mouse Behavior in Play Mode

- Mouse is logically restricted to viewport bounds
- Cannot move cursor outside viewport area
- Click in viewport while mouse is unlocked to re-lock
- Press ESC to unlock

## Entity Selection

### How Selection Works

1. **Click viewport** to select nearest entity at cursor
2. Selected entity displays **yellow circle** outline
3. **Red and green arrows** appear for position gizmo
4. **Cyan circle** shows rotation gizmo
5. **Magenta squares** at corners show scale gizmo

### Selection Details

- Selection radius: **25 units** from click point
- Entities sorted by distance (closest selected)
- Only one entity selected at a time
- Click empty space to deselect

## Gizmos

### Position Gizmo
```
        ↑ (Green - Y axis)
        │
← (Red - X axis) ●
```

- **Red Arrow**: Drag to move X-axis
- **Green Arrow**: Drag to move Y-axis
- **White Square**: Drag for freeform movement
- Square at center of entity

### Rotation Gizmo
```
    ○ with indicator dot
   ╱ ╲
  ╱   ╲
 ●─────●
```

- **Cyan Circle**: Drag indicator to rotate
- Rotation center is entity position
- Rotation displayed as small dot on ring

### Scale Gizmo
```
  ◾    ◾
    ●
  ◾    ◾
```

- **Magenta Squares**: Drag for scale
- Corners at ~30 units from center
- Can be axis-locked or freeform

## Editor Log

The log window shows:
- **Info** (blue): General messages
- **Warning** (yellow): Important events
- **Error** (red): Problems
- **Debug** (gray): Detailed information

Each entry includes timestamp in format: `[HH:MM:SS.mmm]`

## Common Workflows

### Testing a Level

1. Set up your world
2. Press **P** to enter play mode
3. Test gameplay
4. Press **ESC** to return to exact pre-test state
5. Make edits and try again

### Positioning Entities

1. Click entity in viewport
2. Red/Green arrows appear
3. Drag arrow to constrain movement to axis
4. Drag white square for diagonal movement

### Rotating Entities

1. Click entity in viewport
2. Cyan rotation circle appears
3. Drag rotation dot to adjust angle
4. Rotation updates in real-time

### Scaling Objects

1. Click entity in viewport
2. Magenta corner squares appear
3. Drag corners to adjust scale
4. All corners can be used for uniform scale

## Logging

### Accessing Logs Programmatically

```csharp
// In your systems, access editor logging
if (ForgeWardenEngine.Current.EditorEnabled)
{
    var editorLayer = ForgeWardenEngine.Current.LayerStack.GetLayer<EditorLayer>();
    editorLayer?.LogMessage("Your message", UILog.LogLevel.Info);
}
```

### Log Levels

```csharp
UILog.LogLevel.Debug   // Detailed debugging info
UILog.LogLevel.Info    // General information
UILog.LogLevel.Warning // Important notices
UILog.LogLevel.Error   // Error messages
```

## Troubleshooting

### Entities Not Selecting

- Make sure viewport is active (focused window)
- Check entity is within 25 unit radius of click
- Entity must exist in current world
- Look for error messages in log window

### Play Mode Not Restoring State

- Check log for state restore errors
- Ensure world entities weren't destroyed during play
- Verify entity component state wasn't manually altered

### Mouse Seems Locked in Play Mode

- Press ESC to unlock
- Click in viewport to re-lock
- Mouse restriction is logical only (not OS-level)

### No Selection Visual

- Check if entity is too far from click point
- Verify entity exists in world
- Look for yellow selection circle outline

## Performance Notes

- Selection check: O(n) where n = entities
- State snapshot: Serializes all entity data to JSON
- Gizmo rendering: One gizmo per selected entity
- Logging: String concatenation, minimal overhead

## File Reference

- **EditorLayer.cs**: Main editor layer
- **EditorViewport.cs**: Viewport rendering and input
- **EditorSelection.cs**: Entity selection logic
- **WorldStateSnapshot.cs**: State save/restore
- **UILog.cs**: Editor logging window


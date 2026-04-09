using System;
using System.Collections.Generic;
using System.Numerics;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.Worlds;

namespace WinterRose.ForgeWarden.UserInterface.Content;

/// <summary>
/// Manages entity selection in the editor, with support for hierarchical selection
/// </summary>
public class EditorSelection
{
    private List<Entity> selectedEntities = new();
    public IReadOnlyList<Entity> SelectedEntities => selectedEntities.AsReadOnly();

    /// <summary>
    /// Gets the primary selected entity (first in selection)
    /// </summary>
    public Entity? PrimarySelection => selectedEntities.Count > 0 ? selectedEntities[0] : null;

    /// <summary>
    /// Clear all selections
    /// </summary>
    public void Clear()
    {
        if (selectedEntities.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"[EditorSelection] Cleared {selectedEntities.Count} selected entities");
        }
        selectedEntities.Clear();
    }

    /// <summary>
    /// Select a single entity (replaces previous selection)
    /// </summary>
    public void Select(Entity entity)
    {
        selectedEntities.Clear();
        selectedEntities.Add(entity);
        System.Diagnostics.Debug.WriteLine($"[EditorSelection] Selected entity: {entity.Name} at {entity.transform.position}");
    }

    /// <summary>
    /// Toggle entity selection
    /// </summary>
    public void Toggle(Entity entity)
    {
        if (selectedEntities.Contains(entity))
            selectedEntities.Remove(entity);
        else
            selectedEntities.Add(entity);
    }

    /// <summary>
    /// Add entity to selection
    /// </summary>
    public void Add(Entity entity)
    {
        if (!selectedEntities.Contains(entity))
            selectedEntities.Add(entity);
    }

    /// <summary>
    /// Check if entity is selected
    /// </summary>
    public bool IsSelected(Entity entity)
    {
        return selectedEntities.Contains(entity);
    }

    /// <summary>
    /// Find all entities at a world position and return them in hierarchical order
    /// </summary>
    public List<Entity> FindEntitiesAtPosition(Vector2 worldPosition, Worlds.World world)
    {
        List<Entity> found = new();
        const float detectionRadius = 50f;

        System.Diagnostics.Debug.WriteLine($"[EditorSelection] Finding entities at world position: {worldPosition}");

        // Simple bounds-based selection for now
        // Could be extended with more sophisticated collision detection
        foreach (var entity in world._Entities)
        {
            if (IsEntityAtPosition(entity, worldPosition, detectionRadius))
            {
                found.Add(entity);
                System.Diagnostics.Debug.WriteLine($"  - Found entity: {entity.Name}");
            }
        }

        // Sort by distance from position (closest first)
        found.Sort((a, b) =>
        {
            float distA = Vector2.Distance(new Vector2(a.transform.position.X, a.transform.position.Y), worldPosition);
            float distB = Vector2.Distance(new Vector2(b.transform.position.X, b.transform.position.Y), worldPosition);
            return distA.CompareTo(distB);
        });

        System.Diagnostics.Debug.WriteLine($"[EditorSelection] Found {found.Count} entities at position {worldPosition}");
        return found;
    }

    /// <summary>
    /// Check if entity contains the given world position (simple distance check)
    /// </summary>
    private bool IsEntityAtPosition(Entity entity, Vector2 worldPosition, float detectionRadius)
    {
        // Simple check: entity is within detection radius of position
        // This can be extended with actual mesh/collider bounds
        Vector3 entityPos = entity.transform.position;
        Vector2 entityPos2D = new Vector2(entityPos.X, entityPos.Y);
        float distance = Vector2.Distance(entityPos2D, worldPosition);
        bool isInRange = distance < detectionRadius;

        if (isInRange)
            System.Diagnostics.Debug.WriteLine($"  - Entity {entity.Name} is {distance:F2} units away (within radius {detectionRadius})");

        return isInRange;
    }
}

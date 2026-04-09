using System;
using System.Collections.Generic;
using System.Numerics;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.Worlds;

namespace WinterRose.ForgeWarden.UserInterface.Content;

/// <summary>
/// Snapshot of world state for play mode restoration using in-memory object copies.
/// This avoids JSON serialization overhead and is more reliable for runtime state preservation.
/// </summary>
public class WorldStateSnapshot
{
    private List<EntityState> entityStates = new();
    private World world;

    public WorldStateSnapshot(World world)
    {
        this.world = world;
        CaptureState();
    }

    /// <summary>
    /// Capture the current world state by storing references and copies of entity data
    /// </summary>
    public void CaptureState()
    {
        try
        {
            entityStates.Clear();

            foreach (var entity in world._Entities)
            {
                // Create a snapshot of this entity's state
                var state = new EntityState
                {
                    Entity = entity,
                    Name = entity.Name,
                    Tags = new List<string>(entity.Tags),
                    Position = entity.transform.position,
                    Rotation = entity.transform.rotation,
                    Scale = entity.transform.scale,
                };

                entityStates.Add(state);
            }

            System.Diagnostics.Debug.WriteLine($"[WorldSnapshot] Captured state for {entityStates.Count} entities");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WorldSnapshot] Failed to capture world state: {ex.Message}");
        }
    }

    /// <summary>
    /// Restore the world to this snapshot state
    /// </summary>
    public void RestoreState()
    {
        try
        {
            if (entityStates.Count == 0)
                return;

            // Restore each entity's state by applying stored values back
            for (int i = 0; i < entityStates.Count && i < world._Entities.Count; i++)
            {
                var entity = world._Entities[i];
                var state = entityStates[i];

                // Verify it's the same entity
                if (entity != state.Entity)
                {
                    System.Diagnostics.Debug.WriteLine($"[WorldSnapshot] Entity mismatch during restore at index {i}");
                    continue;
                }

                entity.Name = state.Name;
                entity.Tags = state.Tags.ToArray();
                entity.transform.position = state.Position;
                entity.transform.rotation = state.Rotation;
                entity.transform.scale = state.Scale;
            }

            System.Diagnostics.Debug.WriteLine($"[WorldSnapshot] Restored state for {entityStates.Count} entities");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WorldSnapshot] Failed to restore world state: {ex.Message}");
        }
    }

    /// <summary>
    /// Internal class for storing entity state
    /// </summary>
    private class EntityState
    {
        public Entity Entity { get; set; }
        public string Name { get; set; }
        public List<string> Tags { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
    }
}

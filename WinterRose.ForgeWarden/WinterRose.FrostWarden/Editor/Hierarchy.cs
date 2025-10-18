using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Components;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.ForgeWarden.Worlds;

namespace WinterRose.ForgeWarden.Editor;
/// <summary>
/// A hirarchy window showing all entities within the world
/// </summary>
public class Hierarchy : UIWindow
{
    List<WeakReference<Entity>> entitiesLast;

    /// <summary>
    /// Update method called by framework
    /// </summary>
    internal void UpdateHirarchy()
    {
        World world = Universe.CurrentWorld;
        if (entitiesLast is null || world._Entities.Count != entitiesLast.Count)
            RebuildEntitiesCache();
    }

    private void RebuildEntitiesCache()
    {
        (entitiesLast ??= []).Clear();
        World world = Universe.CurrentWorld;

        foreach (var entity in world._Entities)
            entitiesLast.Add(new WeakReference<Entity>(entity));

        List<Entity> seen = [];
        foreach (Entity e in world._Entities)
            if (!seen.Contains(e))
                AddContent(ConstructTreeNode(e, seen));
    }

    internal Hierarchy() : base("Hierarchy", 400, 600)
    {
    }

    public override void Show()
    {
        List<Entity> seen = [];
        World world = Universe.CurrentWorld;
        if (world is not null)
            foreach (Entity e in world._Entities)
                if(!seen.Contains(e))
                    AddContent(ConstructTreeNode(e, seen));

        base.Show();
    }

    private UITreeNode ConstructTreeNode(Entity e, List<Entity> seen)
    {
        UITreeNode node = new UITreeNode(e.Name, new WeakReference<Entity>(e));
        foreach (Transform t in e.transform.Children)
        {
            seen.Add(t.owner);
            node.AddChild(ConstructTreeNode(t.owner, seen));
        }

        node.DoubleClickInvocation.Subscribe((tree) => new Inspector(new WeakReference<Entity>(e)).Show());
        return node;
    }
}

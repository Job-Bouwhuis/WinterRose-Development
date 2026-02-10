using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden.Components;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Tooltipping;
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
                if (!seen.Contains(e))
                    AddContent(ConstructTreeNode(e, seen));

        base.Show();
    }

    private UITreeNode ConstructTreeNode(Entity e, List<Entity> seen)
    {
        UITreeNode node = new UITreeNode(e.Name, new WeakReference<Entity>(e));
        node.OnTooltipConfigure = Invocation.Create((Tooltip tooltip) =>
        {
            tooltip.AddText($"Name: {e.Name}");
            tooltip.AddText($"Tags: {string.Join(", ", e.Tags)}");
            tooltip.AddText($"Children: {e.transform.Children.Count}");
            tooltip.AddContent(new UIText("Update time: --:--")
            {
                TextProvider = () => $"Update time: {Math.ToStringFixedDecimals(e.updateTimeMs, 3)}ms"
            });
            tooltip.AddContent(new UIText("Render time: --:--")
            {
                TextProvider = () => $"Render time: {Math.ToStringFixedDecimals(e.drawTimeMs, 3)}ms"
            });

            UITreeNode position = new UITreeNode("Transform", e.transform);
            position.AddContent(new UIText("")
            {
                TextProvider = () => $"Position: {e.transform.position.ToStringFixed(3)}"
            });
            position.AddContent(new UIText("")
            {
                TextProvider = () => $"Rotation: {e.transform.rotationEulerDegrees.ToStringFixed(3)}"
            });
            position.AddContent(new UIText("") 
            { 
                TextProvider = () => $"Scale: {e.transform.scale.ToStringFixed(3)}" 
            });
            tooltip.AddContent(position);
            tooltip.AddContent(new UISpacer(60));
            tooltip.AddText("Double click to open inspector!");
        });
        node.Collapse();
        foreach (Transform t in e.transform.Children)
        {
            seen.Add(t.owner);
            node.AddChild(ConstructTreeNode(t.owner, seen));
        }

        node.DoubleClickInvocation.Subscribe((tree) => new Inspector(new WeakReference<Entity>(e)).Show());
        return node;
    }
}

using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.ForgeWarden.Worlds;
using WinterRose.Music;
using WinterRose.Reflection;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.ForgeWarden.Editor;
internal class Inspector : UIWindow
{
    WeakReference<Entity> Entity { get; set; }

    List<WeakReference<Component>> componentsLast;

    List<TrackedValue> trackedValues = [];
    Dictionary<TrackedValue, UIText> ValueContents = [];


    internal Inspector(WeakReference<Entity> entity) : base("Inspector", 400, 600) => Entity = entity;

    public bool IsAlreadyTracked(object o, string memberName)
    {
        return trackedValues.Any(x => x.Value?.Equals(o) ?? false
                                   && x.MemberName == memberName);
    }

    private void Track(TrackedValue v)
    {
        trackedValues.Add(v);
    }

    private void Build()
    {
        List<object> seen = [];
        trackedValues.Clear();
        ValueContents.Clear();
        Contents.Clear();
        if (Entity is not null && Entity.TryGetTarget(out Entity ent))
            foreach (Component c in ent.GetAllComponents())
            {
                UIContent content = CreateObjectTreeNode(c, c.GetType().Name, seen)!;
                AddContent(content);
            }
    }

    public override void Show()
    {
        Build();
        base.Show();
    }

    protected override void Update()
    {
        foreach (TrackedValue trackedVal in trackedValues)
            if (trackedVal.HasValueChanged())
            {
                if (ValueContents.TryGetValue(trackedVal, out UIText text))
                {
                    text.Text = trackedVal.MemberName + " = " + trackedVal.Value?.ToString() ?? "null";
                }
            }
        base.Update();
    }

    private UIContent? CreateObjectTreeNode(object c, string fieldName, List<object> seen)
    {
        if (c is null)
            return new UIText(fieldName + " = null");
        if (IsAlreadyTracked(c, fieldName) || seen.Contains(c))
            return null;

        seen.Add(c);
        TreeNode node = new TreeNode(fieldName);
        node.owner = this;

        if (c is IEnumerable e)
        {
            int i = 0;
            foreach (var it in e)
            {
                UIContent content = CreateObjectTreeNode(it, "Index: " + i, seen);
                if (content is TreeNode childNode)
                    childNode.Collapse();
                node.AddChild(content);
                i++;
            }
            return node;
        }

        ReflectionHelper rh = new(c);
        var members = rh.GetMembers();
        foreach (var m in members)
        {
            if (IsInvalidMember(m))
                continue;
            UIContent? child = null;
            if (!WinterForge.SupportedPrimitives.Contains(m.Type))
            {
                UIContent content = CreateObjectTreeNode(m.GetValue(c), m.Name, seen);
                if (content is TreeNode childNode)
                    childNode.Collapse();
                child = content;
            }
            else
                MakeTrackedUIContent(c, m, node);

            if (child != null)
                node.AddChild(child);
        }
        return node;
    }

    private void MakeTrackedUIContent(object c, MemberData m, TreeNode node)
    {
        TrackedValue val = new TrackedValue(c, m.Name);
        UIText child = new UIText(m.Name + " = " + val.Value?.ToString() ?? "null");
        node.AddChild(child);
        ValueContents.Add(val, child);
        Track(val);
    }

    private bool IsInvalidMember(MemberData member)
    {
        return member.Attributes.Any(x => x is HideAttribute)
            || member.Type.IsAssignableTo(typeof(Invocation))
            || member.Name.Contains("<")
            || member.Type == typeof(World)
            || member.IsStatic;
    }
}

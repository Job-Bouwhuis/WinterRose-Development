using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.EventBusses;

namespace WinterRose.ForgeWarden.UserInterface.Content;
public class UIInvocationContent : UIContent
{
    public Invocation<Rectangle, Vector2>? OnGetSize { get; set; }
    public Invocation<float, float>? OnGetHeight{ get; set; }
    public VoidInvocation<MouseButton>? OnMouseClickedOutsideOfContent { get; set; }
    public VoidInvocation<MouseButton>? OnContentMouseClicked { get; set; }
    public VoidInvocation? OnContentHovered { get; set; }
    public VoidInvocation? OnContentHoverEnd { get; set; }
    public VoidInvocation? OnContentOwnerClosing { get; set; }
    public VoidInvocation? OnSetup {  get; set; }
    public VoidInvocation? OnUpdate { get; set; }

    public override Vector2 GetSize(Rectangle availableArea) 
    {
        return OnGetSize?.Invoke(availableArea) ?? new();
    }
    protected override void Draw(Rectangle bounds) { }
    protected internal override float GetHeight(float maxWidth) 
    {
        return OnGetHeight?.Invoke(maxWidth) ?? 0;
    }
    protected internal override void OnClickedOutsideOfContent(MouseButton button) 
        => OnMouseClickedOutsideOfContent?.Invoke(button);
    protected internal override void OnContentClicked(MouseButton button) 
        => OnContentMouseClicked?.Invoke(button);
    protected internal override void OnHover() => OnContentHovered?.Invoke();
    protected internal override void OnHoverEnd() => OnContentHoverEnd?.Invoke();
    protected internal override void OnOwnerClosing() => OnContentOwnerClosing?.Invoke(); 
    protected internal override void Setup() => OnSetup?.Invoke();
    protected internal override void Update() => OnUpdate?.Invoke();
}

//using ImGuiNET;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
//using System.Text;
//using System.Threading.Tasks;
//using gui = ImGuiNET.ImGui;

//namespace WinterRose.Monogame.UserInterface.ImGuiInterface;

//public sealed class UIWindow : ImGuiItem
//{
//    public string Title { get; set; } = "New Window";
//    public ImGuiChildFlags WindowFlags { get; set; }

//    private List<ImGuiItem> controls = new();
//    private Dictionary<AddOrRemove, ImGuiItem> changingControls = new();
//    private bool rendering = false;


//    public ImGuiItem this[int i]
//    {
//        get => controls[i];
//        set => controls[i] = value;
//    }
//    public UIWindow Add(ImGuiItem item)
//    {
//        item.owner = this;
//        if (rendering)
//        {
//            changingControls.Add(new(true), item);
//            return this;
//        }
//        controls.Add(item);
//        return this;
//    }
//    public UIWindow Remove(ImGuiItem item)
//    {
//        if (rendering)
//        {
//            changingControls.Add(new(false), item);
//            return this;
//        }
//        controls.Remove(item);
//        return this;
//    }
//    public static UIWindow operator +(UIWindow w, ImGuiItem item) => w.Add(item);
//    public static UIWindow operator -(UIWindow w, ImGuiItem item) => w.Remove(item);

//    public override void CreateItem()
//    {
//        rendering = true;

//        if (owner is not null)
//            gui.BeginChild(Title, Size, WindowFlags);
//        else
//            gui.Begin(Title, );

//        if (owner is null)
//        {
//            if (Size != Vector2.Zero)
//                gui.SetWindowSize(Size, ImGuiCond.Appearing);

//            gui.SetWindowPos(Position, ImGuiCond.Appearing);
//        }

//        controls.Foreach(x => x.CreateItem());

//        if (owner is not null)
//            gui.EndChild();
//        else
//            gui.End();

//        rendering = false;
//    }
//    private record AddOrRemove(bool Add);
//}

using ImGuiNET;
using System.Numerics;
using Color = Microsoft.Xna.Framework.Color;

namespace WinterRose.Monogame;

public static class Style
{
    private class C
    {
        Color c;

        public static implicit operator Color(C c) => c.c;
        public static implicit operator C(Vector4 v) => new() { c = new Color(v.X * 255, v.Y * 255, v.Z * 255, v.W * 255) };
        public static implicit operator Vector4(C c) => new(c.c.R / 255f, c.c.G / 255f, c.c.B / 255f, c.c.A / 255f);
        public static implicit operator C(Color c) => new() { c = c };
    }

    private static Vector4 ToVector4(Color c) => (C)c;


    private static ImGuiStylePtr style => ImGui.GetStyle();

    public static Vector2 WindowPadding
    {
        get => style.WindowPadding;
        set => style.WindowPadding = value;
    }
    public static float WindowRounding
    {
        get => style.WindowRounding;
        set => style.WindowRounding = value;
    }
    public static Vector2 FramePadding
    {
        get => style.FramePadding;
        set => style.FramePadding = value;
    }
    public static float FrameRounding
    {
        get => style.FrameRounding;
        set => style.FrameRounding = value;
    }
    public static Vector2 ItemSpacing
    {
        get => style.ItemSpacing;
        set => style.ItemSpacing = value;
    }
    public static Vector2 ItemInnerSpacing
    {
        get => style.ItemInnerSpacing;
        set => style.ItemInnerSpacing = value;
    }
    public static float IndentSpacing
    {
        get => style.IndentSpacing;
        set => style.IndentSpacing = value;
    }
    public static float ScrollbarSize
    {
        get => style.ScrollbarSize;
        set => style.ScrollbarSize = value;
    }
    public static float ScrollbarRounding
    {
        get => style.ScrollbarRounding;
        set => style.ScrollbarRounding = value;
    }
    public static float GrabMinSize
    {
        get => style.GrabMinSize;
        set => style.GrabMinSize = value;
    }
    public static float GrabRounding
    {
        get => style.GrabRounding;
        set => style.GrabRounding = value;
    }

    public static Color TextColor
    {
        get => (C)style.Colors[(int)ImGuiCol.Text];
        set => style.Colors[(int)ImGuiCol.Text] = ToVector4(value);
    }
    public static Color TextDisabledColor
    {
        get => (C)style.Colors[(int)ImGuiCol.TextDisabled];
        set => style.Colors[(int)ImGuiCol.TextDisabled] = ToVector4(value); 
    }
    public static Color WindowBgColor
    {
        get => (C)style.Colors[(int)ImGuiCol.WindowBg];
        set => style.Colors[(int)ImGuiCol.WindowBg] = ToVector4(value);
    }
    public static Color ChildBgColor
    {
        get => (C)style.Colors[(int)ImGuiCol.ChildBg];
        set => style.Colors[(int)ImGuiCol.ChildBg] = ToVector4(value);
    }
    public static Color PopupBgColor
    {
        get => (C)style.Colors[(int)ImGuiCol.PopupBg];
        set => style.Colors[(int)ImGuiCol.PopupBg] = ToVector4(value);
    }
    public static Color BorderColor
    {
        get => (C)style.Colors[(int)ImGuiCol.Border];
        set => style.Colors[(int)ImGuiCol.Border] = ToVector4(value);
    }
    public static Color BorderShadowColor
    {
        get => (C)style.Colors[(int)ImGuiCol.BorderShadow];
        set => style.Colors[(int)ImGuiCol.BorderShadow] = ToVector4(value);
    }
    public static Color FrameBgColor
    {
        get => (C)style.Colors[(int)ImGuiCol.FrameBg];
        set => style.Colors[(int)ImGuiCol.FrameBg] = ToVector4(value);
    }
    public static Color FrameBgHoveredColor
    {
        get => (C)style.Colors[(int)ImGuiCol.FrameBgHovered];
        set => style.Colors[(int)ImGuiCol.FrameBgHovered] = ToVector4(value);
    }
    public static Color FrameBgActiveColor
    {
        get => (C)style.Colors[(int)ImGuiCol.FrameBgActive];
        set => style.Colors[(int)ImGuiCol.FrameBgActive] = ToVector4(value);
    }
    public static Color TitleBgColor
    {
        get => (C)style.Colors[(int)ImGuiCol.TitleBg];
        set => style.Colors[(int)ImGuiCol.TitleBg] = ToVector4(value);
    }
    public static Color TitleBgActiveColor
    {
        get => (C)style.Colors[(int)ImGuiCol.TitleBgActive];
        set => style.Colors[(int)ImGuiCol.TitleBgActive] = ToVector4(value);
    }
    public static Color TitleBgCollapsedColor
    {
        get => (C)style.Colors[(int)ImGuiCol.TitleBgCollapsed];
        set => style.Colors[(int)ImGuiCol.TitleBgCollapsed] = ToVector4(value);
    }
    public static Color MenuBarBgColor
    {
        get => (C)style.Colors[(int)ImGuiCol.MenuBarBg];
        set => style.Colors[(int)ImGuiCol.MenuBarBg] = ToVector4(value);
    }
    public static Color ScrollbarBgColor
    {
        get => (C)style.Colors[(int)ImGuiCol.ScrollbarBg];
        set => style.Colors[(int)ImGuiCol.ScrollbarBg] = ToVector4(value);
    }
    public static Color ScrollbarGrabColor
    {
        get => (C)style.Colors[(int)ImGuiCol.ScrollbarGrab];
        set => style.Colors[(int)ImGuiCol.ScrollbarGrab] = ToVector4(value);
    }
    public static Color ScrollbarGrabHoveredColor
    {
        get => (C)style.Colors[(int)ImGuiCol.ScrollbarGrabHovered];
        set => style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = ToVector4(value);
    }
    public static Color ScrollbarGrabActiveColor
    {
        get => (C)style.Colors[(int)ImGuiCol.ScrollbarGrabActive];
        set => style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = ToVector4(value);
    }
    public static Color CheckMarkColor
    {
        get => (C)style.Colors[(int)ImGuiCol.CheckMark];
        set => style.Colors[(int)ImGuiCol.CheckMark] = ToVector4(value);
    }
    public static Color SliderGrabColor
    {
        get => (C)style.Colors[(int)ImGuiCol.SliderGrab];
        set => style.Colors[(int)ImGuiCol.SliderGrab] = ToVector4(value);
    }
    public static Color SliderGrabActiveColor
    {
        get => (C)style.Colors[(int)ImGuiCol.SliderGrabActive];
        set => style.Colors[(int)ImGuiCol.SliderGrabActive] = ToVector4(value);
    }
    public static Color ButtonColor
    {
        get => (C)style.Colors[(int)ImGuiCol.Button];
        set => style.Colors[(int)ImGuiCol.Button] = ToVector4(value);
    }
    public static Color ButtonHoveredColor
    {
        get => (C)style.Colors[(int)ImGuiCol.ButtonHovered];
        set => style.Colors[(int)ImGuiCol.ButtonHovered] = ToVector4(value);
    }
    public static Color ButtonActiveColor
    {
        get => (C)style.Colors[(int)ImGuiCol.ButtonActive];
        set => style.Colors[(int)ImGuiCol.ButtonActive] = ToVector4(value);
    }
    public static Color HeaderColor
    {
        get => (C)style.Colors[(int)ImGuiCol.Header];
        set => style.Colors[(int)ImGuiCol.Header] = ToVector4(value);
    }
    public static Color HeaderHoveredColor
    {
        get => (C)style.Colors[(int)ImGuiCol.HeaderHovered];
        set => style.Colors[(int)ImGuiCol.HeaderHovered] = ToVector4(value);
    }
    public static Color HeaderActiveColor
    {
        get => (C)style.Colors[(int)ImGuiCol.HeaderActive];
        set => style.Colors[(int)ImGuiCol.HeaderActive] = ToVector4(value);
    }
    public static Color SeparatorColor
    {
        get => (C)style.Colors[(int)ImGuiCol.Separator];
        set => style.Colors[(int)ImGuiCol.Separator] = ToVector4(value);
    }
    public static Color SeparatorHoveredColor
    {
        get => (C)style.Colors[(int)ImGuiCol.SeparatorHovered];
        set => style.Colors[(int)ImGuiCol.SeparatorHovered] = ToVector4(value);
    }
    public static Color SeparatorActiveColor
    {
        get => (C)style.Colors[(int)ImGuiCol.SeparatorActive];
        set => style.Colors[(int)ImGuiCol.SeparatorActive] = ToVector4(value);
    }
    public static Color ResizeGripColor
    {
        get => (C)style.Colors[(int)ImGuiCol.ResizeGrip];
        set => style.Colors[(int)ImGuiCol.ResizeGrip] = ToVector4(value);
    }
    public static Color ResizeGripHoveredColor
    {
        get => (C)style.Colors[(int)ImGuiCol.ResizeGripHovered];
        set => style.Colors[(int)ImGuiCol.ResizeGripHovered] =  ToVector4(value)    ;
    }
    public static Color ResizeGripActiveColor
    {
        get => (C)style.Colors[(int)ImGuiCol.ResizeGripActive];
        set => style.Colors[(int)ImGuiCol.ResizeGripActive] = ToVector4(value)  ;
    }
    public static Color TabColor
    {
        get => (C)style.Colors[(int)ImGuiCol.Tab];
        set => style.Colors[(int)ImGuiCol.Tab] = ToVector4(value)   ;
    }
    public static Color TabHoveredColor
    {
        get => (C)style.Colors[(int)ImGuiCol.TabHovered];
        set => style.Colors[(int)ImGuiCol.TabHovered] = ToVector4(value);
    }
    public static Color TabActiveColor
    {
        get => (C)style.Colors[(int)ImGuiCol.TabActive];
        set => style.Colors[(int)ImGuiCol.TabActive] = ToVector4(value);
    }
    public static Color TabUnfocusedColor
    {
        get => (C)style.Colors[(int)ImGuiCol.TabUnfocused];
        set => style.Colors[(int)ImGuiCol.TabUnfocused] = ToVector4(value);
    }
    public static Color TabUnfocusedActiveColor
    {
        get => (C)style.Colors[(int)ImGuiCol.TabUnfocusedActive];
        set => style.Colors[(int)ImGuiCol.TabUnfocusedActive] = ToVector4(value);
    }
    public static Color DockingPreviewColor
    {
        get => (C)style.Colors[(int)ImGuiCol.DockingPreview];
        set => style.Colors[(int)ImGuiCol.DockingPreview] = ToVector4(value);
    }
    public static Color DockingEmptyBgColor
    {
        get => (C)style.Colors[(int)ImGuiCol.DockingEmptyBg];
        set => style.Colors[(int)ImGuiCol.DockingEmptyBg] = ToVector4(value);
    }
    public static Color PlotLinesColor
    {
        get => (C)style.Colors[(int)ImGuiCol.PlotLines];
        set => style.Colors[(int)ImGuiCol.PlotLines] = ToVector4(value);
    }
    public static Color PlotLinesHoveredColor
    {
        get => (C)style.Colors[(int)ImGuiCol.PlotLinesHovered];
        set => style.Colors[(int)ImGuiCol.PlotLinesHovered] = ToVector4(value);
    }
    public static Color PlotHistogramColor
    {
        get => (C)style.Colors[(int)ImGuiCol.PlotHistogram];
        set => style.Colors[(int)ImGuiCol.PlotHistogram] = ToVector4(value);
    }
    public static Color PlotHistogramHoveredColor
    {
        get => (C)style.Colors[(int)ImGuiCol.PlotHistogramHovered];
        set => style.Colors[(int)ImGuiCol.PlotHistogramHovered] = ToVector4(value);
    }
    public static Color TextSelectedBgColor
    {
        get => (C)style.Colors[(int)ImGuiCol.TextSelectedBg];
        set => style.Colors[(int)ImGuiCol.TextSelectedBg] = ToVector4(value);
    }
    public static Color DragDropTargetColor
    {
        get => (C)style.Colors[(int)ImGuiCol.DragDropTarget];
        set => style.Colors[(int)ImGuiCol.DragDropTarget] = ToVector4(value);
    }
    public static Color NavHighlightColor
    {
        get => (C)style.Colors[(int)ImGuiCol.NavHighlight];
        set => style.Colors[(int)ImGuiCol.NavHighlight] = ToVector4(value);
    }
    public static Color NavWindowingHighlightColor
    {
        get => (C)style.Colors[(int)ImGuiCol.NavWindowingHighlight];
        set => style.Colors[(int)ImGuiCol.NavWindowingHighlight] = ToVector4(value);
    }
    public static Color NavWindowingDimBgColor
    {
        get => (C)style.Colors[(int)ImGuiCol.NavWindowingDimBg];
        set => style.Colors[(int)ImGuiCol.NavWindowingDimBg] = ToVector4(value);
    }
    public static Color ModalWindowDimBgColor
    {
        get => (C)style.Colors[(int)ImGuiCol.ModalWindowDimBg];
        set => style.Colors[(int)ImGuiCol.ModalWindowDimBg] = ToVector4(value);
    }

    public static void ApplyDefault()
    {
        WindowPadding = new(15, 15);
        WindowRounding = 5.0f;
        FramePadding = new(5, 5);
        FrameRounding = 4.0f;
        ItemSpacing = new(12, 8);
        ItemInnerSpacing = new(8, 6);
        IndentSpacing = 25.0f;
        ScrollbarSize = 15.0f;
        ScrollbarRounding = 9.0f;
        GrabMinSize = 5.0f;
        GrabRounding = 3.0f;

        style.ChildRounding = 4.0f;
        style.ChildBorderSize = 1.0f;
        style.PopupRounding = 3.0f;
        style.PopupBorderSize = 1.0f;
        style.FrameBorderSize = 0.0f;
        style.WindowBorderSize = 1.0f;
        style.TabBorderSize = 0.0f;

        // change loadbar filled color to green
        style.Colors[(int)ImGuiCol.PlotHistogram] = new(0, 255, 0, 255);

        TextColor = new(255, 255, 255, 255);
        TextDisabledColor = new(255, 255, 255, 128);
        WindowBgColor = new(37, 37, 37, 255);
        ChildBgColor = new(69, 69, 69, 255);
        PopupBgColor = new(37, 37, 37, 255);
        BorderColor = new(51, 51, 100, 255);
        BorderShadowColor = new(0, 0, 255, 0);
        FrameBgColor = new(0, 0, 0, 255);
        FrameBgHoveredColor = new(0, 0, 0, 0);
        FrameBgActiveColor = new(51, 51, 51, 0);
        TitleBgColor = new(37, 37, 37, 255);
        TitleBgActiveColor = new(51, 51, 51, 255);
        TitleBgCollapsedColor = new(0, 0, 0, 0);
        MenuBarBgColor = new(37, 37, 37, 255);
        ScrollbarBgColor = new(37, 37, 37, 255);
        ScrollbarGrabColor = new(79, 79, 79, 255);
        ScrollbarGrabHoveredColor = new(105, 105, 105, 255);
        ScrollbarGrabActiveColor = new(130, 130, 130, 255);
        CheckMarkColor = new(79, 255, 79, 255);
        SliderGrabColor = new(79, 79, 79, 255);
        SliderGrabActiveColor = new(105, 105, 105, 255);
        ButtonColor = new(79, 79, 79, 255);
        ButtonHoveredColor = new(105, 105, 130, 255);
        ButtonActiveColor = new(130, 130, 130, 255);
        HeaderColor = new(255, 79, 79, 255);
        HeaderHoveredColor = new(105, 105, 105, 255);
        HeaderActiveColor = new(130, 130, 130, 255);
        SeparatorColor = new(51, 51, 150, 255);
        SeparatorHoveredColor = new(0, 0, 0, 0);
        SeparatorActiveColor = new(0, 0, 0, 0);
        ResizeGripColor = new(105, 105, 105, 255);
        ResizeGripHoveredColor = new(130, 130, 130, 255);
        ResizeGripActiveColor = new(150, 150, 150, 255);
        TabColor = new(20, 20, 20, 255);
        TabHoveredColor = new(105, 105, 105, 255);
        TabActiveColor = new(51, 51, 70, 255);
        TabUnfocusedColor = new(20, 20, 20, 255);
        TabUnfocusedActiveColor = new(51, 51, 51, 255);
        DockingPreviewColor = new(51, 51, 80, 255);
        DockingEmptyBgColor = new(0, 0, 0, 0);
        PlotLinesColor = new(79, 79, 79, 255);
        PlotLinesHoveredColor = new(105, 105, 105, 255);
        PlotHistogramColor = new(79, 79, 79, 255);
        PlotHistogramHoveredColor = new(105, 105, 105, 255);
        TextSelectedBgColor = new(51, 51, 51, 255);
        DragDropTargetColor = new(255, 255, 0, 230);
        NavHighlightColor = new(51, 51, 51, 255);
        NavWindowingHighlightColor = new(255, 255, 255, 179);
        NavWindowingDimBgColor = new(204, 204, 204, 51);
        ModalWindowDimBgColor = new(104, 254, 104, 51);
    }
}
using ImGuiNET;
using System.Numerics;
using Color = WinterRose.ImGuiApps.Color;

namespace WinterRose.ImGuiApps;

public static class Style
{
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
        get => style.Colors[(int)ImGuiCol.Text];
        set => style.Colors[(int)ImGuiCol.Text] = value;
    }
    public static Color TextDisabledColor
    {
        get => style.Colors[(int)ImGuiCol.TextDisabled];
        set => style.Colors[(int)ImGuiCol.TextDisabled] = value;
    }
    public static Color WindowBgColor
    {
        get => style.Colors[(int)ImGuiCol.WindowBg];
        set => style.Colors[(int)ImGuiCol.WindowBg] = value;
    }
    public static Color ChildBgColor
    {
        get => style.Colors[(int)ImGuiCol.ChildBg];
        set => style.Colors[(int)ImGuiCol.ChildBg] = value;
    }
    public static Color PopupBgColor
    {
        get => style.Colors[(int)ImGuiCol.PopupBg];
        set => style.Colors[(int)ImGuiCol.PopupBg] = value;
    }
    public static Color BorderColor
    {
        get => style.Colors[(int)ImGuiCol.Border];
        set => style.Colors[(int)ImGuiCol.Border] = value;
    }
    public static Color BorderShadowColor
    {
        get => style.Colors[(int)ImGuiCol.BorderShadow];
        set => style.Colors[(int)ImGuiCol.BorderShadow] = value;
    }
    public static Color FrameBgColor
    {
        get => style.Colors[(int)ImGuiCol.FrameBg];
        set => style.Colors[(int)ImGuiCol.FrameBg] = value;
    }
    public static Color FrameBgHoveredColor
    {
        get => style.Colors[(int)ImGuiCol.FrameBgHovered];
        set => style.Colors[(int)ImGuiCol.FrameBgHovered] = value;
    }
    public static Color FrameBgActiveColor
    {
        get => style.Colors[(int)ImGuiCol.FrameBgActive];
        set => style.Colors[(int)ImGuiCol.FrameBgActive] = value;
    }
    public static Color TitleBgColor
    {
        get => style.Colors[(int)ImGuiCol.TitleBg];
        set => style.Colors[(int)ImGuiCol.TitleBg] = value;
    }
    public static Color TitleBgActiveColor
    {
        get => style.Colors[(int)ImGuiCol.TitleBgActive];
        set => style.Colors[(int)ImGuiCol.TitleBgActive] = value;
    }
    public static Color TitleBgCollapsedColor
    {
        get => style.Colors[(int)ImGuiCol.TitleBgCollapsed];
        set => style.Colors[(int)ImGuiCol.TitleBgCollapsed] = value;
    }
    public static Color MenuBarBgColor
    {
        get => style.Colors[(int)ImGuiCol.MenuBarBg];
        set => style.Colors[(int)ImGuiCol.MenuBarBg] = value;
    }
    public static Color ScrollbarBgColor
    {
        get => style.Colors[(int)ImGuiCol.ScrollbarBg];
        set => style.Colors[(int)ImGuiCol.ScrollbarBg] = value;
    }
    public static Color ScrollbarGrabColor
    {
        get => style.Colors[(int)ImGuiCol.ScrollbarGrab];
        set => style.Colors[(int)ImGuiCol.ScrollbarGrab] = value;
    }
    public static Color ScrollbarGrabHoveredColor
    {
        get => style.Colors[(int)ImGuiCol.ScrollbarGrabHovered];
        set => style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = value;
    }
    public static Color ScrollbarGrabActiveColor
    {
        get => style.Colors[(int)ImGuiCol.ScrollbarGrabActive];
        set => style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = value;
    }
    public static Color CheckMarkColor
    {
        get => style.Colors[(int)ImGuiCol.CheckMark];
        set => style.Colors[(int)ImGuiCol.CheckMark] = value;
    }
    public static Color SliderGrabColor
    {
        get => style.Colors[(int)ImGuiCol.SliderGrab];
        set => style.Colors[(int)ImGuiCol.SliderGrab] = value;
    }
    public static Color SliderGrabActiveColor
    {
        get => style.Colors[(int)ImGuiCol.SliderGrabActive];
        set => style.Colors[(int)ImGuiCol.SliderGrabActive] = value;
    }
    public static Color ButtonColor
    {
        get => style.Colors[(int)ImGuiCol.Button];
        set => style.Colors[(int)ImGuiCol.Button] = value;
    }
    public static Color ButtonHoveredColor
    {
        get => style.Colors[(int)ImGuiCol.ButtonHovered];
        set => style.Colors[(int)ImGuiCol.ButtonHovered] = value;
    }
    public static Color ButtonActiveColor
    {
        get => style.Colors[(int)ImGuiCol.ButtonActive];
        set => style.Colors[(int)ImGuiCol.ButtonActive] = value;
    }
    public static Color HeaderColor
    {
        get => style.Colors[(int)ImGuiCol.Header];
        set => style.Colors[(int)ImGuiCol.Header] = value;
    }
    public static Color HeaderHoveredColor
    {
        get => style.Colors[(int)ImGuiCol.HeaderHovered];
        set => style.Colors[(int)ImGuiCol.HeaderHovered] = value;
    }
    public static Color HeaderActiveColor
    {
        get => style.Colors[(int)ImGuiCol.HeaderActive];
        set => style.Colors[(int)ImGuiCol.HeaderActive] = value;
    }
    public static Color SeparatorColor
    {
        get => style.Colors[(int)ImGuiCol.Separator];
        set => style.Colors[(int)ImGuiCol.Separator] = value;
    }
    public static Color SeparatorHoveredColor
    {
        get => style.Colors[(int)ImGuiCol.SeparatorHovered];
        set => style.Colors[(int)ImGuiCol.SeparatorHovered] = value;
    }
    public static Color SeparatorActiveColor
    {
        get => style.Colors[(int)ImGuiCol.SeparatorActive];
        set => style.Colors[(int)ImGuiCol.SeparatorActive] = value;
    }
    public static Color ResizeGripColor
    {
        get => style.Colors[(int)ImGuiCol.ResizeGrip];
        set => style.Colors[(int)ImGuiCol.ResizeGrip] = value;
    }
    public static Color ResizeGripHoveredColor
    {
        get => style.Colors[(int)ImGuiCol.ResizeGripHovered];
        set => style.Colors[(int)ImGuiCol.ResizeGripHovered] = value;
    }
    public static Color ResizeGripActiveColor
    {
        get => style.Colors[(int)ImGuiCol.ResizeGripActive];
        set => style.Colors[(int)ImGuiCol.ResizeGripActive] = value;
    }
    public static Color TabColor
    {
        get => style.Colors[(int)ImGuiCol.Tab];
        set => style.Colors[(int)ImGuiCol.Tab] = value;
    }
    public static Color TabHoveredColor
    {
        get => style.Colors[(int)ImGuiCol.TabHovered];
        set => style.Colors[(int)ImGuiCol.TabHovered] = value;
    }
    public static Color TabActiveColor
    {
        get => style.Colors[(int)ImGuiCol.TabActive];
        set => style.Colors[(int)ImGuiCol.TabActive] = value;
    }
    public static Color TabUnfocusedColor
    {
        get => style.Colors[(int)ImGuiCol.TabUnfocused];
        set => style.Colors[(int)ImGuiCol.TabUnfocused] = value;
    }
    public static Color TabUnfocusedActiveColor
    {
        get => style.Colors[(int)ImGuiCol.TabUnfocusedActive];
        set => style.Colors[(int)ImGuiCol.TabUnfocusedActive] = value;
    }
    public static Color DockingPreviewColor
    {
        get => style.Colors[(int)ImGuiCol.DockingPreview];
        set => style.Colors[(int)ImGuiCol.DockingPreview] = value;
    }
    public static Color DockingEmptyBgColor
    {
        get => style.Colors[(int)ImGuiCol.DockingEmptyBg];
        set => style.Colors[(int)ImGuiCol.DockingEmptyBg] = value;
    }
    public static Color PlotLinesColor
    {
        get => style.Colors[(int)ImGuiCol.PlotLines];
        set => style.Colors[(int)ImGuiCol.PlotLines] = value;
    }
    public static Color PlotLinesHoveredColor
    {
        get => style.Colors[(int)ImGuiCol.PlotLinesHovered];
        set => style.Colors[(int)ImGuiCol.PlotLinesHovered] = value;
    }
    public static Color PlotHistogramColor
    {
        get => style.Colors[(int)ImGuiCol.PlotHistogram];
        set => style.Colors[(int)ImGuiCol.PlotHistogram] = value;
    }
    public static Color PlotHistogramHoveredColor
    {
        get => style.Colors[(int)ImGuiCol.PlotHistogramHovered];
        set => style.Colors[(int)ImGuiCol.PlotHistogramHovered] = value;
    }
    public static Color TextSelectedBgColor
    {
        get => style.Colors[(int)ImGuiCol.TextSelectedBg];
        set => style.Colors[(int)ImGuiCol.TextSelectedBg] = value;
    }
    public static Color DragDropTargetColor
    {
        get => style.Colors[(int)ImGuiCol.DragDropTarget];
        set => style.Colors[(int)ImGuiCol.DragDropTarget] = value;
    }
    public static Color NavHighlightColor
    {
        get => style.Colors[(int)ImGuiCol.NavHighlight];
        set => style.Colors[(int)ImGuiCol.NavHighlight] = value;
    }
    public static Color NavWindowingHighlightColor
    {
        get => style.Colors[(int)ImGuiCol.NavWindowingHighlight];
        set => style.Colors[(int)ImGuiCol.NavWindowingHighlight] = value;
    }
    public static Color NavWindowingDimBgColor
    {
        get => style.Colors[(int)ImGuiCol.NavWindowingDimBg];
        set => style.Colors[(int)ImGuiCol.NavWindowingDimBg] = value;
    }
    public static Color ModalWindowDimBgColor
    {
        get => style.Colors[(int)ImGuiCol.ModalWindowDimBg];
        set => style.Colors[(int)ImGuiCol.ModalWindowDimBg] = value;
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
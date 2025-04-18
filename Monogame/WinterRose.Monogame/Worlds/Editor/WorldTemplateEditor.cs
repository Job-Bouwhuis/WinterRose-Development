using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WinterRose.FileManagement;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame.Worlds;

/// <summary>
/// A little editor window for editing world templates within the game.
/// 
/// upon first opening the editor, it may freeze for a few moments, this is because it is fetching all the world templates.
/// </summary>
public static class WorldEditor
{
    static bool show = false;
    /// <summary>
    /// Whether or not the editor should be shown.
    /// </summary>
    public static bool Show
    {
        get => show;
        set => show = value;
    }

    private static string[] worldTemplates;
    private static string[] worldTemplateFileNames;
    private static int width = 0;
    private static int selectedItem = 0;
    private static string templateName = "";
    private static string tip = "Name of your template";
    private static bool isEditing = false;
    private static string editString = "";
    private static string editingFileContent = "";
    private static bool firstSetup = true;

    private static void Setup()
    {
        if (!Directory.Exists("Content/WorldTemplates"))
            Directory.CreateDirectory("Content/WorldTemplates");

        FileInfo[] templates = new DirectoryInfo("Content/WorldTemplates").GetFiles();
        DirectoryInfo saves = new("Content/Saves");
        if (saves.Exists)
            templates = [.. saves.GetFiles(), .. templates];

        worldTemplates = new string[templates.Length];
        foreach (int i in templates.Length)
            worldTemplates[i] = FileManager.PathFrom(templates[i].FullName, "Content");

        List<string> templateclasses = new();
        foreach (var type in TypeWorker.FindTypesWithAttribute<WorldTemplateAttribute>())
        {
            var instance = (WorldTemplate)Activator.CreateInstance(type);
            var attributes = type.GetCustomAttributes<EditorBrowsableAttribute>();
            if (attributes.Any(x => x.state == EditorBrowsableState.Never))
                continue;

#if !DEBUG
            if(attributes.Any(x => x.state == EditorBrowsableState.Debug))
                continue;
#endif

            templateclasses.Add(instance.Name);
        }
        worldTemplates = [.. worldTemplates, .. templateclasses];

        foreach (var s in worldTemplates)
        {
            var v = gui.CalcTextSize(s);
            if (v.X > width)
                width = v.X.CeilingToInt();
        }

        worldTemplateFileNames = worldTemplates.Select(x => FileManager.PathLast(x)).ToArray();
    }
    internal static void RenderLayout()
    {
        if (firstSetup)
        {
            Setup();
            firstSetup = false;
        }

        gui.Begin("World Template Editor");

        gui.SetWindowSize(new System.Numerics.Vector2(400, 300), ImGuiNET.ImGuiCond.Appearing);
        string[] strings = worldTemplates.Select(x => FileManager.PathLast(x)).ToArray();
        gui.Combo("Worlds", ref selectedItem, strings, worldTemplates.Length);
        if (gui.Button("Load Selected World"))
        {
            Universe.CurrentWorld = null;
            Debug.ClearExceptions();
            LoadWorld();
        }

        gui.SameLine();
        if (gui.Button("Save Template"))
        {
            gui.OpenPopup("override template");
        }
        if (gui.BeginPopup("override template", ImGuiNET.ImGuiWindowFlags.AlwaysAutoResize))
        {
            gui.Text($"Are you sure you want to override {worldTemplates[selectedItem]}?");
            gui.Separator();
            if (gui.Button("Yes"))
            {
                WorldTemplateCreator.CreateSave(worldTemplates[selectedItem], Universe.CurrentWorld);
                Setup();
                gui.CloseCurrentPopup();
            }
            gui.SameLine();
            if (gui.Button("No"))
            {
                gui.CloseCurrentPopup();
            }
            gui.EndPopup();
        }
        gui.SameLine();
        if (gui.Button("Delete Template"))
        {
            gui.OpenPopup("Delete Template", ImGuiNET.ImGuiPopupFlags.MouseButtonLeft);
        }
        if (gui.BeginPopup("Delete Template", ImGuiNET.ImGuiWindowFlags.AlwaysAutoResize))
        {
            gui.Text($"Are you sure you want to delete {worldTemplates[selectedItem]}?");
            gui.Separator();
            if (gui.Button("Yes"))
            {
                File.Delete(worldTemplates[selectedItem]);
                Setup();
                gui.CloseCurrentPopup();
            }
            gui.SameLine();
            if (gui.Button("No"))
            {
                gui.CloseCurrentPopup();
            }
            gui.EndPopup();
        }
        gui.Separator();
        gui.InputTextWithHint("Template Name", tip, ref templateName, 100);
        gui.Separator();

        if (gui.Button("New template"))
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                tip = "Template name cannot be empty";
            }
            else
            {
                FileManager.CreateOrOpenFile($"Content\\WorldTemplates\\{templateName}.MonoWorld").Close();
                Setup();
                tip = "Name of your template";
            }
        }
        gui.SameLine();
        if (gui.Button("Save Template As New"))
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                tip = "Template name cannot be empty";
            }
            else
            {
                WorldTemplateCreator.CreateSave($"Content\\WorldTemplates\\{templateName}.MonoWorld", Universe.CurrentWorld);
                Setup();
                tip = "Name of your template";
            }
        }
        gui.SameLine();
        if (!isEditing && gui.Button("Edit template"))
        {
            editString = File.ReadAllText(worldTemplates[selectedItem]);
            editingFileContent = editString;
            isEditing = true;
        }
        if (!Hirarchy.Show)
            if (gui.Button("Show Hirarchy"))
                Hirarchy.Show = true;

        gui.Spacing();
        gui.Spacing();
        if (gui.Button("Close"))
        {
            Show = false;
        }

        if (isEditing)
        {
            gui.Begin("Edit Template", ImGuiNET.ImGuiWindowFlags.AlwaysAutoResize);
            gui.SetWindowSize(new(400, 400));
            gui.Text("Edit Template");
            gui.Separator();
            EditGUI();
            gui.End();
        }

        gui.End();
    }
    private static void EditGUI()
    {
        gui.InputTextMultiline(
            string.Join('.', worldTemplateFileNames[selectedItem].Split()[new Index(0)..new Index(1, true)]),
            ref editString,
            100000,
            new(450, 500),
            ImGuiNET.ImGuiInputTextFlags.AllowTabInput);

        gui.Separator();
        if (gui.Button("Save", new(100, 50)))
        {
            FileManager.Write(worldTemplates[selectedItem], editString, true);
            editingFileContent = editString;
        }
        gui.SameLine();

        if (editString != editingFileContent)
        {
            if (gui.Button("discard", new(100, 50)))
                gui.OpenPopup("Discard?", ImGuiNET.ImGuiPopupFlags.MouseButtonLeft);
        }
        else if (gui.Button("Close", new(100, 50)))
            isEditing = false;
        gui.SameLine();

        if (gui.Button("Save & Reload world", new(100, 50)))
        {
            FileManager.Write(worldTemplates[selectedItem], editString, true);
            editingFileContent = editString;
            Universe.CurrentWorld = null;
            LoadWorld();
            editString = File.ReadAllText(worldTemplates[selectedItem]);
            editingFileContent = editString;
            isEditing = true;
        }

        if (gui.BeginPopup("Discard?", ImGuiNET.ImGuiWindowFlags.AlwaysAutoResize))
        {
            gui.Text($"Are you sure you want to discard your changes?");
            gui.Separator();
            if (gui.Button("Save before closing"))
            {
                FileManager.Write(worldTemplates[selectedItem], editString, true);
                isEditing = false;
                gui.CloseCurrentPopup();
            }
            if (gui.Button("Yes"))
            {
                isEditing = false;
                gui.CloseCurrentPopup();
            }
            gui.SameLine();
            if (gui.Button("No"))
            {
                gui.CloseCurrentPopup();
            }
            gui.EndPopup();
        }
    }
    private static void LoadWorld()
    {
        Debug.AutoClear = false;

        if (worldTemplates[selectedItem].EndsWith(".MonoWorld"))
        {
            World world = World.FromTemplate(worldTemplates[selectedItem]);
            Universe.CurrentWorld = world;
            return;
        }

        Type t = TypeWorker.FindTypesWithAttribute<WorldTemplateAttribute>().First(x => ((WorldTemplate)Activator.CreateInstance(x)).Name == worldTemplates[selectedItem]);
        Universe.CurrentWorld = new(t);
        Debug.AutoClear = true;
    }
}

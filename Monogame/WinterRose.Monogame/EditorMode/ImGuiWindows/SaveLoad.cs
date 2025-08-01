﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame.EditorMode.ImGuiWindows;

internal static class SaveLoad
{
    static bool opened = false;

    public static void Render()
    {
        if (!opened) return;

        gui.Begin("Save or load world", ref opened);
        gui.SetWindowSize(new(800, 900));

        // first column, saving
        SaveWorldColumn();

        gui.Text("");
        gui.Separator();
        gui.Text("");

        gui.SetWindowFontScale(1.2f);
        gui.Text("Load your world!");
        gui.SetWindowFontScale(1);

        // second column, loading
        LoadWorldColumn();
        gui.End();
    }

    private static void SaveWorldColumn()
    {
        gui.SetWindowFontScale(1.2f);
        gui.Text("Save your world!");
        gui.SetWindowFontScale(1);
        gui.SameLine();
        if(gui.Button("Save"))
        {
            Universe.CurrentWorld!.SaveTemplate();
            opened = false;
        }
    }

    private static void LoadWorldColumn(DirectoryInfo dir = null)
    {
        dir ??= new DirectoryInfo("Content/WorldTemplates");
        if (!dir.Exists)
            dir.Create();
        foreach (var subDir in dir.GetDirectories())
        {
            if (subDir.GetFileSystemInfos().Length == 0)
                continue;
            gui.TreeNode(subDir.Name);
            LoadWorldColumn(subDir);
            gui.TreePop();
        }
        
        foreach (var worldfile in dir.GetFiles("*.world"))
        {
            gui.Text(Path.GetFileNameWithoutExtension(worldfile.Name));
            gui.SameLine();
            gui.Button("Load");
            if(gui.BeginItemTooltip())
            {
                Input.BlockInputIfUISelected = false;
                if(Input.GetMouseDown(MouseButton.Left, true))
                {
                    Editor.Opened = false;
                    Windows.OpenConsole();
                    Console.Title = $"Loading world '{Path.GetFileNameWithoutExtension(worldfile.Name)}'";
                    Universe.CurrentWorld = World.FromTemplateFile(worldfile.Name);
                    Windows.CloseConsole();
                    Editor.Opened = true;
                    return;
                }
                Input.BlockInputIfUISelected = true;
                gui.EndTooltip();
            }

            gui.SameLine();

            Color defaultButtonHoverColor = Style.ButtonHoveredColor;
            Style.ButtonHoveredColor = Color.Red;
            if(gui.Button("Delete"))
            {
                Windows.DialogResult result = Windows.MessageBox("Are you sure you want to delete world " + Path.GetFileNameWithoutExtension(worldfile.Name) + "?",
                    "WARNING", Windows.MessageBoxButtons.YesNo, Windows.MessageBoxIcon.Question);
                if(result == Windows.DialogResult.Yes)
                {
                    worldfile.Delete();
                }
            }
            Style.ButtonHoveredColor = defaultButtonHoverColor;
        }
    }

    public static void Open() => opened = true;
    public static void Close() => opened = false;
}

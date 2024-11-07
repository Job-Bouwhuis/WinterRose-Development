using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FileManagement;
using WinterRose.Reflection;
using Microsoft.Xna.Framework;

namespace WinterRose.Monogame.EditorMode.BuildInDisplays
{
    internal class SpriteDisplay : EditorDisplay<Sprite>
    {
        private bool isCreatingNewSprite = false;
        private bool renderSpriteInEditorWindow = false;

        private bool circleTexture = false;
        private string selectedSpriteFileName = "";
        private int newSpriteWidth = 0;
        private int newSpriteHeight = 0;
        private System.Numerics.Vector4 newSpriteColor = new(1, 1, 1, 1);

        public override void Render(ref Sprite sprite, MemberData field, object obj)
        {
            if (isCreatingNewSprite)
            {

                if (field.HasAttribute<ReadonlyAttribute>() || !field.CanWrite)
                {
                    gui.Text("Field is readonly.");
                    if (gui.Button("Ok"))
                        isCreatingNewSprite = false;
                    return;
                }
                CreateNew(ref sprite);
                return;
            }

            if (sprite is null)
                gui.Text("No sprite.");

            else if (sprite.TexturePath != null)
            {
                gui.Text($"Sprite name: '{sprite.TexturePath}'");
            }
            else
            {
                gui.Text("Sprite is custom made.");
            }



            if (gui.Button("Assign new texture"))
                isCreatingNewSprite = true;
        }

        private void CreateNew(ref Sprite sprite)
        {
            gui.Text("Choose a new texture");

            var allFiles = FindAllFilesFrom(MonoUtils.Content.RootDirectory);
            if (allFiles.Count > 0)
            {
                gui.SeparatorText("TextureList");
                gui.Text("Selecting one submits right away");
            }

            gui.Indent();
            foreach (var file in allFiles)
            {
                Texture2D tex = null;
                string contentName = "";

                if (!file.EndsWith(".png") && !file.EndsWith(".jpg") && !file.EndsWith(".xnb"))
                    continue;

                if (file.EndsWith(".xnb"))
                {
                    try
                    {
                        contentName = FileManager.PathFrom(file, "content");
                        string[] split = contentName.Split('.');
                        contentName = string.Join('.', split, 0, split.Length - 1);

                        if (contentName.StartsWith("Content"))
                        {
                            contentName = contentName.Replace('/', '\\');
                            split = contentName.Split('\\');

                            contentName = string.Join('\\', split, 1, split.Length - 1);
                        }

                        tex = MonoUtils.Content.Load<Texture2D>(contentName);
                        if (tex is null)
                            continue;
                    }
                    catch
                    {
                        continue;
                    }
                }
                string path = Path.GetFileName(file);

                if (gui.Selectable(path))
                {
                    if (path.EndsWith(".xnb"))
                        sprite.SetTexture(MonoUtils.Content.Load<Texture2D>(contentName));
                    else
                    {
                        Sprite newSprite = new(file);
                        sprite.SetTexture(newSprite);
                    }

                }
            }
            gui.Unindent();

            gui.Text("");
            gui.SeparatorText("Create a new texture");

            gui.Checkbox("Circle texture", ref circleTexture);

            gui.InputTextWithHint("Name", "e.g: MyTexture", ref selectedSpriteFileName, 150);
            if (!circleTexture)
            {
                gui.InputInt("Width", ref newSpriteWidth);
                gui.InputInt("Height", ref newSpriteHeight);

            }
            else
            {
                gui.InputInt("Radius", ref newSpriteWidth);
            }

            gui.Text("");

            gui.ColorEdit4("Color", ref newSpriteColor, ImGuiNET.ImGuiColorEditFlags.AlphaBar);

            gui.Text("");

            if (gui.Button("Submit"))
            {
                var v = newSpriteColor;
                Color color = new(v.X, v.Y, v.Z, v.W);

                Sprite newSprite;

                if (!circleTexture)
                    newSprite = new(newSpriteWidth, newSpriteHeight, color);
                else
                    newSprite = Sprite.Circle(newSpriteWidth, color);

                sprite.SetTexture(newSprite);
                isCreatingNewSprite = false;
            }
            gui.SameLine();
            if (gui.Button("Cancel"))
                isCreatingNewSprite = false;

        }

        private static List<string> FindAllFilesFrom(string path)
        {
            List<string> files = [];
            foreach (var file in Directory.GetFiles(path))
            {
                files.Add(file);
            }
            foreach (var dir in Directory.GetDirectories(path))
            {
                files.AddRange(FindAllFilesFrom(dir));
            }
            return files;
        }
    }
}

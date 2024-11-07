using ImGuiNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Worlds;
using flag = ImGuiNET.ImGuiWindowFlags;
using Microsoft.Xna.Framework;
using System.IO;
using Vector4 = System.Numerics.Vector4;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using WinterRose.FileManagement;
using WinterRose.Monogame.EditorMode;
using System.Collections.Concurrent;
using System.Security.AccessControl;
using System.ComponentModel;

namespace WinterRose.Monogame
{
    /// <summary>
    /// World Hirarchy window.
    /// </summary>
    public static class Hirarchy
    {
        /// <summary>
        /// Whether or not the window is shown.
        /// </summary>
        public static bool Show
        {
            get => show;
            set => show = value;
        }
        private static bool showWorldProperties = false;
        private static bool show = false;
        private static string componentData = "";
        private static bool valueContextDialogUp = false;
        private static string valueContextDialogData = "";
        private static bool submittedContextValue = false;
        private static string valueContextHint = "";
        private static int selectedIndex = 0;
        static bool renderChunkGrid = false;
        static bool filterOutEmpty = true;
        private static int chunkSize = 0;
        static int chunkGenerationRadius;
        static Vector4 newSpriteColor;
        static int newSpriteWidth;
        static int newSpriteHeight;
        static string selectedSpriteFileName = "";
        static bool circleTexture = false;
        static int cameraIndex = -1;
        static WorldObject? objectDragging = null;
        private static bool newParentSet;
        private static bool doubleClickStarted;
        private static readonly List<Type> AllowedForFieldTree = [
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
            typeof(Quaternion),
            typeof(IndexF),
            typeof(ColorRange),
            typeof(ValueRange),

        ];



        /// <summary>
        /// A list of types that are allowed to be evaluated for fields in the hirarchy window. These types will have their fields shown as seperate nodes in the hirarchy window.
        /// </summary>
        //public static readonly List<Type> AllowedForFieldTree =
        //[
        //    typeof(Vector2),
        //    typeof(Vector3),
        //    typeof(Color),
        //    typeof(Rectangle),
        //    typeof(Vector2I),
        //    typeof(RangeF),
        //    typeof(ColorRange),
        //    typeof(ValueRange),
        //    typeof(ValueRangePoint),
        //    typeof(ColorRangePoint),
        //    typeof(IndexF)
        //];

        // Hirarchy window
        internal static void RenderLayout()
        {
            if (!Show)
                return;

            World world = Universe.CurrentWorld;

            gui.Begin("World Hirargy", ref show);
            gui.SetWindowSize(new(400, 500), ImGuiCond.Once);

            if (gui.Button("World Properties"))
            {
                showWorldProperties = true;
            }
            if (showWorldProperties)
            {
                CreateWorldPropertiesWindow();
            }
            gui.SameLine();
            if (Universe.CurrentWorld is not null && !Editor.Opened && gui.Button("Create new object"))
            {
                valueContextDialogUp = true;
                valueContextHint = "New Value";
                valueContextDialogData = "";
                gui.OpenPopup("NewObject", ImGuiPopupFlags.MouseButtonLeft);
            }
            ContextDialog("NewObject", () =>
            {
                gui.InputTextWithHint("Object name", valueContextHint, ref valueContextDialogData, 100);
                if (gui.Button("Submit"))
                {
                    if (string.IsNullOrWhiteSpace(valueContextDialogData))
                    {
                        valueContextHint = "Value may not be empty or only whitespaces";
                        return;
                    }
                    Universe.CurrentWorld.CreateObject(valueContextDialogData);
                    gui.CloseCurrentPopup();
                }
            });
            gui.SameLine();
            if (Universe.CurrentWorld is not null)
            {
                bool clicked;
                if (Editor.Opened)
                    clicked = gui.Button("Close Editor");
                else
                    clicked = gui.Button("Open Editor");

                if (clicked)
                    Editor.ToggleEditor();
            }
            if (world is null)
            {
                gui.Text($"No world loaded.");
                gui.End();
                return;
            }

            gui.BeginChild("Hirachy", gui.GetContentRegionAvail(), true);

            if (Editor.Opened)
            {
                EditorOpenedView(world);
                gui.EndChild();
                gui.End();
                return;
            }

            for (int i = 0; i < world.ObjectCount; i++)
            {
                var obj = world[i];

                if (obj.Name == "Editor Camera (DO NOT DELETE)")
                    continue;
                if (obj.Name == "Debug Object (DO NOT DELETE)")
                    continue;

                if (gui.TreeNode(obj.Name))
                {
                    if (!gui.IsPopupOpen("", ImGuiPopupFlags.AnyPopup))
                        componentData = "";

                    if (gui.IsItemClicked(ImGuiMouseButton.Right))
                        gui.OpenPopup(obj.Name);

                    ContextDialog(obj.Name, () =>
                    {
                        if (gui.Button("Duplicate"))
                        {
                            Universe.CurrentWorld.Duplicate(obj, obj.Name + " (Copy)");
                        }
                        gui.SameLine();
                        gui.Text("Creates an exact copy of this object.");

                        gui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(1, 0, 0, 1));
                        gui.PushStyleColor(ImGuiCol.ButtonHovered, new System.Numerics.Vector4(0.7f, 0, 0, 1));
                        gui.PushStyleColor(ImGuiCol.ButtonActive, new System.Numerics.Vector4(0.6f, 0, 0, 0.8f));
                        if (gui.Button("Delete Object"))
                        {
                            obj.Destroy();
                        }
                        gui.PopStyleColor();
                        gui.PopStyleColor();
                        gui.PopStyleColor();

                        if (gui.BeginItemTooltip())
                        {
                            gui.Text("Deletes this object and all of its children.");
                            gui.EndTooltip();
                        }

                        gui.SeparatorText("Components");

                        gui.InputTextWithHint("New Component", "e.g: SpriteRenderer(50, 50, \"#FFFFFF\")", ref componentData, 150);
                        gui.SetWindowSize(new(200, 80), ImGuiCond.Once);
                        if (gui.Button("Add"))
                        {
                            new WorldTemplateLoader(Universe.CurrentWorld).ParseComponent(obj, componentData, new());
                            var c = obj.FetchComponents().Last();
                            c.CallAwake();
                            c.CallStart();
                        }
                    });

                    if (gui.TreeNode("Chunks"))
                    {
                        // show the list of chunks this object occupies
                        foreach (var chunk in obj.ChunkPositionData.Chunks)
                        {
                            gui.Text($"Chunk: {chunk.ChunkPosition}");
                        }
                        gui.TreePop();
                    }

                    ComponentTreeNodes(obj);
                    if (gui.Selectable($"Flag = \"{obj.Flag}\""))
                        gui.OpenPopup("Flag");

                    ContextDialog("Flag", () =>
                    {
                        gui.InputTextWithHint("New Component", "e.g: Ground", ref componentData, 150);
                        gui.SetWindowSize(new(200, 80), ImGuiCond.Once);
                        if (gui.Button("Set Flag"))
                        {
                            obj.Flag = componentData;
                            gui.CloseCurrentPopup();
                        }
                        gui.SameLine();
                        if (gui.Button("Clear Flag"))
                        {
                            obj.Flag = "";
                            gui.CloseCurrentPopup();
                        }
                    });

                    gui.TreePop();
                }
            }
            gui.EndChild();
            gui.End();
        }


        private static Dictionary<WorldObject, bool> objectExpandedStates = [];
        private static bool objectDraggingReleased;

        private static void EditorOpenedView(World world)
        {
            if (objectExpandedStates.Count is 0 || objectExpandedStates.Count != world.ObjectCount)
            {
                objectExpandedStates.Clear();
                world.Objects.Foreach(x => objectExpandedStates.Add(x, false));
            }

            List<WorldObject> parents = [];

            for (int i = 0; i < world.ObjectCount; i++)
            {
                var obj = world[i];
                if (obj.transform.parent != null)
                    continue;
                parents.Add(obj);
            }

            if (!newParentSet && objectDraggingReleased)
            {
                objectDragging!.transform.parent = null;
                objectDraggingReleased = false;
                objectDragging = null;
            }
            if (newParentSet)
            {
                newParentSet = false;
                objectDragging = null;
                objectDraggingReleased = false;
            }

            if (objectDragging != null)
            {
                DoObjectDragging();
            }

            foreach (var obj in parents)
            {
                CreateEditorObjectTree(obj);
            }
        }

        private static void DoObjectDragging()
        {
            gui.BeginTooltip();
            gui.Text(objectDragging.Name);
            gui.Text("Click on another object to set the new parent, \nor click on an empty space in the hirarchy to set no parent.\n 'Escape' to cancel");
            gui.EndTooltip();

            if (doubleClickStarted)
            {
                if (!gui.IsMouseDown(ImGuiMouseButton.Left))
                    doubleClickStarted = false;
            }
            else if (gui.IsKeyDown(ImGuiKey.Escape))
            {
                objectDragging = null;
                newParentSet = false;
                objectDraggingReleased = false;
            }
            else if (gui.IsMouseDown(ImGuiMouseButton.Left))
            {
                objectDraggingReleased = true;
            }
        }

        private static void CreateEditorObjectTree(WorldObject obj)
        {
            if (obj == null)
            {
                gui.TextColored(new(1, 0, 0, 1), "ERROR: OBJECT NULL.");
                return;
            }

            bool expanded;
            try
            {
                bool? exp = objectExpandedStates.FirstOrDefault(x => x.Key.Name == obj.Name).Value;
                if (exp == null)
                    throw new KeyNotFoundException($"object {obj.Name} not found");
                expanded = exp.Value;
            }
            catch (KeyNotFoundException ex)
            {
                return;
            }


            if (obj.transform.HasChildren)
            {
                if (gui.ArrowButton("collapse_" + obj.Name, expanded ? ImGuiDir.Down : ImGuiDir.Right))
                    objectExpandedStates[obj] = !expanded;
                gui.SameLine();
            }

            DisplayObjectName(obj);

            if (expanded)
            {
                gui.Indent();
                foreach (var child in obj.transform)
                {
                    CreateEditorObjectTree(child.owner);
                }
                gui.Unindent();
            }
        }

        private static void DisplayObjectName(WorldObject obj)
        {
            bool returnColor = false;
            Color textColor = Style.TextColor;
            if (EditorMain.SelectedObject == obj)
            {
                Style.TextColor = Color.Aquamarine;
                returnColor = true;
            }
            if (gui.Selectable(obj.Name))
            {
                EditorMain.SelectedObject = obj;
            }
            if (returnColor)
                Style.TextColor = textColor;

            if (gui.BeginItemTooltip())
            {
                if (objectDragging == null)
                    gui.Text("Double click to set new parent");
                if (objectDraggingReleased)
                {
                    SetDraggedObjectParent(obj);
                }
                else if (gui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    objectDragging = obj;
                    doubleClickStarted = true;
                }
                gui.EndTooltip();
            }

        }

        private static void SetDraggedObjectParent(WorldObject obj)
        {
            if (objectDragging == obj)
                return;

            if (objectDragging!.transform.parent == obj.transform)
                return;

            objectDragging.transform.parent = obj.transform;
            objectDragging = null;
            newParentSet = true;
        }

        /// <summary>
        /// An easier way to make popup dialogs in the hirarchy window.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="contentProvider"></param>
        /// <param name="OnCancel"></param>
        /// <returns></returns>
        public static bool ContextDialog(string label, Action contentProvider, Action? OnCancel = null)
        {
            bool begin = gui.BeginPopup(label, flag.AlwaysAutoResize);
            if (begin)
            {
                contentProvider();
                gui.EndPopup();
            }
            else
                OnCancel?.Invoke();
            return begin;
        }
        /// <summary>
        /// Generates a object tree in the hirarchy window for this component.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="onRightClick"></param>
        /// <param name="OnCancel"></param>
        public static void ComponentTree(ObjectComponent obj, Action? onRightClick = null, Action? OnCancel = null)
        {
            string name = obj.GetType().Name;
            if (!obj.Enabled)
                name += " (Dissabled)";
            object o = obj;
            ObjectTree(name, o, onRightClick, OnCancel);
        }
        /// <summary>
        /// Generates a object tree in the hirarchy window for this object.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="o"></param>
        /// <param name="onRightClick"></param>
        /// <param name="OnCancel"></param>
        public static void ObjectTree(string label, object? o, Action? onRightClick = null, Action? OnCancel = null)
        {
            if (gui.TreeNode(label))
            {
                if (o is null)
                {
                    gui.Text("Null reference.");
                    return;
                }
                if (gui.IsItemClicked(ImGuiMouseButton.Right) && onRightClick is not null)
                {
                    gui.OpenPopup(label);
                }
                ContextDialog(label, onRightClick);

                gui.Indent();

                if (o is Sprite s)
                {
                    NewSpriteDialog(s);
                }

                FieldTreeNodes(o);
                PropertyTreeNodes(o);
                gui.Unindent();
                gui.TreePop();
            }
            else
                OnCancel?.Invoke();


            if (o is Sprite sprite)
            {
                gui.SameLine();
                NewSpriteDialog(sprite);
            }
        }

        /// <summary>
        /// If confirmed by the user, will override the current <paramref name="sprite"/>'s texture data with new data.
        /// </summary>
        /// <param name="sprite"></param>
        public static void NewSpriteDialog(Sprite sprite)
        {
            if (gui.Button("Assign New Texture"))
            {
                if (!gui.IsPopupOpen("AssignTexture"))
                {
                    gui.OpenPopup("AssignTexture");
                    newSpriteColor = new(1, 1, 1, 1);
                    newSpriteWidth = 20;
                    newSpriteHeight = 20;
                    selectedSpriteFileName = "";
                    circleTexture = false;
                }
            }
            ContextDialog("AssignTexture", () =>
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

                gui.ColorEdit4("Color", ref newSpriteColor);

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
                    gui.CloseCurrentPopup();
                }
                gui.SameLine();
                if (gui.Button("Cancel"))
                {
                    gui.CloseCurrentPopup();
                }
            });
        }
        /// <summary>
        /// Displays a default editable value meant for the hirarchy window. Exposed for use in <see cref="EditorDisplay{T}"/> implementations
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="info"></param>
        public static void DisplayDefaultEditableValue(object obj, PropertyInfo info)
        {
            object? val = null;
            if (obj is not IList)
                val = info.GetValue(obj);

            if (CheckIfObjectMayNestTree(ref val, info.Name))
            {
                ObjectTree(info.Name, val);
                if (submittedContextValue && !info.PropertyType.IsClass && !info.PropertyType.IsInterface)
                {
                    info.SetValue(obj, val);
                    submittedContextValue = false;
                    Universe.RequestRender = true;
                }
                return;
            }

            if (!valueContextDialogUp)
            {
                if (!gui.IsPopupOpen("", ImGuiPopupFlags.AnyPopup))
                    valueContextDialogData = "";
                if (val is bool && info.CanWrite)
                {
                    bool v = TypeWorker.CastPrimitive<bool>(val);
                    if (gui.Selectable($"{info.Name} = {val}", v))
                    {
                        v = !v;
                        info.SetValue(obj, v);
                    }
                    return;
                }
                else if (val is float && info.CanWrite)
                {
                    float f = TypeWorker.CastPrimitive<float>(val);
                    if (gui.Selectable($"{info.Name} = {val}"))
                    {
                        gui.OpenPopup(info.Name);
                        valueContextDialogUp = true;
                        valueContextDialogData = val.ToString();
                    }
                }
                else if (val is int && info.CanWrite)
                {
                    int f = TypeWorker.CastPrimitive<int>(val);
                    if (gui.Selectable($"{info.Name} = {val}"))
                    {
                        gui.OpenPopup(info.Name);
                        valueContextDialogUp = true;
                        valueContextDialogData = val.ToString();
                    }
                }
                else if (val is byte && info.CanWrite)
                {
                    byte b = TypeWorker.CastPrimitive<byte>(val);
                    if (gui.Selectable($"{info.Name} = {val}"))
                    {
                        gui.OpenPopup(info.Name);
                        valueContextDialogUp = true;
                        valueContextDialogData = val.ToString();
                    }
                }
                else if (val is Enum e && info.CanWrite)
                {
                    if (gui.Selectable($"{info.Name} = {e.GetType().Name}.{val}"))
                    {
                        gui.OpenPopup(info.Name);
                        valueContextDialogUp = true;
                        valueContextDialogData = val.ToString();
                    }
                }
                else
                    gui.Text($"{info.Name} = {val}");
                return;
            }
            else
            {
                if (val is float)
                {
                    float f = TypeWorker.CastPrimitive<float>(val);
                    ContextDialog(info.Name, () =>
                    {
                        gui.InputText("new value", ref valueContextDialogData, 100);
                        if (gui.Button("Submit"))
                            if (TypeWorker.TryCastPrimitive(valueContextDialogData, out f))
                            {
                                info.SetValue(obj, f);
                                gui.CloseCurrentPopup();
                            }
                    }, () =>
                    {
                        if (!gui.IsPopupOpen("", ImGuiPopupFlags.AnyPopup))
                            valueContextDialogUp = false;
                    });
                }
                if (val is int)
                {
                    int f = TypeWorker.CastPrimitive<int>(val);
                    ContextDialog(info.Name, () =>
                    {
                        gui.InputText("new value", ref valueContextDialogData, 100);
                        if (gui.Button("Submit"))
                            if (TypeWorker.TryCastPrimitive(valueContextDialogData, out f))
                            {
                                info.SetValue(obj, f);
                                gui.CloseCurrentPopup();
                                if (!info.PropertyType.IsClass && !info.PropertyType.IsInterface)
                                    submittedContextValue = true;
                            }
                    }, () =>
                    {
                        if (!gui.IsPopupOpen("", ImGuiPopupFlags.AnyPopup))
                            valueContextDialogUp = false;
                    });
                }
                if (val is byte)
                {
                    byte f = TypeWorker.CastPrimitive<byte>(val);
                    ContextDialog(info.Name, () =>
                    {
                        gui.InputText("new value", ref valueContextDialogData, 100);
                        if (gui.Button("Submit"))
                            if (TypeWorker.TryCastPrimitive(valueContextDialogData, out f))
                            {
                                info.SetValue(obj, f);
                                gui.CloseCurrentPopup();
                                if (!info.PropertyType.IsClass && !info.PropertyType.IsInterface)
                                    submittedContextValue = true;
                            }
                    }, () =>
                    {
                        if (!gui.IsPopupOpen("", ImGuiPopupFlags.AnyPopup))
                            valueContextDialogUp = false;
                    });
                }
                if (val is Enum e)
                {
                    ContextDialog(info.Name, () =>
                    {
                        var options = Enum.GetNames(e.GetType());
                        gui.Combo("New Value", ref selectedIndex, options, options.Length);
                        if (gui.Button("Submit"))
                        {
                            var newVal = Enum.Parse(e.GetType(), options[selectedIndex]);
                            info.SetValue(obj, newVal);
                            gui.CloseCurrentPopup();
                        }
                    });
                }
            }

            gui.Text($"{info.Name} = {val}");
        }
        /// <summary>
        /// Displays a default editable value meant for the hirarchy window. Exposed for use in <see cref="EditorDisplay{T}"/> implementations
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="info"></param>
        public static void DisplayDefaultEditableValues(object? obj, FieldInfo info)
        {
            var val = info.GetValue(obj);
            if (CheckIfObjectMayNestTree(ref val, info.Name))
            {
                ObjectTree(info.Name, val);
                return;
            }

            if (!valueContextDialogUp)
            {
                if (!gui.IsPopupOpen("", ImGuiPopupFlags.AnyPopup))
                    valueContextDialogData = "";
                if (val is bool && !info.IsInitOnly)
                {
                    bool v = TypeWorker.CastPrimitive<bool>(val);
                    if (gui.Selectable($"{info.Name} = {val}", v))
                    {
                        v = !v;
                        info.SetValue(obj, v);
                    }
                    return;
                }
                else if (val is float && !info.IsInitOnly)
                {
                    float f = TypeWorker.CastPrimitive<float>(val);
                    if (gui.Selectable($"{info.Name} = {val}"))
                    {
                        gui.OpenPopup(info.Name);
                        valueContextDialogUp = true;
                        valueContextDialogData = val.ToString();
                    }
                }
                else if (val is int && !info.IsInitOnly)
                {
                    int f = TypeWorker.CastPrimitive<int>(val);
                    if (gui.Selectable($"{info.Name} = {val}"))
                    {
                        gui.OpenPopup(info.Name);
                        valueContextDialogUp = true;
                        valueContextDialogData = val.ToString();
                    }
                }
                else if (val is byte && !info.IsInitOnly)
                {
                    byte b = TypeWorker.CastPrimitive<byte>(val);
                    if (gui.Selectable($"{info.Name} = {val}"))
                    {
                        gui.OpenPopup(info.Name);
                        valueContextDialogUp = true;
                        valueContextDialogData = val.ToString();
                    }
                }
                else if (val is Enum e && !info.IsInitOnly)
                {
                    if (gui.Selectable($"{info.Name} = {e.GetType().Name}.{val}"))
                    {
                        gui.OpenPopup(info.Name);
                        valueContextDialogUp = true;
                        valueContextDialogData = val.ToString();
                    }
                }
                else gui.Text($"{info.Name} = {val}");
                return;
            }
            else
            {
                if (val is float)
                {
                    float f = TypeWorker.CastPrimitive<float>(val);
                    ContextDialog(info.Name, () =>
                    {
                        gui.InputText("new value", ref valueContextDialogData, 100);
                        if (gui.Button("Submit"))
                            if (TypeWorker.TryCastPrimitive(valueContextDialogData, out f))
                            {
                                if (!info.FieldType.IsByRef)
                                {
                                    TypedReference r = __makeref(obj);
                                    info.SetValueDirect(r, f);
                                    val = (float)info.GetValueDirect(r);
                                    submittedContextValue = true;
                                    gui.CloseCurrentPopup();
                                    return;
                                }
                                info.SetValue(obj, f);
                                gui.CloseCurrentPopup();
                            }
                    }, () =>
                    {
                        if (!gui.IsPopupOpen("", ImGuiPopupFlags.AnyPopup))
                            valueContextDialogUp = false;
                    });
                }
                if (val is int)
                {
                    int f = TypeWorker.CastPrimitive<int>(val);
                    ContextDialog(info.Name, () =>
                    {
                        gui.InputText("new value", ref valueContextDialogData, 100);
                        if (gui.Button("Submit"))
                            if (TypeWorker.TryCastPrimitive(valueContextDialogData, out f))
                            {
                                if (!info.FieldType.IsByRef)
                                {
                                    TypedReference r = __makeref(obj);
                                    info.SetValueDirect(r, f);
                                    val = (int)info.GetValueDirect(r);
                                    submittedContextValue = true;
                                    gui.CloseCurrentPopup();
                                    return;
                                }
                                info.SetValue(obj, f);
                                gui.CloseCurrentPopup();
                            }
                    }, () =>
                    {
                        if (!gui.IsPopupOpen("", ImGuiPopupFlags.AnyPopup))
                            valueContextDialogUp = false;
                    });
                }
                if (val is byte)
                {
                    byte f = TypeWorker.CastPrimitive<byte>(val);
                    ContextDialog(info.Name, () =>
                    {
                        gui.InputText("new value", ref valueContextDialogData, 100);
                        if (gui.Button("Submit"))
                            if (TypeWorker.TryCastPrimitive(valueContextDialogData, out f))
                            {
                                if (!info.FieldType.IsByRef)
                                {
                                    TypedReference r = __makeref(obj);
                                    info.SetValueDirect(r, f);
                                    val = (int)info.GetValueDirect(r);
                                    submittedContextValue = true;
                                    gui.CloseCurrentPopup();
                                    return;
                                }
                                info.SetValue(obj, f);
                                gui.CloseCurrentPopup();
                            }
                    }, () =>
                    {
                        if (!gui.IsPopupOpen("", ImGuiPopupFlags.AnyPopup))
                            valueContextDialogUp = false;
                    });
                }
                if (val is Enum e)
                {
                    ContextDialog(info.Name, () =>
                    {
                        var options = Enum.GetNames(e.GetType());
                        gui.Combo("New Value", ref selectedIndex, options, options.Length);
                        if (gui.Button("Submit"))
                        {
                            var newVal = Enum.Parse(e.GetType(), options[selectedIndex]);
                            info.SetValue(obj, newVal);
                            gui.CloseCurrentPopup();
                        }
                    });
                }
            }
            gui.Text($"{info.Name} = {val}");
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
        private static void ComponentTreeNodes(WorldObject obj)
        {
            foreach (var comp in obj.FetchComponents())
                ComponentTree(comp, () =>
                {
                    if (gui.Button("Remove Component"))
                    {
                        obj.RemoveComponent(comp);
                    }
                });
        }
        private static void FieldTreeNodes(object? obj)
        {
            if (obj is null)
            {
                gui.Text("Null reference.");
                return;
            }
            if (obj is IList list)
            {
                if(list is null)
                {
                    gui.Text("NULL list");
                    return;
                }
                if(list.Count is 0)
                {
                    gui.Text("Empty list");
                    return;
                }
                bool mayNest = false;
                object? oo = list[0];
                if (oo is not null)
                    mayNest = CheckIfObjectMayNestTree(ref oo, "List");

                int i = 0;
                foreach (object o in list)
                {
                    if (mayNest)
                        ObjectTree(o.GetType().Name + $"_{i}", o);
                    else
                        gui.Text(o.ToString());

                    i++;
                }
                return;
            }
            var publics = obj.GetType().GetFields(bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                .Where(x => x.GetCustomAttributes<HiddenAttribute>().Count() == 0);

            var privates = obj.GetType().GetFields(bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .Where(x => x.GetCustomAttributes<ShowAttribute>().Count() != 0).ToArray();

            privates.Foreach(x => publics = publics.Append(x).ToArray());
            var fields = publics.ToArray();
            foreach (FieldInfo info in fields)
            {
                DisplayDefaultEditableValues(obj, info);
            }
        }
        private static void PropertyTreeNodes(object obj)
        {
            if (obj is IList list)
            {
                return;
            }

            var publics = obj.GetType().GetProperties(bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                .Where(x => x.GetCustomAttributes<HiddenAttribute>().Count() == 0);

            var privates = obj.GetType().GetProperties(bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .Where(x => x.GetCustomAttributes<ShowAttribute>().Count() != 0).ToArray();

            privates.Foreach(x => publics = publics.Append(x));
            var properties = publics.ToArray();

            foreach (PropertyInfo info in properties)
            {
                DisplayDefaultEditableValue(obj, info);
            }
        }
        private static bool CheckIfObjectMayNestTree(ref object? o, string label)
        {
            if (o is null)
            {
                o = "Null Reference";
                return false;
            }

            Type t = o.GetType();
            if (o is string || t.IsClass && label.Contains("owner") && !(o is Transform && (label.Contains("transform") || label.Contains("parent") || label.Contains("world"))))
                return false;

            if (t.IsClass)
                return true;

            if (AllowedForFieldTree.Contains(t))
                return true;

            return t.IsClass;
        }
        private static void CreateWorldPropertiesWindow()
        {
            World world = Universe.CurrentWorld;

            gui.Begin("World Properties", ref showWorldProperties);

            gui.Text($"Name: {world.Name}");
            gui.Text($"Object Count: {world.ObjectCount}");
            gui.Text($"Component Count: {world.ComponentCount}");
            gui.Text($"Update Time: {world.TotalUpdateTime.TotalMilliseconds}ms");
            gui.Text($"Render Time: {world.TotalDrawTime.TotalMilliseconds}ms");
            gui.Text($"Running Slowly: {Time.IsRunningSlowly}");
            gui.Text($"Desired Time: {MonoUtils.TargetFramerate}");

            gui.Separator();

            gui.Text($"Updates/s: {world.UpdatesPerSecond}");
            gui.Text($"Draws/s: {world.DrawsPerSecond}");

            gui.Separator();

            gui.InputInt("Camera Index", ref cameraIndex);
            if (gui.Button("Submit"))
            {
                int CameraCount = world.UpdateCameraIndexes();

                if (cameraIndex < -1) // -1 is no camera. framework falls back to the default "camera" in Monogame which is at 0, 0
                    cameraIndex = -1;

                if (cameraIndex >= CameraCount)
                    cameraIndex = CameraCount - 1;

                Universe.RequestRender = true;

                Application.Current.CameraIndex = cameraIndex;
            }

            gui.Separator();

            gui.Text("Chunks:");
            gui.Text($"  Total: {world.WorldChunkGrid.Count}");
            if (gui.Button(renderChunkGrid ? "Stop rendering chunk grid" : "Render chunk grid"))
                renderChunkGrid = !renderChunkGrid;

            foreach (var data in world.WorldChunkGrid.AllChunks)
            {
                if (renderChunkGrid)
                {
                    // calculate the rectangle of the chunk
                    var chunkRect = new Rectangle(data.Value.ChunkPosition.X, data.Value.ChunkPosition.Y, WorldGrid.ChunkSize, WorldGrid.ChunkSize);
                    Debug.DrawRectangle(chunkRect, Color.Red);
                }
            }

            gui.SeparatorText("Chunk Specifics");

            if (gui.Selectable($"Chunk size: {WorldGrid.ChunkSize}"))
            {
                gui.OpenPopup("Change Chunk Size");
                chunkSize = WorldGrid.ChunkSize;
            }
            if (gui.BeginPopup("Change Chunk Size"))
            {
                gui.InputInt("New Chunk Size", ref chunkSize);
                if (gui.Button("Submit"))
                {
                    WorldGrid.ChunkSize = chunkSize;
                    Universe.CurrentWorld?.WorldChunkGrid.ResetChunks();
                    gui.CloseCurrentPopup();
                }
                gui.EndPopup();
            }

            if (gui.Selectable($"Chunk Load Distance: {WorldGrid.ChunkGenerationRadius}"))
            {
                gui.OpenPopup("Change Chunk Load Distance");
                chunkGenerationRadius = WorldGrid.ChunkGenerationRadius;
            }
            if (gui.BeginPopup("Change Chunk Load Distance"))
            {
                gui.InputInt("New Chunk Load Distance", ref chunkGenerationRadius);
                if (gui.Button("Submit"))
                {
                    WorldGrid.ChunkGenerationRadius = chunkGenerationRadius;
                    gui.CloseCurrentPopup();
                }
                gui.EndPopup();
            }

            if (gui.Button("Clear Chunks"))
            {
                Universe.CurrentWorld?.WorldChunkGrid.ResetChunks();
            }
            if (gui.BeginItemTooltip())
            {
                gui.Text("Clears all chunks in the world\n\n" +
                    "The next frame, all objects in the world\n" +
                    "will request new chunks right away");

                gui.EndTooltip();
            }

            gui.Separator();
            gui.Text("");

            if (gui.TreeNode("Chunk Grid"))
            {
                gui.Separator();
                if (gui.Button(filterOutEmpty ? "Show empty chunks" : "Hide empty chunks"))
                    filterOutEmpty = !filterOutEmpty;

                gui.Separator();
                foreach (var data in world.WorldChunkGrid.AllChunks)
                {
                    if (data.Value.Count == 0 && filterOutEmpty)
                        continue;
                    gui.Indent();
                    gui.Text($"{data.Key}");

                    gui.Indent();

                    gui.Text($"Objects: {data.Value.Count}");

                    gui.Unindent();
                    gui.Unindent();
                    gui.Separator();
                }
            }

            gui.End();
        }

    }
}

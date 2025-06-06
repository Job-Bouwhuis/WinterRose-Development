using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using WinterRose.Reflection;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using WinterRose.Monogame.Worlds;
using WinterRose.Monogame.EditorMode.BuildInDisplays;
using SharpDX.Direct3D9;
using System.Collections;
using System.Windows.Forms;
using WinterRose.WinterForgeSerializing.Workers;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.Monogame.EditorMode
{
    internal static partial class EditorMain
    {
        private static string objectName = "New Object";
        private static string componentSearch = "";

        static List<Type> allComponents;
        static Type? selectedComponentToAdd;
        static object[]? newComponentArgs;
        static string[] newComponentArgInputs;
        static ConstructorInfo? newComponentSelectedConstructor;
        static ObjectComponent? newComponentInstance;

        static ObjectComponent? inspectorComponent;
        static object inspectorComponentObject;

        static List<string> componentValueEdits = [];

        private static List<EditorDisplay> displayClasses = [];

        static EditorMain()
        {
            // Add all components to the allComponents list
            allComponents = [];
            ConcurrentStack<Type> types = new();
            Parallel.ForEach(AppDomain.CurrentDomain.GetAssemblies(), assembly =>
            {
                foreach (var type in assembly.GetTypes())
                    types.Push(type);
            });

            foreach (Type type in types)
            {
                if (type.IsSubclassOf(typeof(ObjectComponent)) || type == typeof(ObjectComponent))
                    if (!type.IsAbstract)
                        allComponents.Add(type);
                if (type.IsSubclassOf(typeof(EditorDisplay)) && type != typeof(EditorDisplay<>))
                    displayClasses.Add((EditorDisplay)Activator.CreateInstance(type));
            }

            allComponents = allComponents.OrderBy(t => t.Name).ToList();
        }

        private static void Setup()
        {
            if (SelectedObject is not null)
                objectName = selectedObject.Name;
            selectedComponentToAdd = null;
            inspectorComponent = null;
            inspectorComponentObject = null;
        }

        private static void SelectedObjectGUI()
        {
            if (selectedObject.IsDestroyed)
            {
                selectedObject = null;
                return;
            }
            bool open = true;
            gui.Begin("Selected Object", ref open, ImGuiWindowFlags.HorizontalScrollbar);
            if(objectGUILocSet)
            {
                gui.SetWindowSize(
                    new System.Numerics.Vector2(500, 
                                        MonoUtils.ScreenSize.Y - 100), 
                    ImGuiCond.Appearing);
                gui.SetWindowPos(new System.Numerics.Vector2(), ImGuiCond.Appearing);
                objectGUILocSet = false;
            }
            

            if (gui.TreeNodeEx("Basic Stuff", ImGuiTreeNodeFlags.DefaultOpen))
            {
                OSE_BasicStuff();
                gui.TreePop();
            }

            gui.Text("");
            gui.Separator();
            gui.Text("");
            if (selectedComponentToAdd != null)
                OSE_AddComponentFlow();
            else if (gui.TreeNodeEx("Add Component", ImGuiTreeNodeFlags.DefaultOpen))
            {
                OSE_AddComponentFlow();
                gui.TreePop();
            }

            gui.Text("");
            gui.Separator();
            gui.Text("");

            OSE_ObjectComponentInspector();

            gui.Text("");
            gui.Separator();
            gui.Text("");

            gui.End();

            if(!open)
            {
                SelectedObject = null;
            }
        }

        private static void OSE_ObjectComponentInspector()
        {
            if (inspectorComponent == null)
            {
                var components = selectedObject.FetchComponents();

                gui.BeginChild("Component List", new System.Numerics.Vector2(0, 200), true);
                foreach (ObjectComponent component in components)
                {
                    if (component is Transform)
                        continue;

                    if (gui.Button(component.GetType().Name))
                    {
                        inspectorComponent = component;
                        inspectorComponentObject = component;
                        object cc = inspectorComponent;
                        ReflectionHelper rhh = new(ref cc);
                        componentValueEdits.Clear();
                        rhh.GetMembers().Foreach(member =>
                        {
                            if (!member.IsValid)
                            {

                            }
                            if (member.HasAttribute<HideAttribute>() || (!member.IsPublic && !member.HasAttribute<ShowAttribute>()))
                                return;
                            var value = member.GetValue(ref cc);
                            componentValueEdits.Add(value?.ToString());
                        });
                    }
                }
                gui.EndChild();
                return;
            }

            if (gui.Button("Back"))
            {
                inspectorComponent = null;
                inspectorComponentObject = null;
                return;
            }

            RenderObjectFields(inspectorComponent);
        }

        private static object RenderObjectFields(object c)
        {
            ReflectionHelper rh = new(ref c);
            var members = rh.GetMembers();

            int valueIndex = 0;
            for (int i = 0; i < members.Count; i++)
            {
                MemberData? member = members[i];
                if(!member.CanWrite)
                    continue;
                if (!member.IsPublic && !member.HasAttribute<ShowAttribute>() || member.HasAttribute<HideAttribute>())
                    continue;

                try
                {
                    if (displayClasses.FirstOrDefault(x => x.DisplayType == member.Type) is EditorDisplay display and not null)
                    {
                        object value = member.GetValue(ref c);

                        display.I_Render(ref value, member, c);
                        continue;
                    }
                    if (member.Type.IsEnum)
                    {
                        EditorEnumDisplay enumDisplay = displayClasses.FirstOrDefault(x => x is EditorEnumDisplay) as EditorEnumDisplay;
                        if (enumDisplay != null)
                        {
                            object value = member.GetValue(ref c);
                            enumDisplay.I_Render(ref value, member, c);
                            continue;
                        }
                    }

                    if (!WinterForge.SupportedPrimitives.Contains(member.Type))
                    {
                        if (member.Type.IsAssignableTo(typeof(ObjectComponent)))
                        {
                            gui.TextColored(new(1, 0, 0, 1), "Unsupported Type.");
                            gui.SameLine();
                            gui.Text(member.Name + ": " + member.GetValue(ref inspectorComponentObject)?.ToString() ?? "null");
                            continue;
                        }
                        if(member.Type == typeof(WorldObject))
                        {
                            WorldObject? obj = (WorldObject)member.GetValue(ref c)!;
                            if (gui.Button(member.Name) && obj != null)
                            {
                                selectedObject = obj;
                            }
                            else
                                gui.Text($"{member.Name} = null");
                        }

                        if(member.Type.IsAssignableTo(typeof(IEnumerable)))
                        {
                            if(!member.Attributes.Any(x => x is SerializeAsAttributeINTERNAL))
                            {
                                gui.Text($"Collection of type '{member.Type.Name}' " +
                                         $"must have custom EditorDisplay implemented");
                                continue;
                            }
                        }

                        if(member.Type.IsAssignableTo(typeof(Delegate)))
                        {
                            gui.TextColored(Color.Red.ToVector4()
                                .ToNumerics(), $"{member.Name}: Cant edit a Delegate type!");
                            continue;
                        }

                        if (gui.TreeNode(member.Name))
                        {
                            object result = RenderObjectFields(member.GetValue(ref c));
                            if(member.Type.IsByRef)
                            {
                                member.SetValue(ref result, c);
                            }
                            if(member.Type.Name.Contains("Nullable`1"))
                            {
                                Type kind = member.Type.GetGenericArguments()[0];
                                var method = 
                                    TypeWorker.FindImplicitConversionMethod(member.Type, kind);
                                object val = method.Invoke(null, [result]);
                                member.SetValue(ref c, val);
                            }
                            gui.TreePop();
                        }
                        
                        continue;
                    }

                    ImGuiInputTextFlags flags = ImGuiInputTextFlags.None;
                    if (!member.CanWrite)
                        flags = ImGuiInputTextFlags.ReadOnly;

                    string strVal = componentValueEdits[valueIndex];

                    gui.InputText(member.Name + $" ({member.Type.Name})", ref strVal, 100, flags);

                    componentValueEdits[valueIndex] = strVal;

                    try
                    {
                        object castedVal = TypeWorker.CastPrimitive(strVal, member.Type);
                        member.SetValue(ref c, castedVal);
                    }
                    catch
                    {
                        gui.TextColored(new(1, 0, 0, 1), "Cant edit this value.");
                    }
                }
                finally
                {
                    valueIndex++;
                }
            }
            return c;
        }

        private static void OSE_AddComponentFlow()
        {
            if (selectedComponentToAdd is not null)
            {
                if (gui.Button("Cancel"))
                {
                    selectedComponentToAdd = null;
                    newComponentInstance = null;
                    newComponentArgs = null;
                    newComponentSelectedConstructor = null;
                    return;
                }

                OSE_AddComponent();
            }
            else
                OSE_ComponentAddingSelection();
        }

        private static void OSE_AddComponent()
        {
            if (newComponentSelectedConstructor is null)
            {
                gui.Text("Select Constructor");

                ConstructorInfo[] constructors = selectedComponentToAdd.GetConstructors();

                for (int i = 0; i < constructors.Length; i++)
                {
                    Style.TextColor = new Color(80, 200, 255, 255);
                    ConstructorInfo constructor = constructors[i];

                    if (gui.Selectable($"{i + 1}. {constructor}"))
                    {
                        newComponentSelectedConstructor = constructor;
                        newComponentArgs = new object[newComponentSelectedConstructor.GetParameters().Length];
                        newComponentArgInputs = new string[newComponentSelectedConstructor.GetParameters().Length];
                        foreach (int j in newComponentArgInputs.Length)
                            newComponentArgInputs[j] = "";
                    }
                    if (gui.IsItemHovered())
                        Style.TextColor = new Color(255, 200, 80, 255);
                }

                Style.TextColor = Color.White;
            }
            else
            {
                bool mayAdd = true;
                gui.Text("Component Arguments");

                ParameterInfo[] parameters = newComponentSelectedConstructor.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo parameter = parameters[i];
                    gui.Text(parameter.Name);

                    string arg = newComponentArgInputs[i];
                    gui.InputText(parameter.Name, ref arg, 100);
                    newComponentArgInputs[i] = arg;

                    try
                    {
                        object argVal = TypeWorker.CastPrimitive(arg, parameter.ParameterType);
                        newComponentArgs[i] = argVal;
                    }
                    catch (Exception e)
                    {
                        gui.TextColored(new(1, 0, 0, 1), "Invalid Argument");
                        mayAdd = false;
                        continue;
                    }


                }

                gui.Separator();

                if (!mayAdd)
                    gui.TextColored(new(1, 0, 0, 1), "Something is preventing you from adding the component! check above!");
                else if (gui.Button("Add Component"))
                {
                    newComponentInstance = (ObjectComponent)newComponentSelectedConstructor.Invoke(newComponentArgs);
                    selectedObject.AttachComponent(newComponentInstance);

                    selectedComponentToAdd = null;
                    newComponentInstance = null;
                    newComponentArgs = null;
                    newComponentSelectedConstructor = null;
                }
            }
        }

        private static void OSE_ComponentAddingSelection()
        {
            gui.InputText("Search Component", ref componentSearch, 100);

            List<Type> filteredComponents = [];
            if (componentSearch == "")
                filteredComponents = allComponents;
            else
            {
                var search = allComponents.SearchMany(componentSearch, type => type.Name, Fuzzy.ComparisonType.IgnoreCase | Fuzzy.ComparisonType.Trim);
                filteredComponents = search.Select(x => x.item).ToList();
            }

            gui.BeginChild("Component List", new System.Numerics.Vector2(0, 200), true);
            foreach (Type type in filteredComponents)
            {
                if (gui.Button(type.Name))
                {
                    selectedComponentToAdd = type;
                }
            }
            gui.EndChild();
        }

        private static string prefabName = "";
        private static void OSE_BasicStuff()
        {
            gui.Text("");
            gui.Separator();
            gui.Text("");

            gui.InputText("Object Name", ref objectName, 100);
            if (objectName != selectedObject.Name && gui.Button("Rename Object"))
            {
                selectedObject.Name = objectName;
            }
            gui.Text($"Position: {selectedObject.transform.position}\n\t(Drag object to move)");

            if(gui.TreeNode("Prefab Creation"))
            {
                gui.InputText("Prefab name", ref prefabName, 100);

                void create()
                {
                    bool allowed = true;
                    if (string.IsNullOrWhiteSpace(prefabName))
                    {
                        Windows.MessageBox("Invalid prefab name!");
                        allowed = false;
                    }
                    if (allowed)
                    {
                        gui.PushID("Prefab name");
                        gui.SetNextItemOpen(false, ImGuiCond.Always);
                        gui.PopID();
                        selectedObject.CreatePrefab(prefabName);
                        if (AssetDatabase.AssetExists(prefabName))
                        {
                            Windows.MessageBox("Prefab created!");
                            selectedObject.IncludeWithSceneSerialization = false;
                        }
                        else
                            Windows.MessageBox("Prefab creation failed!");
                        prefabName = "";
                    }
                }

                if (gui.Button("Create Prefab"))
                {
                    create();
                }
                gui.SameLine();
                if (gui.Button("Create Prefab With Spawner"))
                {
                    Windows.MessageBox("Not implemented!");
                }
                gui.TreePop();
            }

            if (gui.TreeNode("Danger zone"))
            {
                Color tc = Style.TextColor;
                Style.TextColor = Color.Red;
                if (gui.Button("Delete object"))
                    selectedObject.Destroy();
                Style.TextColor = tc;
                gui.TreePop();
            }
        }
    }
}
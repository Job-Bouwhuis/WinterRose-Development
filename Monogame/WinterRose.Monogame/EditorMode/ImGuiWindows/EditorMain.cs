using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.EditorMode.ImGuiWindows;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame.EditorMode
{
    internal static partial class EditorMain
    {
        static WorldObject? selectedObject;
        static WorldObject? hoveredObject;

        static Vector2 lastMousePos;

        internal static WorldObject? SelectedObject
        {
            get => selectedObject;
            set
            {
                selectedObject = value;
                Setup();
            }
        }

        internal static void Update()
        {
            // loop over all objects, and check if the mouse is over them,
            // If the object does not have a collider, use the SpriteRenderer bounds.
            // If it doesnt have a sprite renderer, imagine a 10x10 box around the object where the objetcs transform.position is the center
            // If the object has a collider, use the collider bounds.
            // If the object is clicked, set it as the selected object.

            // then render a context menu with options and information about the object.
            // in the middle of the object, draw a circle around the object with a radius of at least 10 pixels, max 50 pixels depending on the size of the objects bounds.

            // Also when the left mouse button is held down while in this circle, allow the object to be dragged around.

            bool selectedThisFrame = false;
            bool anyHovered = false;
            Vector2 mouseWorldPos = Transform.ScreenToWorldPos(Input.MousePosition, Camera.current);
            bool hoveredUI = gui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow);

            foreach (var obj in Universe.CurrentWorld.Objects)
            {
                // assume a minimum of 10x10 box around the object.
                // if the sprite renderer is enabled, use at minimum the size of the sprite renderer bounds.

                if (obj == Editor.debugObject || obj == Editor.editorCamera.owner)
                    continue;

                Vector2 size = new(10, 10);
                if (obj.HasComponent<SpriteRenderer>())
                {
                    SpriteRenderer sr = obj.FetchComponent<SpriteRenderer>();
                    size = sr.Bounds.Size;
                }

                Vector2 minPos = obj.transform.position - size / 2;
                Vector2 maxPos = obj.transform.position + size / 2;

                // check if mouse is inside the box
                if (mouseWorldPos.X > minPos.X && mouseWorldPos.X < maxPos.X && mouseWorldPos.Y > minPos.Y && mouseWorldPos.Y < maxPos.Y)
                {
                    hoveredObject = obj;
                    anyHovered = true;

                    Input.BlockInputIfUISelected = false;
                    if (!gui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow) && Input.GetMouseDown(MouseButton.Left, true))
                    {
                        selectedObject = obj;
                        selectedThisFrame = true;
                        Setup();
                    }
                    Input.BlockInputIfUISelected = true;
                }
            }

            if (!anyHovered)
                hoveredObject = null;

            if (!selectedThisFrame && Input.GetMouseDown(MouseButton.Left, true))
                selectedObject = null;

            if (selectedObject is not null)
            {
                Vector2 MouseDelta = Input.MousePosition - lastMousePos;

                if (Input.GetMouse(MouseButton.Left))
                {
                    Editor.AllowScrollForSpeed = false;
                    selectedObject.transform.position += MouseDelta;
                }
                else
                    Editor.AllowScrollForSpeed = true;

                if (!gui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow))
                {
                    ScrollScaling();
                    ObjectRotating();
                    ObjectDuplication();
                    ObjectDeletion();
                }
            }

            lastMousePos = Input.MousePosition;
        }

        private static void ObjectDeletion()
        {
            Input.BlockInputIfUISelected = false;
            if(Input.GetKeyDown(Keys.Delete, true))
            {
                if(SelectedObject is not null)
                {
                    SelectedObject.DestroyImmediately();
                    SelectedObject = null;
                }            
            }
            Input.BlockInputIfUISelected = true;
        }

        private static void ObjectDuplication()
        {
            Input.BlockInputIfUISelected = false;
            if(Input.GetKey(Keys.LeftControl, true) && Input.GetKeyDown(Keys.D, true))
            {
                Universe.CurrentWorld.Duplicate(SelectedObject, SelectedObject.Name + " (Copy)");
            }
            Input.BlockInputIfUISelected = true;
        }

        private static void ObjectRotating()
        {
            if (Input.GetKey(Keys.R, true))
            {
                gui.BeginTooltip();
                gui.Text("Move mouse left or right to rotate object.");
                gui.EndTooltip();

                float mouseXDelta = Input.MouseDelta.X;

                SelectedObject.transform.rotation += mouseXDelta;
            }
        }

        private static void ScrollScaling()
        {
            float scrollDelta = Input.MouseScrollDelta;

            Vector2 scalingDirections = new(1, 1);

            if (Input.GetKey(Keys.LeftShift, true))
                scalingDirections.Y = 0;
            else if (Input.GetKey(Keys.LeftControl, true))
                scalingDirections.X = 0;

            selectedObject.transform.scale += scalingDirections * scrollDelta / 10;
            var scale = selectedObject.transform.scale;
            if (scale.X < 0)
                scale.X = 0;
            if (scale.Y < 0)
                scale.Y = 0;

            selectedObject.transform.scale = scale;
        }

        internal static void Render()
        {
            Input.BlockInputIfUISelected = false;
            if (Input.GetMouseDown(MouseButton.Right, true))
            {
                if (!gui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow))
                    EditorContextMenu.Open();
            }
            if (Input.GetKeyDown(Keys.Space, true))
            {
                if (!gui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow))
                    SaveLoad.Open();
            }
            Input.BlockInputIfUISelected = true;


            var batch = MonoUtils.SpriteBatch;

            foreach (var obj in Universe.CurrentWorld.Objects)
            {
                if (obj == Editor.debugObject || obj == Editor.editorCamera.owner)
                    continue;

                Vector2 size = new(10, 10);
                if (obj.HasComponent<SpriteRenderer>())
                {
                    SpriteRenderer sr = obj.FetchComponent<SpriteRenderer>();
                    size = sr.Bounds.Size;
                }

                size *= obj.transform.scale;

                if (obj == selectedObject)
                {
                    batch.DrawBox(obj.transform.position, size, Color.Lime, 3);
                }
                else if (obj == hoveredObject)
                {
                    batch.DrawBox(obj.transform.position, size, Color.Blue, 3);
                }
                else
                {
                    batch.DrawBox(obj.transform.position, size, Color.Magenta, 3);
                }
            }

            if (selectedObject is not null)
            {
                SelectedObjectGUI();
            }

            EditorContextMenu.Render();
            SaveLoad.Render();
        }
    }
}

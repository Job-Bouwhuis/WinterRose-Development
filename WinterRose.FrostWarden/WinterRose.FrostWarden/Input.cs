using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden
{
    using Raylib_cs;

    public static class Input
    {
        private static HashSet<KeyboardKey> keysDown = new();
        private static HashSet<KeyboardKey> keysPressed = new();
        private static HashSet<KeyboardKey> keysReleased = new();

        private static HashSet<MouseButton> mouseButtonsDown = new();
        private static HashSet<MouseButton> mouseButtonsPressed = new();
        private static HashSet<MouseButton> mouseButtonsReleased = new();

        private static int mouseX;
        private static int mouseY;
        private static float scrollWheelDelta;

        // Called by your game loop once per frame to update input states
        internal static void Update()
        {
            keysPressed.Clear();
            keysReleased.Clear();

            mouseButtonsPressed.Clear();
            mouseButtonsReleased.Clear();

            scrollWheelDelta = Raylib.GetMouseWheelMove();

            mouseX = Raylib.GetMouseX();
            mouseY = Raylib.GetMouseY();

            // Update keyboard keys
            foreach (KeyboardKey key in Enum.GetValues(typeof(KeyboardKey)))
            {
                bool isDown = Raylib.IsKeyDown(key);

                if (isDown)
                {
                    if (!keysDown.Contains(key))
                    {
                        keysDown.Add(key);
                        keysPressed.Add(key);
                    }
                }
                else
                {
                    if (keysDown.Contains(key))
                    {
                        keysDown.Remove(key);
                        keysReleased.Add(key);
                    }
                }
            }

            // Update mouse buttons
            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
            {
                bool isDown = Raylib.IsMouseButtonDown(button);

                if (isDown)
                {
                    if (!mouseButtonsDown.Contains(button))
                    {
                        mouseButtonsDown.Add(button);
                        mouseButtonsPressed.Add(button);
                    }
                }
                else
                {
                    if (mouseButtonsDown.Contains(button))
                    {
                        mouseButtonsDown.Remove(button);
                        mouseButtonsReleased.Add(button);
                    }
                }
            }
        }

        // Keyboard methods
        public static bool GetKey(KeyboardKey key) => keysDown.Contains(key);
        public static bool GetKeyDown(KeyboardKey key) => keysPressed.Contains(key);
        public static bool GetKeyUp(KeyboardKey key) => keysReleased.Contains(key);

        // Mouse button methods
        public static bool GetMouseButton(int button) => mouseButtonsDown.Contains((MouseButton)button);
        public static bool GetMouseButtonDown(int button) => mouseButtonsPressed.Contains((MouseButton)button);
        public static bool GetMouseButtonUp(int button) => mouseButtonsReleased.Contains((MouseButton)button);

        // Mouse position
        public static int mousePositionX => mouseX;
        public static int mousePositionY => mouseY;

        // Scroll wheel delta (positive = up, negative = down)
        public static float scrollDelta => scrollWheelDelta;
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.SilkEngine
{
    using Silk.NET.Input;
    using System.Numerics;
    using System.Collections.Generic;

    public static class Input
    {
        private static IInputContext inputContext;
        private static IKeyboard keyboard;
        private static IMouse mouse;

        private static HashSet<Key> keysDown = new();
        private static HashSet<Key> keysUp = new();

        private static HashSet<MouseButton> mouseButtonsDown = new();
        private static HashSet<MouseButton> mouseButtonsUp = new();

        public static Vector2 MousePosition { get; private set; }

        public static void Initialize(IInputContext context)
        {
            inputContext = context;
            keyboard = inputContext.Keyboards[0];
            mouse = inputContext.Mice[0];

            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp += OnKeyUp;

            mouse.MouseDown += OnMouseDown;
            mouse.MouseUp += OnMouseUp;
            mouse.MouseMove += OnMouseMove;
        }

        public static void Update()
        {
            keysDown.Clear();
            keysUp.Clear();

            mouseButtonsDown.Clear();
            mouseButtonsUp.Clear();
        }

        public static bool GetKey(Key key) => keyboard.IsKeyPressed(key);
        public static bool GetKeyDown(Key key) => keysDown.Contains(key);
        public static bool GetKeyUp(Key key) => keysUp.Contains(key);

        public static bool GetMouse(MouseButton button) => mouse.IsButtonPressed(button);
        public static bool GetMouseDown(MouseButton button) => mouseButtonsDown.Contains(button);
        public static bool GetMouseUp(MouseButton button) => mouseButtonsUp.Contains(button);

        private static void OnKeyDown(IKeyboard _, Key key, int scancode) => keysDown.Add(key);
        private static void OnKeyUp(IKeyboard _, Key key, int scancode) => keysUp.Add(key);

        private static void OnMouseDown(IMouse _, MouseButton button) => mouseButtonsDown.Add(button);
        private static void OnMouseUp(IMouse _, MouseButton button) => mouseButtonsUp.Add(button);

        private static void OnMouseMove(IMouse _, Vector2 position) => MousePosition = position;
    }

}

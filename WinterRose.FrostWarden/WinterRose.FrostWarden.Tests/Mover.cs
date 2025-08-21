using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Components;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;

namespace WinterRose.ForgeWarden.Tests
{
    class Mover : Component, IUpdatable
    {
        public float speed = 1000f;

        private float fadeAmount = 0f;
        private const float fadeSpeed = 5.1f;

        ToastStackSide side = ToastStackSide.Bottom;
        ToastStackSide Side
        {
            get
            {
                if(side == ToastStackSide.Bottom)
                    side = ToastStackSide.Top;
                else if(side == ToastStackSide.Top)
                    side = ToastStackSide.Bottom;
                return side;
            }
        }
        public void Update()
        {
            
            Vector2 input = Vector2.Zero;

            if (Input.IsUp(KeyboardKey.F))
            {
                // Neutral
                Toasts.ShowToast(
                    new Toast(ToastType.Neutral, ToastRegion.Right, ToastStackSide.Top)
                        .AddContent("This is a neutral toast prompt")
                        .AddButton("OK", (toast, button) => true));

                // Success
                Toasts.ShowToast(
                    new Toast(ToastType.Success, ToastRegion.Right, ToastStackSide.Top)
                        .AddContent("Operation completed successfully!")
                        .AddButton("Great!", (toast, button) => true));

                // Info
                Toasts.ShowToast(
                    new Toast(ToastType.Info, ToastRegion.Right, ToastStackSide.Top)
                        .AddContent("This is an informational message.")
                        .AddButton("Got it", (toast, button) => true));

                // Warning
                Toasts.ShowToast(
                    new Toast(ToastType.Warning, ToastRegion.Right,ToastStackSide.Bottom)
                        .AddContent("Be careful with this action!")
                        .AddButton("Understood", (toast, button) => true));

                // Error
                Toasts.ShowToast(
                    new Toast(ToastType.Error, ToastRegion.Center, ToastStackSide.Bottom)
                        .AddContent("Something went wrong!")
                        .AddButton("Retry", (toast, button) => true));

                // Fatal
                Toasts.ShowToast(
                    new Toast(ToastType.Fatal, ToastRegion.Center, ToastStackSide.Bottom)
                        .AddContent("Fatal error encountered!")
                        .AddButton("Close", (toast, button) => true));

                // Highlight
                Toasts.ShowToast(
                    new Toast(ToastType.Highlight, ToastRegion.Left, ToastStackSide.Top)
                        .AddContent("Check this important highlight!")
                        .AddButton("Check", (toast, button) => true));

                // Question
                Toasts.ShowToast(
                    new Toast(ToastType.Question, ToastRegion.Left, ToastStackSide.Top)
                        .AddContent("Do you want to proceed?")
                        .AddButton("Yes", (toast, button) => true)
                        .AddButton("No", (toast, button) => true));

                // Critical Action
                Toasts.ShowToast(
                    new Toast(ToastType.CriticalAction, ToastRegion.Center, ToastStackSide.Bottom)
                        .AddContent("This action is irreversible. Continue?")
                        .AddButton("Yes", (toast, button) => true)
                        .AddButton("Cancel", (toast, button) => true));

            }

            if(Input.IsDown(MouseButton.Left))
            {
                
            }

            if (Input.IsDown(KeyboardKey.E))
                transform.rotation -= new Quaternion(0, 0, 1, 0);

            if (Input.IsDown(KeyboardKey.Q))
                transform.rotation += new Quaternion(0, 0, 1, 0);

            if (Input.IsDown(KeyboardKey.W)) input.Y -= 1;
            if (Input.IsDown(KeyboardKey.S)) input.Y += 1;
            if (Input.IsDown(KeyboardKey.A)) input.X -= 1;
            if (Input.IsDown(KeyboardKey.D)) input.X += 1;

            if (input != Vector2.Zero)
            {
                input = Vector2.Normalize(input);
                Vector3 move = new(input.X, input.Y, 0);
                owner.transform.position += move * speed * Raylib.GetFrameTime();
            }
        }
    }
}

using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Components;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes.Enums;
using WinterRose.ForgeWarden.UserInterface.DragDrop;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface;

namespace WinterRose.ForgeWarden.Tests
{
    class Mover : Component, IUpdatable
    {
        public float speed = 1000f;

        private float fadeAmount = 0f;
        private const float fadeSpeed = 5.1f;

        OLEDragDrop dragManager = new OLEDragDrop();
        Toast t;
        protected override void Awake()
        {
            dragManager.OnDragDetected += () =>
            {
                t?.Close();

                t = new Toast(ToastType.Info, ToastRegion.Right, ToastStackSide.Top)
        .AddText("Right?")
        .AddButton("btn", (t, b) => ((Toast)t).OpenAsDialog(
                new Dialog("Horizontal Big",
                    "refer to \\L[https://github.com/Job-Bouwhuis/WinterRose.WinterForge|WinterForge github page] for info",
                    DialogPlacement.RightBig, priority: DialogPriority.High)
                .AddContent(new UIButton("OK", (c, b) =>
                {
                    c.Close();
                }))
                .AddProgressBar(-1)))
        .AddButton("btn2", (t, b) => Toasts.Success("Worked!", ToastRegion.Right, ToastStackSide.Top))
        .AddButton("btn3", (c, b) => Application.Close())
        .AddProgressBar(-1, infiniteSpinText: "Waiting for browser download...")
        //.AddSprite(Assets.Load<Sprite>("bigimg"))
        .AddContent(new HeavyFileDropContent());

                Toasts.ShowToast(t);
            };

            dragManager.OnDragStopped += () =>
            {
                t.Close();
            };

        }


        public void Update()
        {
            Vector2 input = Vector2.Zero;

            

            if (Input.IsUp(KeyboardKey.F))
            {
                // Neutral
                Toasts.ShowToast(
                    new Toast(ToastType.Neutral, ToastRegion.Right, ToastStackSide.Top)
                        .AddText("This is a neutral toast prompt")
                        .AddButton("OK"));

                // Success
                Toasts.ShowToast(
                    new Toast(ToastType.Success, ToastRegion.Right, ToastStackSide.Top)
                        .AddText("Operation completed successfully!")
                        .AddButton("Great!"));

                // Info
                Toasts.ShowToast(
                    new Toast(ToastType.Info, ToastRegion.Right, ToastStackSide.Top)
                        .AddText("This is an informational message.")
                        .AddButton("Got it"));

                // Warning
                Toasts.ShowToast(
                    new Toast(ToastType.Warning, ToastRegion.Right,ToastStackSide.Bottom)
                        .AddText("Be careful with this action!")
                        .AddButton("Understood"));

                // Error
                Toasts.ShowToast(
                    new Toast(ToastType.Error, ToastRegion.Center, ToastStackSide.Bottom)
                        .AddText("Something went wrong!")
                        .AddButton("Retry"));

                // Fatal
                Toasts.ShowToast(
                    new Toast(ToastType.Fatal, ToastRegion.Center, ToastStackSide.Bottom)
                        .AddText("Fatal error encountered!")
                        .AddButton("Close"));

                // Highlight
                Toasts.ShowToast(
                    new Toast(ToastType.Highlight, ToastRegion.Left, ToastStackSide.Top)
                        .AddText("Check this important highlight!")
                        .AddButton("Check"));

                // Question
                Toasts.ShowToast(
                    new Toast(ToastType.Question, ToastRegion.Left, ToastStackSide.Top)
                        .AddText("Do you want to proceed?")
                        .AddButton("Yes")
                        .AddButton("No"));

                // Critical Action
                Toasts.ShowToast(
                    new Toast(ToastType.CriticalAction, ToastRegion.Center, ToastStackSide.Bottom)
                        .AddText("This action is irreversible. Continue?")
                        .AddButton("Yes")
                        .AddButton("Cancel"));

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

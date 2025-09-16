using Raylib_cs;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace WinterRose.ForgeWarden.UserInterface.DialogBoxes.Boxes;
internal class ExceptionDialog : Dialog
{
    public ExceptionDialog(Exception ex, ExceptionDispatchInfo info)
        : base($"\\c[red]A fetal error occured of type \\c[yellow]'{ex.GetType().Name}'\\c[red] and the game did not handle it",
            $"\\c[#FFAAAA]{ex.Message}\n\n\\c[white]StackTrace:\n{ex.StackTrace}",
            DialogPlacement.CenterBig,
            DialogPriority.EngineNotifications)
    {

        AddButton("Ok, and close game");
        if (Debugger.IsAttached)
            AddButton("Throw exception", (container, button) =>
            {
                Application.Current.AllowThrow = true;
                info.Throw();
            });

    }
}
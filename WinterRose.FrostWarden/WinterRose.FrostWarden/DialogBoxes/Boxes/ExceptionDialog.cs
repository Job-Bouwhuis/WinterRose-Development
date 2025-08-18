using Raylib_cs;
using System.Diagnostics;
using WinterRose.ForgeWarden.DialogBoxes;
using WinterRose.ForgeWarden.DialogBoxes.Boxes;

namespace WinterRose.ForgeWarden;
internal class ExceptionDialog : Dialog
{
    public ExceptionDialog(Exception ex)
        : base($"\\c[red]A fetal error occured of type \\c[yellow]'{ex.GetType().Name}'\\c[red] and the game did not handle it",
            $"\\c[#FFAAAA]{ex.Message}\n\n\\c[white]StackTrace:\n{ex.StackTrace}",
            DialogPlacement.CenterBig,
            DialogPriority.EngineNotifications,
            [], [], null)
    {
        Buttons.Add(new DialogButton("Ok, and close game", () =>
        {
            Close();
        }));
        if (Debugger.IsAttached)
            Buttons.Add(new DialogButton("Throw exception", null));

    }

    public override void DrawContent(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth, ref int y)
    {

    }
    public override void Update()
    {

    }
}
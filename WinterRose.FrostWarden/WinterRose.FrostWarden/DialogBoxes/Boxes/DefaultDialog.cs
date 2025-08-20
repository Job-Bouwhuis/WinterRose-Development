using Raylib_cs;
using WinterRose.ForgeWarden.TextRendering;

namespace WinterRose.ForgeWarden.DialogBoxes
{
    // Implement DefaultDialogInstance for dialogs that don't require special behavior
    public class DefaultDialog : Dialog
    {
        public DefaultDialog(
            string title, 
            string message, 
            DialogPlacement placement = DialogPlacement.CenterSmall, 
            DialogPriority priority = DialogPriority.Normal, 
            string[]? buttons = null,
            Func<bool>[]? onButtonClick = null,
            Action<UIContext>? onImGui = null) 
            : base(title, message, placement, priority, buttons, onButtonClick, onImGui)
        {
        }

        public override void DrawContent(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth, ref int y)
        {
            
        }

        public override void Update()
        {
        }
    }
}
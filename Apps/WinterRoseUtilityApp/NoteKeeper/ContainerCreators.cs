using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Windowing;

namespace WinterRoseUtilityApp.NoteKeeper;
internal static class ContainerCreators
{
    public static Toast QuickNote()
    {
        Toast note = new Toast(ToastType.Neutral, ToastRegion.Left, ToastStackSide.Top);
        note.Style.TimeUntilAutoDismiss = 0;

        UITextInput textInput = new UITextInput();
        textInput.MinLines = 8;
        textInput.Placeholder = "Your quick note...";
        textInput.IsMultiline = true;

        UIColumns cols = new UIColumns();
        cols.AddToColumn(0, new UIButton("Discard", (c, b) => c.Close()));
        cols.AddToColumn(1, new UIButton("Save", (c, b) =>
        {
            ((Toast)c).OpenAsDialog(SaveNoteDialog(textInput.Text));
        }));

        note.AddText("Quick note", UIFontSizePreset.Title);
        note.AddContent(textInput);
        note.AddContent(cols);
        return note;
    }

    public static Dialog SaveNoteDialog(string BodyText)
    {
        Dialog d = new Dialog("Alert", DialogPlacement.CenterSmall, DialogPriority.Normal);

        UIColumns primaryCols = new UIColumns();
        d.AddContent(primaryCols);

        UITextInput titleInput = new UITextInput();
        titleInput.MinLines = 1;
        titleInput.AllowMultiline = false;
        titleInput.IsMultiline = false;
        titleInput.Placeholder = "Note title...";
        primaryCols.AddToColumn(0, titleInput);

        UITextInput textInput = new UITextInput();
        textInput.MinLines = 16;
        textInput.Placeholder = "Your quick note...";
        textInput.IsMultiline = true;
        textInput.Text = BodyText;
        primaryCols.AddToColumn(0, textInput);

        UIColumns cols = new UIColumns();
        cols.AddToColumn(0, new UIButton("Discard", (c, b) => c.Close()));
        cols.AddToColumn(1, new UIButton("Save", (c, b) =>
        {
            if(string.IsNullOrWhiteSpace(titleInput.Text))
            {
                Toasts.Error("Note must have a title!");
                titleInput.Focus();
                return;
            }

            Note note = new()
            {
                Title = titleInput.Text,
                Body = textInput.Text,
            };

            new NoteManager().Add(note);
            c.Close();
            Toasts.Success("Note saved!");
        }));
        primaryCols.AddToColumn(0, cols);
        return d;
    }

    internal static UIWindow AllNotes()
    {
        UIWindow window = new UIWindow("All Notes", 700, 1200);
        UIColumns cols = new();
        window.AddContent(cols);

        UIColumns filterCols = new();
        UITextInput filter = new();
        filter.MinLines = 1;
        filter.AllowMultiline = false;
        filter.IsMultiline = false;
        filter.Placeholder = "Note title...";
        filterCols.AddToColumn(0, filter);
        filterCols.AddToColumn(1, new UIButton("Filter", (c, b) => Toasts.Info("working on it...")));

        cols.AddToColumn(0, filterCols);

        var manager = new NoteManager();
        foreach(var note in manager.Notes)
        {
            cols.AddToColumn(1, 
                new UIButton(note.Title, (c, b) =>
                {
                    Toasts.Error("Not implemented", ToastRegion.Center);
                }));
        }
        return window;
    }
}

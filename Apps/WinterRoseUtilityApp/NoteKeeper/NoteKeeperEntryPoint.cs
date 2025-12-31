using Raylib_cs;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using WinterRose;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.Utility;
using WinterRose.Recordium;
using WinterRoseUtilityApp.SubSystems;

namespace WinterRoseUtilityApp.NoteKeeper;

internal class NoteKeeperEntryPoint : SubSystem
{
    private const string QuickNoteHotkey = "QuickNoteHotkey";
    private const string AllNotesHotkey = "AllNotesHotkey";

    public NoteKeeperEntryPoint()
        : base("NoteKeeper",
            "Stores notes and can take temporary notes on toast messages",
            new Version(1, 0, 0))
    {
        // Alt+V for Quick Note
        GlobalHotkey.RegisterHotkey(QuickNoteHotkey, true, HotkeyScancode.LeftAlt, HotkeyScancode.V);

        // Alt+N for All Notes window
        GlobalHotkey.RegisterHotkey(AllNotesHotkey, true, HotkeyScancode.LeftAlt, HotkeyScancode.N);
    }

    public override void Update()
    {
        if(GlobalHotkey.IsTriggered(QuickNoteHotkey))
            ContainerCreators.QuickNote().Show();
        if (GlobalHotkey.IsTriggered(AllNotesHotkey))
            ContainerCreators.AllNotes().Show();
    }
}


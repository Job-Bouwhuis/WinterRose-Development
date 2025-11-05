using Raylib_cs;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using WinterRose;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
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
        GlobalHotkey.RegisterHotkey(QuickNoteHotkey, true, 0x12, 0x56);

        // Alt+N for All Notes window
        GlobalHotkey.RegisterHotkey(AllNotesHotkey, true, 0x12, 0x4E);
    }

    public override void Update()
    {
        if(GlobalHotkey.IsTriggered(QuickNoteHotkey))
            ContainerCreators.QuickNote().Show();
        if (GlobalHotkey.IsTriggered(AllNotesHotkey))
            ContainerCreators.AllNotes().Show();
    }

    public override void Destroy()
    {
        
    }
}


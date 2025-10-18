using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.UserInterface;
internal class FileDropContent : UIContent
{
    private const int WM_DROPFILES = 0x0233;

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern uint DragQueryFile(IntPtr hDrop, uint iFile, StringBuilder lpszFile, int cch);

    [DllImport("shell32.dll")]
    private static extern void DragFinish(IntPtr hDrop);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern void DragAcceptFiles(IntPtr hWnd, bool fAccept);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, WndProcDelegate newProc);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private const int GWLP_WNDPROC = -4;
    private static WndProcDelegate newWndProc;
    private static IntPtr oldWndProc;

    public FileDropContent()
    {
        nint hwnd = Windows.MyHandle.Handle;

        // enable drag & drop for this hwnd
        DragAcceptFiles(hwnd, true);

        // hook the WndProc
        newWndProc = WndProc;
        oldWndProc = SetWindowLongPtr(hwnd, GWLP_WNDPROC, newWndProc);
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_DROPFILES)
        {
            uint fileCount = DragQueryFile(wParam, 0xFFFFFFFF, null, 0);
            List<string> files = new();

            for (uint i = 0; i < fileCount; i++)
            {
                int length = (int)DragQueryFile(wParam, i, null, 0);
                StringBuilder fileName = new(length + 1);

                if (DragQueryFile(wParam, i, fileName, fileName.Capacity) > 0)
                {
                    files.Add(fileName.ToString());
                }
            }

            DragFinish(wParam);

            foreach (var file in files)
            {
                OnFileDropped(file);
            }

            return IntPtr.Zero; // swallow the message
        }

        return CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
    }

    private void OnFileDropped(string file)
    {
        Console.WriteLine("File Dropped: " + file);
    }

    public static void EnableFileDrop(IntPtr hwnd)
    {
        DragAcceptFiles(hwnd, true);
    }

    public static void DisableFileDrop(IntPtr hwnd)
    {
        DragAcceptFiles(hwnd, false);
    }

    public override Vector2 GetSize(Rectangle availableArea)
    {
        return new Vector2(availableArea.Width, GetHeight(availableArea.Width));
    }

    protected override void Draw(Rectangle bounds)
    {
    }

    protected internal override void OnOwnerClosing()
    {
        // the container this content is in is closing
    }

    protected internal override float GetHeight(float maxWidth)
    {
        return 200;
    }
}
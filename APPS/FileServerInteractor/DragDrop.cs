using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using WinterRose;

namespace FileServerInteractor;

public class FileDropTarget : IDropTarget, IClearDisposable
{
    private const string CF_HDROP = "CF_HDROP";
    private static readonly short CF_HDROP_FORMAT = RegisterClipboardFormat(CF_HDROP);

    public bool IsDisposed { get; private set; } = false;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern short RegisterClipboardFormat(string lpszFormat);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern uint DragQueryFile(IntPtr hDrop, uint iFile, StringBuilder lpszFile, int cch);

    [DllImport("ole32.dll", CharSet = CharSet.Auto)]
    private static extern int OleGetClipboard(out IDataObject ppDataObj);

    [DllImport("ole32.dll")]
    static extern int RegisterDragDrop(IntPtr hwnd, IDropTarget pDropTarget);

    [DllImport("ole32.dll")]
    static extern int RevokeDragDrop(IntPtr hwnd);

    [DllImport("ole32.dll")]
    static extern int OleInitialize(IntPtr pvReserved);

    public FileDropTarget()
    {
        // Initialize COM
        OleInitialize(IntPtr.Zero);
        RegisterDragDrop(Windows.MyHandle.Handle, this);
    }

    ~FileDropTarget()
    {
        Dispose();
    }

    public int DragEnter(IDataObject pDataObj, uint grfKeyState, POINTL pt, ref DROPEFFECT pdwEffect)
    {
        if (HasHdropFormat(pDataObj))
        {
            pdwEffect = DROPEFFECT.COPY;
        }
        else
        {
            pdwEffect = DROPEFFECT.NONE;
        }
        return 0; // S_OK
    }

    public int DragOver(uint grfKeyState, POINTL pt, ref DROPEFFECT pdwEffect)
    {
        pdwEffect = DROPEFFECT.COPY;
        return 0; // S_OK
    }

    public int DragLeave()
    {
        return 0; // S_OK
    }

    public int Drop(IDataObject pDataObj, uint grfKeyState, POINTL pt, ref DROPEFFECT pdwEffect)
    {
        if (HasHdropFormat(pDataObj))
        {
            IntPtr hGlobal;
            var format = new FORMATETC { cfFormat = CF_HDROP_FORMAT, dwAspect = DVASPECT.DVASPECT_CONTENT, lindex = -1, tymed = TYMED.TYMED_HGLOBAL };
            pDataObj.GetData(ref format, out STGMEDIUM medium);
            hGlobal = medium.unionmember;

            // Get the file names from the global memory
            uint numFiles = DragQueryFile(hGlobal, 0xFFFFFFFF, null, 0);
            for (uint i = 0; i < numFiles; i++)
            {
                StringBuilder fileName = new StringBuilder(260);
                DragQueryFile(hGlobal, i, fileName, fileName.Capacity);
                Console.WriteLine($"Dropped file: {fileName}");
            }

            return 0; // S_OK
        }
        return 0; // S_OK
    }

    private bool HasHdropFormat(IDataObject pDataObj)
    {
        FORMATETC format = new FORMATETC
        {
            cfFormat = CF_HDROP_FORMAT,
            dwAspect = DVASPECT.DVASPECT_CONTENT,
            lindex = -1,
            tymed = TYMED.TYMED_HGLOBAL
        };

        return pDataObj.QueryGetData(ref format) == 0; // S_OK
    }

    public void Dispose()
    {
        if (!IsDisposed)
        {
            RevokeDragDrop(Windows.MyHandle.Handle);
            IsDisposed = true;

            GC.SuppressFinalize(this);
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct POINTL
{
    public int x;
    public int y;
}

[Flags]
public enum DROPEFFECT : uint
{
    NONE = 0,
    COPY = 1,
    MOVE = 2,
    LINK = 4,
    SCROLL = 0x80000000,
}

[ComImport, Guid("00000122-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IDropTarget
{
    [PreserveSig]
    int DragEnter([In] IDataObject pDataObj, [In] uint grfKeyState, [In] POINTL pt, [In, Out] ref DROPEFFECT pdwEffect);

    [PreserveSig]
    int DragOver([In] uint grfKeyState, [In] POINTL pt, [In, Out] ref DROPEFFECT pdwEffect);

    [PreserveSig]
    int DragLeave();

    [PreserveSig]
    int Drop([In] IDataObject pDataObj, [In] uint grfKeyState, [In] POINTL pt, [In, Out] ref DROPEFFECT pdwEffect);
}
